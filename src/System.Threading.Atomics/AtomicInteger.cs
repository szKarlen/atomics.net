using System.Diagnostics;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="int"/> value wrapper with atomic operations
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicInteger : IAtomic<int>, IEquatable<int>
    {
        private volatile MemoryOrder _order; // making volatile to prohibit reordering in constructors
        private int _value;

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
                    // should use compare_exchange_weak 
                    // but implementing CAS using compare_exchange_strong since we are on .NET
                    int currentValue = this._value;
                    int tempValue;
                    do
                    {
                        tempValue = Interlocked.CompareExchange(ref this._value, value, currentValue);
                    } while (tempValue != currentValue);
                }
            }
        }

        public static AtomicInteger operator ++(AtomicInteger atomicInteger)
        {
            return Interlocked.Increment(ref atomicInteger._value);
        }

        public static AtomicInteger operator --(AtomicInteger atomicInteger)
        {
            return Interlocked.Decrement(ref atomicInteger._value);
        }

        public static int operator +(AtomicInteger atomicInteger, int value)
        {
            return Interlocked.Add(ref atomicInteger._value, value);
        }

        public static int operator -(AtomicInteger atomicInteger, int value)
        {
            return Interlocked.Add(ref atomicInteger._value, -value);
        }

        public static int operator *(AtomicInteger atomicInteger, int value)
        {
            // we do not use C# lock statment to prohibit the use of try/finally, which affects performance
            bool entered = false;
            int currentValue = atomicInteger.Value;

            if (atomicInteger._order == MemoryOrder.SeqCst)
            {
                Monitor.Enter(atomicInteger._instanceLock, ref entered);
            }
            int result = currentValue * value;
            int tempValue;
            do
            {
                tempValue = Interlocked.CompareExchange(ref atomicInteger._value, result, currentValue);
            } while (tempValue != currentValue);

            if (entered)
                Monitor.Exit(atomicInteger._instanceLock);

            return currentValue;
        }

        public static int operator /(AtomicInteger atomicInteger, int value)
        {
            // we do not use C# lock statment to prohibit the use of try/finally, which affects performance
            bool entered = false;
            int currentValue = atomicInteger.Value;

            if (atomicInteger._order == MemoryOrder.SeqCst)
            {
                Monitor.Enter(atomicInteger._instanceLock, ref entered);
            }
            int result = currentValue / value;
            int tempValue;
            do
            {
                tempValue = Interlocked.CompareExchange(ref atomicInteger._value, result, currentValue);
            } while (tempValue != currentValue);

            if (entered)
                Monitor.Exit(atomicInteger._instanceLock);

            return currentValue;
        }

        public static implicit operator int(AtomicInteger atomicInteger)
        {
            return atomicInteger.Value;
        }

        public static implicit operator AtomicInteger(int value)
        {
            return new AtomicInteger(value);
        }

        public static bool operator ==(AtomicInteger x, int y)
        {
            return (x != null && x.Value == y);
        }

        public static bool operator !=(AtomicInteger x, int y)
        {
            return (x != null && x.Value != y);
        }

        bool IEquatable<int>.Equals(int other)
        {
            return this.Value == other;
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
    }
}