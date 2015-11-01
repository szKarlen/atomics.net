using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// A wrapper for references with atomic operations
    /// </summary>
    /// <typeparam name="T">The underlying reference's type</typeparam>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicReference<T> : IEquatable<T>, IEquatable<AtomicReference<T>> where T : class
    {
        /*
         * volatile is no-op on x86-64
         * used as _ReadWriteBarrier
         */
        private volatile T _value;
        private readonly MemoryOrder _order;
        
        private readonly object _instanceLock = new object();

        /// <summary>
        /// Creates new instance of <see cref="AtomicLong"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicReference(MemoryOrder order = MemoryOrder.AcqRel)
            : this(null, order)
        {
            
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicLong"/>
        /// </summary>
        /// <param name="initialValue">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Load operations are always use <see cref="MemoryOrder.Acquire"/> semantics</param>
        public AtomicReference(T initialValue, MemoryOrder order = MemoryOrder.AcqRel)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            _order = order;
            this._value = initialValue;
        }

        /// <summary>
        /// Gets or sets atomically the underlying reference
        /// </summary>
        public T Value
        {
            get
            {
                return _order != MemoryOrder.SeqCst ? _value : Volatile.Read(ref _value);
            }
            set
            {
                if (_order == MemoryOrder.SeqCst)
                {
                    Interlocked.Exchange(ref _value, value); // for processors cache coherence
                    return;
                }

                T currentValue;
                T tempValue;
                do
                {
                    currentValue = _value;
                    tempValue = value;
                } while (_value != currentValue || Interlocked.CompareExchange(ref _value, tempValue, currentValue) != currentValue);
            }
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>An updated value</returns>
        public T Set(Func<T, T> setter, MemoryOrder order)
        {
            bool lockTaken = false;
            if (order == MemoryOrder.SeqCst)
                Monitor.Enter(_instanceLock, ref lockTaken);
            try
            {
                T currentValue;
                T tempValue;
                do
                {
                    currentValue = _value;
                    tempValue = setter(currentValue);
                } while (_value != currentValue || Interlocked.CompareExchange(ref _value, tempValue, currentValue) != currentValue);
                return currentValue;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_instanceLock);
            }
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <returns>An updated value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Set(Func<T, T> setter)
        {
            return Set(setter, this._order);
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <param name="data">Any arbitrary value to be passed to <paramref name="setter"/></param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>An updated value</returns>
        public T Set<TData>(Func<T, TData, T> setter, TData data, MemoryOrder order)
        {
            bool lockTaken = false;
            if (order == MemoryOrder.SeqCst)
                Monitor.Enter(_instanceLock, ref lockTaken);
            try
            {
                T currentValue;
                T tempValue;
                do
                {
                    currentValue = _value;
                    tempValue = setter(currentValue, data);
                } while (_value != currentValue || Interlocked.CompareExchange(ref _value, tempValue, currentValue) != currentValue);
                return currentValue;
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(_instanceLock);
            }
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <param name="data">Any arbitrary value to be passed to <paramref name="setter"/></param>
        /// <returns>An updated value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Set<TData>(Func<T, TData, T> setter, TData data)
        {
            return Set(setter, data, this._order);
        }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public void Store(T value, MemoryOrder order)
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
                    // ARM JIT should emit DMB
                    this._value = value;
                    break;
                case MemoryOrder.SeqCst:
                    Interlocked.Exchange(ref _value, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> reads the value as <see cref="MemoryOrder.Acquire"/></remarks>
        public T Load(MemoryOrder order)
        {
            switch (order)
            {
                case MemoryOrder.Relaxed:
                    return this._value;
                case MemoryOrder.Consume:
                    throw new NotSupportedException();
                case MemoryOrder.Acquire:
                    return this._value;
                case MemoryOrder.Release:
                    throw new InvalidOperationException("Cannot get (load) value with Release semantics");
                case MemoryOrder.AcqRel:
                    return this._value;
                case MemoryOrder.SeqCst:
                    return Volatile.Read(ref _value);
                default:
                    throw new ArgumentOutOfRangeException("order");
            }
        }

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        public bool IsLockFree
        {
            get { return _order != MemoryOrder.SeqCst; }
        }

        /// <summary>
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current <see cref="AtomicReference{T}"/></returns>
        public override int GetHashCode()
        {
            return _instanceLock.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="AtomicReference{T}"/> and <typeparamref name="T"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicReference{T}"/>) to compare.</param>
        /// <param name="y">The second value (<typeparamref name="T"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AtomicReference<T> x, T y)
        {
            return (!object.ReferenceEquals(x, null) && x.Value == y);
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="AtomicReference{T}"/> and <typeparamref name="T"/> have different values.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicReference{T}"/>) to compare.</param>
        /// <param name="y">The second value (<typeparamref name="T"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AtomicReference<T> x, T y)
        {
            if (object.ReferenceEquals(x, null))
                return false;

            T value = x.Value;
            return value != y;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="AtomicReference{T}"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            AtomicReference<T> other = obj as AtomicReference<T>;
            if (object.ReferenceEquals(other, null)) return false;

            return object.ReferenceEquals(this, other) || this.Value == other.Value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <paramref name="other"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(T other)
        {
            T value = this.Value;
            return value == other || (other != null && value != null && other.Equals(value));
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AtomicReference{T}"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AtomicReference<T> other)
        {
            T value = this.Value;
            T otherValue = other.Value;
            return value == otherValue || (otherValue != null && value != null && otherValue.Equals(value));
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AtomicReference{T}"/> to a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="atomic">The <see cref="Atomic{T}"/> to convert.</param>
        /// <returns>The converted <see cref="Atomic{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(AtomicReference<T> atomic)
        {
            return atomic.Value;
        }

        /// <summary>
        /// Defines an implicit conversion of a <typeparamref name="T"/> to a <see cref="AtomicReference{T}"/>.
        /// </summary>
        /// <param name="value">The <typeparamref name="T"/> to convert.</param>
        /// <returns>The converted <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtomicReference<T>(T value)
        {
            return new AtomicReference<T>(value);
        }
    }
}
