using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    /// <summary>
    /// Represents an 32-bit integer aligned alongside of CPU's cache line
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = Platform.CacheLineSize)]
    public unsafe struct Int32Aligned : IEquatable<int>, IEquatable<Int32Aligned>
    {
        [FieldOffset(0)]
        private readonly int _value;

        [FieldOffset(sizeof(int))]
        private fixed byte padding[Platform.CacheLineSize - sizeof(int)];

        /// <summary>
        /// Creates new instance of <see cref="Int32Aligned"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        public Int32Aligned(int value)
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
        public bool Equals(int other)
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
        public bool Equals(Int32Aligned other)
        {
            return this._value == other._value;
        }

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Int32Aligned"/> to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The <see cref="Int32Aligned"/> to convert.</param>
        /// <returns>The converted <see cref="Int32Aligned"/>.</returns>
        public static implicit operator int(Int32Aligned value)
        {
            return value._value;
        }

        /// <summary>
        /// Defines an implicit conversion of a 32-bit signed integer to a <see cref="Int32Aligned"/>.
        /// </summary>
        /// <param name="value">The 32-bit signed integer to convert.</param>
        /// <returns>The converted 32-bit signed integer.</returns>
        public static implicit operator Int32Aligned(int value)
        {
            return new Int32Aligned(value);
        }
    }
}
