using System.Runtime.InteropServices;

namespace System.Threading.Atomics
{
    /// <summary>
    /// Represents an 64-bit integer aligned alongside of CPU's cache line
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Platform.CacheLineSize)]
    public unsafe struct Int64Aligned : IEquatable<long>, IEquatable<Int64Aligned>
    {
        [FieldOffset(0)]
        private readonly long _value;

        [FieldOffset(sizeof(long))]
        private fixed byte padding[Platform.CacheLineSize - sizeof(long)];

        /// <summary>
        /// Creates new instance of <see cref="Int64Aligned"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        public Int64Aligned(long value)
        {
            this._value = value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(long other)
        {
            return this._value == other;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Int64Aligned other)
        {
            return this._value == other._value;
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Int64Aligned"/> to a 64-bit signed integer.
        /// </summary>
        /// <param name="value">The <see cref="Int64Aligned"/> to convert.</param>
        /// <returns>The converted <see cref="Int64Aligned"/>.</returns>
        public static implicit operator long(Int64Aligned value)
        {
            return value._value;
        }

        /// <summary>
        /// Defines an implicit conversion of a 64-bit signed integer to a <see cref="Int64Aligned"/>.
        /// </summary>
        /// <param name="value">The 64-bit signed integer to convert.</param>
        /// <returns>The converted 64-bit signed integer.</returns>
        public static implicit operator Int64Aligned(long value)
        {
            return new Int64Aligned(value);
        }
    }
}