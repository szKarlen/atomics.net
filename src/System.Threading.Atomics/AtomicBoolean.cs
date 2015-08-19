using System.Diagnostics;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="bool"/> value wrapper with atomic access
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicBoolean : IAtomic<bool>, IEquatable<bool>
    {
        private volatile AtomicInteger _storageInteger;

        /// <summary>
        /// Creates new instance of <see cref="AtomicBoolean"/> with default False value
        /// </summary>
        public AtomicBoolean(MemoryOrder order = MemoryOrder.AcqRel)
            : this(false, order)
        {
            
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicBoolean"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        public AtomicBoolean(bool value, MemoryOrder order = MemoryOrder.AcqRel)
        {
            _storageInteger = new AtomicInteger(value ? 1 : 0, order);
        }

        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        public bool Value
        {
            get { return _storageInteger.Value != 0; }
            set { _storageInteger.Value = value ? 1 : 0; }
        }

        public override string ToString()
        {
            return this.Value ? bool.TrueString : bool.FalseString;
        }

        bool IAtomicsOperator<bool>.CompareExchange(ref bool location1, bool value, bool comparand)
        {
            int intLocation = location1.ToInt32();
            int intValue = value.ToInt32();
            int intComparand = comparand.ToInt32();

            return ((IAtomicsOperator<int>) _storageInteger).CompareExchange(ref intLocation, intValue, intComparand) == 0;
        }

        bool IAtomicsOperator<bool>.Read(ref bool location1)
        {
            return Volatile.Read(ref location1);
        }

        bool IEquatable<bool>.Equals(bool other)
        {
            return this.Value == other;
        }
    }
}