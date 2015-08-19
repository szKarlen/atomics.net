using System.Diagnostics;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="long"/> value wrapper with atomic operations
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicLong : IAtomic<long>, IEquatable<long>
    {
        private volatile MemoryOrder _order; // making volatile to prohibit reordering in constructors
        private long _value;

        private volatile object _instanceLock;

        /// <summary>
        /// Creates new instance of <see cref="AtomicLong"/>
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicLong(MemoryOrder order = MemoryOrder.AcqRel)
        {
            if (!order.IsSpported()) throw new ArgumentException(string.Format("{0} is not supported", order));

            if (order == MemoryOrder.SeqCst)
                _instanceLock = new object();

            _order = order;
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicLong"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Load operations are always use <see cref="MemoryOrder.Acquire"/> semantics</param>
        public AtomicLong(long value, MemoryOrder order = MemoryOrder.AcqRel)
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
        public long Value
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
                    // should use compare_exchange_weak 
                    // but implementing CAS using compare_exchange_strong since we are on .NET
                    long currentValue = Interlocked.Read(ref _value);
                    long tempValue;
                    do
                    {
                        tempValue = Interlocked.CompareExchange(ref this._value, value, currentValue);
                    } while (tempValue != currentValue);
                }
            }
        }

        public static AtomicLong operator ++(AtomicLong atomicLong)
        {
            return Interlocked.Increment(ref atomicLong._value);
        }

        public static AtomicLong operator --(AtomicLong atomicLong)
        {
            return Interlocked.Decrement(ref atomicLong._value);
        }

        public static long operator +(AtomicLong atomicLong, long value)
        {
            return Interlocked.Add(ref atomicLong._value, value);
        }

        public static long operator -(AtomicLong atomicLong, long value)
        {
            return Interlocked.Add(ref atomicLong._value, -value);
        }

        public static long operator *(AtomicLong atomicLong, long value)
        {
            // we do not use C# lock statment to prohibit the use of try/finally, which affects performance
            bool entered = false;
            long currentValue = atomicLong.Value;

            if (atomicLong._order == MemoryOrder.SeqCst)
            {
                Monitor.Enter(atomicLong._instanceLock, ref entered);
            }
            long result = currentValue * value;
            long tempValue;
            do
            {
                tempValue = Interlocked.CompareExchange(ref atomicLong._value, result, currentValue);
            } while (tempValue != currentValue);

            if (entered)
                Monitor.Exit(atomicLong._instanceLock);

            return result;
        }

        public static long operator /(AtomicLong atomicLong, int value)
        {
            if (value == 0) throw new DivideByZeroException();

            // we do not use C# lock statment to prohibit the use of try/finally, which affects performance
            bool entered = false;
            long currentValue = atomicLong.Value;

            if (atomicLong._order == MemoryOrder.SeqCst)
            {
                Monitor.Enter(atomicLong._instanceLock, ref entered);
            }
            long result = currentValue / value;
            long tempValue;
            do
            {
                tempValue = Interlocked.CompareExchange(ref atomicLong._value, result, currentValue);
            } while (tempValue != currentValue);

            if (entered)
                Monitor.Exit(atomicLong._instanceLock);

            return result;
        }

        public static implicit operator long(AtomicLong atomicLong)
        {
            return atomicLong.Value;
        }

        public static implicit operator AtomicLong(long value)
        {
            return new AtomicLong(value);
        }

        public static bool operator ==(AtomicLong x, long y)
        {
            return (x != null && x.Value == y);
        }

        public static bool operator !=(AtomicLong x, long y)
        {
            return (x != null && x.Value != y);
        }

        bool IEquatable<long>.Equals(long other)
        {
            return this.Value == other;
        }

        long IAtomic<long>.Value
        {
            get { return this.Value; }
            set { this.Value = value; }
        }

        long IAtomicsOperator<long>.CompareExchange(ref long location1, long value, long comparand)
        {
            return Interlocked.CompareExchange(ref location1, value, comparand);
        }

        long IAtomicsOperator<long>.Read(ref long location1)
        {
            return Volatile.Read(ref location1);
        }
    }
}