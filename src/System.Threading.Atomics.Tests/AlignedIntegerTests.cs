using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AlignedIntegerTests
    {
        [Fact]
        public void Int32Aligned_IEquatable_Of_Int_Should_Compare()
        {
            IEquatable<int> firstAtomic = new Int32Aligned(int.MaxValue);
            IEquatable<int> secondAtomic = new Int32Aligned(int.MinValue);

            Assert.False(firstAtomic.Equals(int.MinValue));
            Assert.False(secondAtomic.Equals(int.MaxValue));

            Assert.True(firstAtomic.Equals(int.MaxValue));
            Assert.True(secondAtomic.Equals(int.MinValue));

            Assert.False(firstAtomic.Equals(0));
            Assert.False(secondAtomic.Equals(0));
        }

        [Fact]
        public void Int32Aligned_Comparison_With_Should_Success()
        {
            Int32Aligned alignedValue = 123;
            Assert.Equal(123, (int)alignedValue);
            Assert.True(alignedValue == 123);
        }

        [Fact]
        public void Int64Aligned_IEquatable_Of_Int_Should_Compare()
        {
            IEquatable<long> firstAtomic = new Int64Aligned(long.MaxValue);
            IEquatable<long> secondAtomic = new Int64Aligned(long.MinValue);

            Assert.False(firstAtomic.Equals(long.MinValue));
            Assert.False(secondAtomic.Equals(long.MaxValue));

            Assert.True(firstAtomic.Equals(long.MaxValue));
            Assert.True(secondAtomic.Equals(long.MinValue));

            Assert.False(firstAtomic.Equals(0));
            Assert.False(secondAtomic.Equals(0));
        }

        [Fact]
        public void Int64Aligned_Comparison_With_Should_Success()
        {
            Int64Aligned alignedValue = 123;
            Assert.Equal(123, (long)alignedValue);
            Assert.True(alignedValue == 123);
        }
    }
}
