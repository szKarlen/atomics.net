using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// A wrapper for references with atomic operations
    /// </summary>
    /// <typeparam name="T">The underlying reference's type</typeparam>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicReference<T> : IEquatable<T> where T : class
    {
        private volatile MemoryOrder _order;
        private T _value;

        private volatile object _instanceLock;

        /// <summary>
        /// Creates new instance of <see cref="AtomicLong"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicReference(MemoryOrder order = MemoryOrder.AcqRel)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            if (order == MemoryOrder.SeqCst)
                _instanceLock = new object();

            _order = order;
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
            this.Value = initialValue;
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
                    T currentValue = this._value;
                    T tempValue;
                    do
                    {
                        tempValue = Interlocked.CompareExchange(ref this._value, value, currentValue);
                    } while (tempValue != currentValue);
                }
            }
        }

        /// <summary>
        /// Sets atomically current <see cref="Value"/> by provided setter method
        /// </summary>
        /// <param name="setter">The setter to use</param>
        /// <returns>An updated value</returns>
        public T Set(Func<T, T> setter)
        {
            if (_order == MemoryOrder.SeqCst)
            {
                lock (_instanceLock)
                {
                    return WriteAcqRel(setter);
                }
            }
            if (_order.IsAcquireRelease())
            {
                return WriteAcqRel(setter);
            }
            return null;
        }

        private T WriteAcqRel(Func<T, T> setter)
        {
            T currentValue = _value;
            T tempValue = null;
            while (tempValue != currentValue)
            {
                tempValue = Interlocked.CompareExchange(ref _value, setter(currentValue), currentValue);

                currentValue = tempValue;
            } 
            return currentValue;
        }

        public static bool operator ==(AtomicReference<T> x, T y)
        {
            return (!object.ReferenceEquals(x, null) && x.Value == y);
        }

        public static bool operator !=(AtomicReference<T> x, T y)
        {
            if (object.ReferenceEquals(x, null))
                return false;

            T value = x.Value;
            return value != y;
        }

        bool IEquatable<T>.Equals(T other)
        {
            T value = this.Value;
            return value == other || (other != null && value != null && other.Equals(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(AtomicReference<T> atomic)
        {
            return atomic.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator AtomicReference<T>(T value)
        {
            return new AtomicReference<T>(value);
        }
    }
}
