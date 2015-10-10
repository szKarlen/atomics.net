using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="int"/> value wrapper with atomic operations
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicInteger : IAtomicRef<int>, IEquatable<int>, IEquatable<AtomicInteger>
    {
        private MemoryOrder _order; // making volatile to prohibit reordering in constructors
        private readonly BoxedInt32 _storage;

        private readonly object _instanceLock = new object();

        /// <summary>
        /// Creates new instance of <see cref="AtomicInteger"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        /// <param name="align">True to store the underlying value aligned, otherwise False</param>
        public AtomicInteger(MemoryOrder order = MemoryOrder.SeqCst, bool align = false)
            : this(0, order, align)
        {
            
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicInteger"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Load operations are always use <see cref="MemoryOrder.Acquire"/> semantics</param>
        /// <param name="align">True to store the underlying value aligned, otherwise False</param>
        public AtomicInteger(int value, MemoryOrder order = MemoryOrder.SeqCst, bool align = false)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            _order = order;
            this._storage = BoxedInt32.Create(value, align);
        }

        [StructLayout(LayoutKind.Explicit)]
        class BoxedInt32
        {
            private const int CacheLineSize = 64;

            [FieldOffset(0)]
            public int value;

            [FieldOffset(0)]
            public volatile int acqRelValue;

            private BoxedInt32(int value)
            {
                this.value = value;
            }

            [StructLayout(LayoutKind.Explicit)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "_alignedValue"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "pad")]
            class AlignedBoxedInt32 : BoxedInt32
            {
                [FieldOffset(0)]
                public new int value;

                [FieldOffset(sizeof(int))]
                private __32BitAlignedValue _alignedValue;

                public AlignedBoxedInt32(int value) : base(value)
                {
                }
            }

            [StructLayout(LayoutKind.Explicit, Size = CacheLineSize - sizeof(int))]
            unsafe struct __32BitAlignedValue
            {
                [FieldOffset(0)]
                private fixed byte pad[CacheLineSize - sizeof(int)];
            }

            public static BoxedInt32 Create(bool aligned)
            {
                return Create(0, aligned);
            }

            public static BoxedInt32 Create(int value, bool aligned)
            {
                return aligned ? new AlignedBoxedInt32(value) : new BoxedInt32(value);
            }
        }

        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        /// <remarks>This method does use CAS approach for value setting. To avoid this use <see cref="Load"/> and <see cref="Store"/> methods pair for get/set operations respectively</remarks>
        public int Value
        {
            get { return Load(_order == MemoryOrder.SeqCst ? MemoryOrder.SeqCst : MemoryOrder.Acquire); }
            set
            {
                if (_order == MemoryOrder.SeqCst)
                {
                    Interlocked.Exchange(ref _storage.value, value);
                    return;
                }

                int currentValue;
                int tempValue;
                do
                {
                    currentValue = _storage.value;
                    tempValue = value;
                } while (_storage.value != currentValue ||
                         Interlocked.CompareExchange(ref _storage.value, tempValue, currentValue) != currentValue);
            }
        }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public void Store(int value, MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    this._storage.value = value;
                    break;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    throw new InvalidOperationException("Cannot set (store) value with Acquire semantics");
                case MemoryOrder.Release:
                case MemoryOrder.AcqRel:
                    _storage.acqRelValue = value;
                    break;
                case MemoryOrder.SeqCst:
#if ARM_CPU
                    Interlocked.MemoryBarrier();
                     _storage.acqRelValue = value;
#else
                    Interlocked.Exchange(ref _storage.value, value);
#endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        public int Load(MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    return _storage.value;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    return _storage.acqRelValue;
                case MemoryOrder.Release:
                    throw new InvalidOperationException("Cannot get (load) value with Release semantics");
                case MemoryOrder.AcqRel:
                    return _storage.acqRelValue;
                case MemoryOrder.SeqCst:
#if ARM_CPU
                    var tmp = _storage.acqRelValue;
                    Interlocked.MemoryBarrier();
                    return tmp;
#else
                    return _storage.acqRelValue;
#endif
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        public bool IsLockFree
        {
            get { return true; }
        }

        void IAtomicRef<int>.Store(ref int value, MemoryOrder order)
        {
            Store(value, order);
        }

        /// <summary>
        /// Increments the <see cref="AtomicInteger"/> operand by one.
        /// </summary>
        /// <param name="atomicInteger">The value to increment.</param>
        /// <returns>The value of <paramref name="atomicInteger"/> incremented by 1.</returns>
        public static AtomicInteger operator ++(AtomicInteger atomicInteger)
        {
            return Interlocked.Increment(ref atomicInteger._storage.value);
        }

        /// <summary>
        /// Decrements the <see cref="AtomicInteger"/> operand by one.
        /// </summary>
        /// <param name="atomicInteger">The value to decrement.</param>
        /// <returns>The value of <paramref name="atomicInteger"/> decremented by 1.</returns>
        public static AtomicInteger operator --(AtomicInteger atomicInteger)
        {
            return Interlocked.Decrement(ref atomicInteger._storage.value);
        }

        /// <summary>
        /// Adds specified <paramref name="value"/> to <see cref="AtomicInteger"/> and returns the result as a <see cref="int"/>.
        /// </summary>
        /// <param name="atomicInteger">The <paramref name="atomicInteger"/> used for addition.</param>
        /// <param name="value">The value to add</param>
        /// <returns>The result of adding value to <paramref name="atomicInteger"/></returns>
        public static int operator +(AtomicInteger atomicInteger, int value)
        {
            return Interlocked.Add(ref atomicInteger._storage.value, value);
        }

        /// <summary>
        /// Subtracts <paramref name="value"/> from <paramref name="atomicInteger"/> and returns the result as a <see cref="int"/>.
        /// </summary>
        /// <param name="atomicInteger">The <paramref name="atomicInteger"/> from which <paramref name="value"/> is subtracted.</param>
        /// <param name="value">The value to subtract from <paramref name="atomicInteger"/></param>
        /// <returns>The result of subtracting value from <see cref="AtomicInteger"/></returns>
        public static int operator -(AtomicInteger atomicInteger, int value)
        {
            return Interlocked.Add(ref atomicInteger._storage.value, -value);
        }

        /// <summary>
        /// Multiplies <see cref="AtomicInteger"/> by specified <paramref name="value"/> and returns the result as a <see cref="int"/>.
        /// </summary>
        /// <param name="atomicInteger">The <see cref="AtomicInteger"/> to multiply.</param>
        /// <param name="value">The value to multiply</param>
        /// <returns>The result of multiplying <see cref="AtomicInteger"/> and <paramref name="value"/></returns>
        public static int operator *(AtomicInteger atomicInteger, int value)
        {
            bool entered = false;
            int currentValue = atomicInteger.Value;

            if (atomicInteger._order == MemoryOrder.SeqCst)
            {
                Monitor.Enter(atomicInteger._instanceLock, ref entered);
            }

            int result = currentValue * value;
            try
            {
                int tempValue;
                do
                {
                    tempValue = Interlocked.CompareExchange(ref atomicInteger._storage.value, result, currentValue);
                } while (tempValue != currentValue);

            }
            finally
            {
                if (entered)
                    Monitor.Exit(atomicInteger._instanceLock);
            }
            
            return result;
        }

        /// <summary>
        /// Divides the specified <see cref="AtomicInteger"/> by the specified <paramref name="value"/> and returns the resulting as <see cref="int"/>.
        /// </summary>
        /// <param name="atomicInteger">The <see cref="AtomicInteger"/> to divide</param>
        /// <param name="value">The value by which <paramref name="atomicInteger"/> will be divided.</param>
        /// <returns>The result of dividing <paramref name="atomicInteger"/> by <paramref name="value"/>.</returns>
        public static int operator /(AtomicInteger atomicInteger, int value)
        {
            if (value == 0) throw new DivideByZeroException();

            bool entered = false;
            int currentValue = atomicInteger.Value;

            if (atomicInteger._order == MemoryOrder.SeqCst)
            {
                Monitor.Enter(atomicInteger._instanceLock, ref entered);
            }

            int result = currentValue / value;
            try
            {
                int tempValue;
                do
                {
                    tempValue = Interlocked.CompareExchange(ref atomicInteger._storage.value, result, currentValue);
                } while (tempValue != currentValue);
            }
            finally
            {
                if (entered)
                    Monitor.Exit(atomicInteger._instanceLock);
            }
            
            return result;
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AtomicInteger"/> to a 32-bit signed integer.
        /// </summary>
        /// <param name="atomicInteger">The <see cref="AtomicInteger"/> to convert.</param>
        /// <returns>The converted <see cref="AtomicInteger"/>.</returns>
        public static implicit operator int(AtomicInteger atomicInteger)
        {
            return atomicInteger.Value;
        }

        /// <summary>
        /// Defines an implicit conversion of a 32-bit signed integer to a <see cref="AtomicInteger"/>.
        /// </summary>
        /// <param name="value">The 32-bit signed integer to convert.</param>
        /// <returns>The converted 32-bit signed integer.</returns>
        public static implicit operator AtomicInteger(int value)
        {
            return new AtomicInteger(value);
        }

        /// <summary>
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current <see cref="AtomicInteger"/></returns>
        public override int GetHashCode()
        {
            return _instanceLock.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="AtomicInteger"/> and <see cref="int"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicInteger"/>) to compare.</param>
        /// <param name="y">The second value (<see cref="int"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AtomicInteger x, int y)
        {
            return (x != null && x.Value == y);
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="AtomicInteger"/> and <see cref="int"/> have different values.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicInteger"/>) to compare.</param>
        /// <param name="y">The second value (<see cref="int"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AtomicInteger x, int y)
        {
            return (x != null && x.Value != y);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="AtomicInteger"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            AtomicInteger other = obj as AtomicInteger;
            if (other == null) return false;

            return object.ReferenceEquals(this, other) || this.Value == other.Value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <paramref name="other"/> represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(int other)
        {
            return this.Value == other;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AtomicInteger"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AtomicInteger other)
        {
            return other.Value == this.Value;
        }

        int IAtomic<int>.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }

        int IAtomicOperators<int>.CompareExchange(ref int location1, int value, int comparand)
        {
            return Interlocked.CompareExchange(ref location1, value, comparand);
        }

        bool IAtomicOperators<int>.Supports<TType>()
        {
            return typeof (TType) == typeof (int);
        }
    }
}