using System.Diagnostics;
using System.Threading.Atomics.Operations;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="int"/> value wrapper with atomic operations
    /// </summary>
    [DebuggerDisplay("{Value}")]
#pragma warning disable 0659, 0661
    public sealed class AtomicInteger : IAtomic<int>, IEquatable<int>, IEquatable<AtomicInteger>
#pragma warning restore 0659, 0661
    {
        private volatile MemoryOrder _order; // making volatile to prohibit reordering in constructors
        private volatile int _value;

        private volatile object _instanceLock;

        /// <summary>
        /// Creates new instance of <see cref="AtomicInteger"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicInteger(MemoryOrder order = MemoryOrder.AcqRel)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            if (order == MemoryOrder.SeqCst)
                _instanceLock = new object();

            _order = order;
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicInteger"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Load operations are always use <see cref="MemoryOrder.Acquire"/> semantics</param>
        public AtomicInteger(int value, MemoryOrder order = MemoryOrder.AcqRel)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            if (order == MemoryOrder.SeqCst)
                _instanceLock = new object();

            _order = order;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        /// <remarks>This method does use CAS approach for value setting. To avoid this use <see cref="Load"/> and <see cref="Store"/> methods pair for get/set operations respectively</remarks>
        public int Value
        {
            get
            {
                if (_order != MemoryOrder.SeqCst) return Volatile.Read(ref _value);

                lock (_instanceLock)
                {
                    return Volatile.Read(ref _value);
                }
            }
            set
            {
                if (_order == MemoryOrder.SeqCst)
                {
                    lock (_instanceLock)
                    {
                        Interlocked.Exchange(ref _value, value); // for processors cache coherence
                    }
                }
                else if (_order.IsAcquireRelease())
                {
                    int currentValue;
                    int tempValue;
                    do
                    {
                        currentValue = _value;
                        tempValue = value;
                    } while (_value != currentValue || Interlocked.CompareExchange(ref _value, tempValue, currentValue) != currentValue);
                }
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
                    this._value = value;
                    break;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    throw new InvalidOperationException("Cannot set (store) value with Acquire semantics");
                case MemoryOrder.Release:
                case MemoryOrder.AcqRel:
                    this._value = value;
                    break;
                case MemoryOrder.SeqCst:
                    lock (_instanceLock)
                    {
                        Interlocked.Exchange(ref _value, value);
                    }
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
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> reads the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public int Load(MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    return Platform.Operations.Read(ref _value);
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    return Platform.Operations.ReadAcquire(ref _value);
                case MemoryOrder.Release:
                    throw new InvalidOperationException("Cannot get (load) value with Release semantics");
                case MemoryOrder.AcqRel:
                    return Platform.Operations.ReadAcquire(ref _value);
                case MemoryOrder.SeqCst:
                    return Platform.Operations.ReadSeqCst(ref _value);
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        public bool IsLockFree
        {
            get { return _order != MemoryOrder.SeqCst || _order.IsAcquireRelease(); }
        }

        /// <summary>
        /// Increments the <see cref="AtomicInteger"/> operand by one.
        /// </summary>
        /// <param name="atomicInteger">The value to increment.</param>
        /// <returns>The value of <paramref name="atomicInteger"/> incremented by 1.</returns>
        public static AtomicInteger operator ++(AtomicInteger atomicInteger)
        {
            return Interlocked.Increment(ref atomicInteger._value);
        }

        /// <summary>
        /// Decrements the <see cref="AtomicInteger"/> operand by one.
        /// </summary>
        /// <param name="atomicInteger">The value to decrement.</param>
        /// <returns>The value of <paramref name="atomicInteger"/> decremented by 1.</returns>
        public static AtomicInteger operator --(AtomicInteger atomicInteger)
        {
            return Interlocked.Decrement(ref atomicInteger._value);
        }

        /// <summary>
        /// Adds specified <paramref name="value"/> to <see cref="AtomicInteger"/> and returns the result as a <see cref="int"/>.
        /// </summary>
        /// <param name="atomicInteger">The <paramref name="atomicInteger"/> used for addition.</param>
        /// <param name="value">The value to add</param>
        /// <returns>The result of adding value to <paramref name="atomicInteger"/></returns>
        public static int operator +(AtomicInteger atomicInteger, int value)
        {
            return Interlocked.Add(ref atomicInteger._value, value);
        }

        /// <summary>
        /// Subtracts <paramref name="value"/> from <paramref name="atomicInteger"/> and returns the result as a <see cref="int"/>.
        /// </summary>
        /// <param name="atomicInteger">The <paramref name="atomicInteger"/> from which <paramref name="value"/> is subtracted.</param>
        /// <param name="value">The value to subtract from <paramref name="atomicInteger"/></param>
        /// <returns>The result of subtracting value from <see cref="AtomicInteger"/></returns>
        public static int operator -(AtomicInteger atomicInteger, int value)
        {
            return Interlocked.Add(ref atomicInteger._value, -value);
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
                    tempValue = Interlocked.CompareExchange(ref atomicInteger._value, result, currentValue);
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
                    tempValue = Interlocked.CompareExchange(ref atomicInteger._value, result, currentValue);
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

        int IAtomicsOperator<int>.CompareExchange(ref int location1, int value, int comparand)
        {
            return Interlocked.CompareExchange(ref location1, value, comparand);
        }

        int IAtomicsOperator<int>.Read(ref int location1)
        {
            return Volatile.Read(ref location1);
        }
        
        bool IAtomicsOperator<int>.Supports<TType>()
        {
            return typeof (TType) == typeof (int);
        }
    }
}