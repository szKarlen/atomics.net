using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// A wrapper for references with atomic operations
    /// </summary>
    /// <typeparam name="T">The underlying reference's type</typeparam>
    [DebuggerDisplay("{Value}")]
#pragma warning disable 0659, 0661
    public sealed class AtomicReference<T> : IEquatable<T>, IEquatable<AtomicReference<T>> where T : class
#pragma warning restore 0659, 0661
    {
        /*
         * volatile is no-op on x86-64
         * used as _ReadWriteBarrier
         */
        private volatile MemoryOrder _order;
        private volatile T _value;

        private volatile object _instanceLock;

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

            if (order == MemoryOrder.SeqCst)
                _instanceLock = new object();

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
                    T currentValue;
                    T tempValue;
                    do
                    {
                        currentValue = _value;
                        tempValue = value;
                    } while (_value != currentValue || Interlocked.CompareExchange(ref _value, tempValue, currentValue) != currentValue);
                }
            }
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        /// <returns>An updated value</returns>
        public T Set(Func<T, T> setter, MemoryOrder order)
        {
            if (order == MemoryOrder.SeqCst)
            {
                lock (_instanceLock)
                {
                    return WriteAcqRel(setter);
                }
            }
            if (order.IsAcquireRelease())
            {
                return WriteAcqRel(setter);
            }
            return null;
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <returns>An updated value</returns>
        public T Set(Func<T, T> setter)
        {
            return Set(setter, this._order);
        }

        private T WriteAcqRel(Func<T, T> setter)
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
