using System.Diagnostics;

namespace System.Threading.Atomics
{
    /// <summary>
    /// An <see cref="bool"/> value wrapper with atomic access
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class AtomicBoolean : IAtomicRef<bool>, IEquatable<bool>, IEquatable<AtomicBoolean>
    {
        private readonly AtomicInteger _storage;

        /// <summary>
        /// Creates new instance of <see cref="AtomicBoolean"/> with default False value
        /// </summary>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        /// <param name="align">True to store the underlying value aligned, otherwise False</param>
        public AtomicBoolean(MemoryOrder order = MemoryOrder.SeqCst, bool align = false)
            : this(false, order, align)
        {
            
        }

        /// <summary>
        /// Creates new instance of <see cref="AtomicBoolean"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">Affects the way store operation occur. Default is <see cref="MemoryOrder.AcqRel"/> semantics</param>
        /// <param name="align">True to store the underlying value aligned, otherwise False</param>
        public AtomicBoolean(bool value, MemoryOrder order = MemoryOrder.SeqCst, bool align = false)
        {
            _storage = new AtomicInteger(value ? 1 : 0, order, align);
        }

        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        public bool Value
        {
            get { return _storage.Value != 0; }
            set { _storage.Value = value ? 1 : 0; }
        }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        public void Store(bool value, MemoryOrder order)
        {
            _storage.Store(value ? 1 : 0, order);
        }

        void IAtomicRef<bool>.Store(ref bool value, MemoryOrder order)
        {
            _storage.Store(value ? 1 : 0, order);
        }

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achive</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        public bool Load(MemoryOrder order)
        {
            return _storage.Load(order) != 0;
        }

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        public bool IsLockFree
        {
            get { return _storage.IsLockFree; }
        }

        /// <summary>
        /// Converts the of this instance to its equivalent string representation (either "True" or "False").
        /// </summary>
        /// <returns><see cref="bool.TrueString"/> if the value of this instance is true, or <see cref="bool.FalseString"/> if the value of this instance is false.</returns>
        public override string ToString()
        {
            return this.Value ? bool.TrueString : bool.FalseString;
        }

        bool IAtomicOperators<bool>.CompareExchange(ref bool location1, bool value, bool comparand)
        {
            int intLocation = location1.ToInt32();
            int intValue = value.ToInt32();
            int intComparand = comparand.ToInt32();

            return ((IAtomicOperators<int>) _storage).CompareExchange(ref intLocation, intValue, intComparand) == 0;
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
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current <see cref="AtomicBoolean"/></returns>
        public override int GetHashCode()
        {
            return _storage.GetHashCode();
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

        bool IAtomicOperators<bool>.Supports<TType>()
        {
            return typeof (TType) == typeof (bool);
        }
    }
}