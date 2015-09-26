using System.Diagnostics;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="bool"/> value wrapper with atomic access
    /// </summary>
    [DebuggerDisplay("{Value}")]
#pragma warning disable 0659, 0661
    public sealed class AtomicBoolean : IAtomic<bool>, IEquatable<bool>, IEquatable<AtomicBoolean>
#pragma warning restore 0659, 0661
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

        public void Set(bool value, MemoryOrder order)
        {
            _storageInteger.Set(value ? 1 : 0, order);
        }

        public bool Load(MemoryOrder order)
        {
            return _storageInteger.Load(order) != 0;
        }

        public bool IsLockFree
        {
            get { return _storageInteger.IsLockFree; }
        }

        /// <summary>
        /// Converts the of this instance to its equivalent string representation (either "True" or "False").
        /// </summary>
        /// <returns><see cref="bool.TrueString"/> if the value of this instance is true, or <see cref="bool.FalseString"/> if the value of this instance is false.</returns>
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

        /// <summary>
        /// Defines an implicit conversion of a <see cref="AtomicBoolean"/> to a boolean.
        /// </summary>
        /// <param name="atomicBoolean">The <see cref="AtomicBoolean"/> to convert.</param>
        /// <returns>The converted <see cref="AtomicBoolean"/>.</returns>
        public static implicit operator bool(AtomicBoolean atomicBoolean)
        {
            return atomicBoolean.Value;
        }

        /// <summary>
        /// Defines an implicit conversion of a boolean to a <see cref="AtomicBoolean"/>.
        /// </summary>
        /// <param name="value">The boolean to convert.</param>
        /// <returns>The converted boolean.</returns>
        public static implicit operator AtomicBoolean(bool value)
        {
            return new AtomicBoolean(value);
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="AtomicBoolean"/> and <see cref="int"/> are equal.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicBoolean"/>) to compare.</param>
        /// <param name="y">The second value (<see cref="int"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(AtomicBoolean x, bool y)
        {
            return (x != null && x.Value == y);
        }

        /// <summary>
        /// Returns a value that indicates whether <see cref="AtomicBoolean"/> and <see cref="int"/> have different values.
        /// </summary>
        /// <param name="x">The first value (<see cref="AtomicBoolean"/>) to compare.</param>
        /// <param name="y">The second value (<see cref="int"/>) to compare.</param>
        /// <returns><c>true</c> if <paramref name="x"/> and <paramref name="y"/> are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(AtomicBoolean x, bool y)
        {
            return (x != null && x.Value != y);
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><c>true</c> if <paramref name="obj"/> is a <see cref="AtomicBoolean"/> and equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            AtomicBoolean other = obj as AtomicBoolean;
            if (other == null) return false;

            return object.ReferenceEquals(this, other) || this.Value == other.Value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified value represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(bool other)
        {
            return this.Value == other;
        }

        /// <summary>
        /// Returns a value indicating whether this instance and a specified <see cref="AtomicBoolean"/> object represent the same value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(AtomicBoolean other)
        {
            return (!ReferenceEquals(other, null) && (ReferenceEquals(this, other) || this.Value == other.Value));
        }

        bool IAtomicsOperator<bool>.Supports<TType>()
        {
            return typeof (TType) == typeof (bool);
        }
    }
}