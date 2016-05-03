using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// A wrapper for structs with atomic operations
    /// </summary>
    /// <typeparam name="T">The underlying struct's type</typeparam>
    [DebuggerDisplay("{Value}")]
    public sealed class Atomic<T> : IAtomicRef<T>, IEquatable<T>, IEquatable<Atomic<T>> where T : struct, IEquatable<T>
    {
        private T _value;
        private static readonly IAtomicOperators<T> Intrinsics = new PrimitiveAtomics() as IAtomicOperators<T>;

        private readonly IAtomicRef<T> _storage;
        
        private readonly MemoryOrder _order;
        private readonly IAtomicOperators<T> _writer;

        /// <summary>
        /// Creates new instance of <see cref="Atomic{T}"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.SeqCst"/> semantics which hurt performance</param>
        /// <param name="align">True to store the underlying value aligned, otherwise False</param>
        public Atomic(MemoryOrder order = MemoryOrder.SeqCst, bool align = false)
            :this (default(T), order, align)
        {
            
        }

        /// <summary>
        /// Creates new instance of <see cref="Atomic{T}"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.SeqCst"/> semantics which hurt performance</param>
        /// <param name="align">True to store the underlying value aligned, otherwise False</param>
        public Atomic(T value, MemoryOrder order = MemoryOrder.SeqCst, bool align = false)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order.ToString()));

            _storage = GetStorage(order, align);

            if (Intrinsics != null && Intrinsics.Supports<T>())
            {
                _writer = Intrinsics;
            }
            else if (_storage as Atomic<T> == this)
            {
                _writer = this;
            }
            else if (_storage.Supports<T>())
            {
                _writer = _storage;
            }
            else
            {
                throw new NotSupportedException(string.Format("{0} type is not supported", typeof(T)));
            }

            _order = order;
            _storage.Value = value;
        }

        private IAtomicRef<T> GetStorage(MemoryOrder order, bool align)
        {
            var type = typeof (T);
            if (type == typeof (bool))
            {
                return (IAtomicRef<T>)(IAtomicRef<bool>)new AtomicBoolean(order, align);
            }
            if (type == typeof(int))
            {
                return (IAtomicRef<T>)(IAtomicRef<int>)new AtomicInteger(order, align);
            }
            if (type == typeof(long))
            {
                return (IAtomicRef<T>)(IAtomicRef<long>)new AtomicLong(order, align);
            }
            if (Pod<T>.AtomicRWSupported() || Pod<T>.Size <= IntPtr.Size)
                return this;
            return new LockBasedAtomic(this);
        }

        /*
         * We use lock(this) to have lower memory footprint
         * Additional instance lock (i.e. object) is redundant
         * LockBasedAtomic is used only as a storage and doesn't get exposed
         */
        class LockBasedAtomic : IAtomicRef<T>
        {
            private readonly IAtomic<T> _atomic;

            public LockBasedAtomic(IAtomic<T> atomic)
            {
                _atomic = atomic;
            }

            T IAtomicOperators<T>.CompareExchange(ref T location1, T value, T comparand)
            {
                lock (this)
                {
                    return _atomic.CompareExchange(ref location1, value, comparand);
                }
            }

            bool IAtomicOperators<T>.Supports<TType>()
            {
                return true;
            }

            public T Value
            {
                get
                {
                    lock (this)
                    {
                        return _atomic.Value;
                    }
                }
                set
                {
                    lock (this)
                    {
                        _atomic.Value = value;
                    }
                }
            }

            public void Store(T value, MemoryOrder order)
            {
                lock (this)
                {
                    _atomic.Store(value, order);
                }
            }

            public T Load(MemoryOrder order)
            {
                lock (this)
                {
                    return _atomic.Load(order);
                }
            }

            public void Store(ref T value, MemoryOrder order)
            {
                lock (this)
                {
                    _atomic.Store(value, order);
                }
            }

            public bool IsLockFree { get { return false; } }

            public T CompareExchange(T value, T comparand)
            {
                return ((IAtomicOperators<T>) this).CompareExchange(ref value, value, comparand);
            }
        }

        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        /// <remarks>This method does use CAS approach for value setting. To avoid this use <see cref="Load"/> and <see cref="Store"/> methods pair for get/set operations respectively</remarks>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _storage.Value; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { _storage.Value = value; }
        }

        T IAtomic<T>.Value
        {
            get { return Load(_order == MemoryOrder.SeqCst ? MemoryOrder.SeqCst : MemoryOrder.Acquire); }
            set
            {
                if (_order == MemoryOrder.SeqCst)
                {
                    Platform.WriteSeqCst(ref _value, ref value);
                    return;
                }

                T currentValue = this._value;
                T tempValue;
                do
                {
                    tempValue = _writer.CompareExchange(ref this._value, value, currentValue);
                } while (!tempValue.Equals(currentValue));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        T IAtomicOperators<T>.CompareExchange(ref T location1, T value, T comparand)
        {
            T temp = location1;
            if (!temp.Equals(comparand))
            {
                Platform.MemoryBarrier();
                location1 = value;
            }

            Platform.MemoryBarrier();
            return temp;
        }

        bool IAtomicOperators<T>.Supports<TType>()
        {
            return true;
        }

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        public bool IsLockFree
        {
            get
            {
                if (object.ReferenceEquals(_storage, this))
                    return Pod<T>.AtomicRWSupported() || Pod<T>.Size <= IntPtr.Size;
                return _storage.IsLockFree;
            }
        }

        /// <summary>
        /// Atomically compares underlying value with <paramref name="comparand"/> for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="value">The value that replaces the underlying value if the comparison results in equality</param>
        /// <param name="comparand">The value that is compared to the underlying value.</param>
        /// <returns>The original underlying value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            return _storage.CompareExchange(value, comparand);
        }
        
        T IAtomic<T>.CompareExchange(T value, T comparand)
        {
            return this._writer.CompareExchange(ref this._value, value, comparand);
        }

        void IAtomic<T>.Store(T value, MemoryOrder order)
        {
            this.Store(ref value, order);
        }

        void IAtomicRef<T>.Store(ref T value, MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    Platform.Write(ref _value, ref value);
                    break;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    throw new InvalidOperationException("Cannot set (store) value with Acquire semantics");
                case MemoryOrder.Release:
                case MemoryOrder.AcqRel:
                    Platform.WriteRelease(ref _value, ref value);
                    break;
                case MemoryOrder.SeqCst:
                    Platform.WriteSeqCst(ref _value, ref value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(ref T value, MemoryOrder order)
        {
            _storage.Store(ref value, order);
        }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T value, MemoryOrder order)
        {
            _storage.Store(value, order);
        }

        T IAtomic<T>.Load(MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    return Platform.Read(ref _value);
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    return Platform.ReadAcquire(ref _value);
                case MemoryOrder.Release:
                    throw new InvalidOperationException("Cannot get (load) value with Release semantics");
                case MemoryOrder.AcqRel:
                    return Platform.ReadAcquire(ref _value);
                case MemoryOrder.SeqCst:
                    return Platform.ReadSeqCst(ref _value);
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load(MemoryOrder order)
        {
            return _storage.Load(order);
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AtomicInteger"/> to a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="atomic">The <see cref="Atomic{T}"/> to convert.</param>
        /// <returns>The converted <see cref="Atomic{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(Atomic<T> atomic)
        {
            return atomic == null ? default(T) : atomic.Value;
        }

        /// <summary>
        /// Defines an implicit conversion of a <typeparamref name="T"/> to a <see cref="AtomicInteger"/>.
        /// </summary>
        /// <param name="value">The <typeparamref name="T"/> to convert.</param>
        /// <returns>The converted <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Atomic<T>(T value)
        {
            return new Atomic<T>(value);
        }

        /// <summary>
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current <see cref="Atomic{T}"/></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="Atomic{T}"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            Atomic<T> other = obj as Atomic<T>;
            if (other == null) return false;

            return object.ReferenceEquals(this, other) || this.Value.Equals(other.Value);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <paramref name="other"/> represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(T other)
        {
            return this.Value.Equals(other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="Atomic{T}"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Atomic<T> other)
        {
            return !object.ReferenceEquals(other, null) && (object.ReferenceEquals(this, other) || this.Value.Equals(other.Value));
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="Atomic{T}"/> and <typeparamref name="T"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="Atomic{T}"/>) to compare.</param>
        /// <param name="y">The second value (<typeparamref name="T"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Atomic<T> x, T y)
        {
            return (!object.ReferenceEquals(x, null) && x.Value.Equals(y));
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="Atomic{T}"/> and <see cref="Atomic{T}"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicInteger"/>) to compare.</param>
        /// <param name="y">The second value (<see cref="int"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Atomic<T> x, Atomic<T> y)
        {
            return (!object.ReferenceEquals(x, null) && x.Equals(y));
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="Atomic{T}"/> and <typeparamref name="T"/> have different values.
        /// </summary>
        /// <param name="x">The first value (<see cref="Atomic{T}"/>) to compare.</param>
        /// <param name="y">The second value (<typeparamref name="T"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Atomic<T> x, T y)
        {
            if (object.ReferenceEquals(x, null))
                return false;

            T value = x.Value;
            return !value.Equals(y);
        }

        public static bool operator !=(Atomic<T> x, Atomic<T> y)
        {
            if (object.ReferenceEquals(x, null))
                return false;

            return !x.Equals(y);
        }

        private class PrimitiveAtomics : IAtomicOperators<int>,
            IAtomicOperators<long>,
            IAtomicOperators<double>,
            IAtomicOperators<float>,
            IAtomicOperators<uint>,
            IAtomicOperators<ulong>,
            IAtomicOperators<sbyte>,
            IAtomicOperators<byte>,
            IAtomicOperators<short>,
            IAtomicOperators<ushort>
        {
            public bool Supports<TType>() where TType : struct
            {
                return this as IAtomicOperators<TType> != null;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            int IAtomicOperators<int>.CompareExchange(ref int location1, int value, int comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            long IAtomicOperators<long>.CompareExchange(ref long location1, long value, long comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            double IAtomicOperators<double>.CompareExchange(ref double location1, double value, double comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            float IAtomicOperators<float>.CompareExchange(ref float location1, float value, float comparand)
            {
                return Interlocked.CompareExchange(ref location1, value, comparand);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe uint IAtomicOperators<uint>.CompareExchange(ref uint location1, uint value, uint comparand)
            {
                fixed (uint* ptr = &location1)
                {
                    var result = Interlocked.CompareExchange(ref *(int*)ptr, *(int*)&value, *(int*)&comparand);
                    return *(uint*)&result;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            unsafe ulong IAtomicOperators<ulong>.CompareExchange(ref ulong location1, ulong value, ulong comparand)
            {
                fixed (ulong* ptr = &location1)
                {
                    var result = Interlocked.CompareExchange(ref *(long*) ptr, *(long*)&value, *(long*)&comparand);
                    return *(ulong*)&result;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            sbyte IAtomicOperators<sbyte>.CompareExchange(ref sbyte location1, sbyte value, sbyte comparand)
            {
                while (true)
                {
                    sbyte temp = location1;
                    int location = temp;
                    int result = Interlocked.CompareExchange(ref location, value, comparand);
                    if (result == location1)
                    {
                        location1 = value;
                        return temp;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            byte IAtomicOperators<byte>.CompareExchange(ref byte location1, byte value, byte comparand)
            {
                while (true)
                {
                    byte temp = location1;
                    int location = temp;
                    int result = Interlocked.CompareExchange(ref location, value, comparand);
                    if (result == location1)
                    {
                        location1 = value;
                        return temp;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            short IAtomicOperators<short>.CompareExchange(ref short location1, short value, short comparand)
            {
                while (true)
                {
                    short temp = location1;
                    int location = temp;
                    int result = Interlocked.CompareExchange(ref location, value, comparand);
                    if (result == location1)
                    {
                        location1 = value;
                        return temp;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            ushort IAtomicOperators<ushort>.CompareExchange(ref ushort location1, ushort value, ushort comparand)
            {
                while (true)
                {
                    ushort temp = location1;
                    int location = temp;
                    int result = Interlocked.CompareExchange(ref location, value, comparand);
                    if (result == location1)
                    {
                        location1 = value;
                        return temp;
                    }
                }
            }
        }
    }
}