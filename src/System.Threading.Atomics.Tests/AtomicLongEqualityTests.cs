using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicLongEqualityTests
    {
        [Fact]
        public void AtomicLong_Value_Change_Should_Success()
        {
            var AtomicLong = new AtomicLong(long.MaxValue);
            Assert.Equal(long.MaxValue, AtomicLong.Value);

            AtomicLong.Value = long.MinValue;
            Assert.Equal(long.MinValue, AtomicLong.Value);

            AtomicLong.Value = 0;
            Assert.Equal(0, AtomicLong.Value);

            // same value assignment
            AtomicLong.Value = 0;
            Assert.Equal(0, AtomicLong.Value);

            AtomicLong.Value = 123;
            Assert.Equal(123, AtomicLong.Value);
            AtomicLong.Value = 123;
            Assert.Equal(123, AtomicLong.Value);
        }

        [Fact]
        public void AtomicLong_IEquatable_Of_Long_Should_Compare()
        {
            IEquatable<long> firstAtomic = new AtomicLong(long.MaxValue);
            IEquatable<long> secondAtomic = new AtomicLong(long.MinValue);

            Assert.False(firstAtomic.Equals(long.MinValue));
            Assert.False(secondAtomic.Equals(long.MaxValue));
        }

        [Fact]
        public void AtomicLong_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new AtomicLong(long.MaxValue);
            var secondAtomic = new AtomicLong(long.MinValue);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void AtomicLong_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new AtomicLong(long.MaxValue);
            var secondAtomic = firstAtomic;

            firstAtomic = long.MinValue;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }

        [Fact]
        public void AtomicLong_Aligned_Value_Change_Should_Success()
        {
            var AtomicLong = new AtomicLong(long.MaxValue, align:true);
            Assert.Equal(long.MaxValue, AtomicLong.Value);

            AtomicLong.Value = long.MinValue;
            Assert.Equal(long.MinValue, AtomicLong.Value);

            AtomicLong.Value = 0;
            Assert.Equal(0, AtomicLong.Value);

            // same value assignment
            AtomicLong.Value = 0;
            Assert.Equal(0, AtomicLong.Value);

            AtomicLong.Value = 123;
            Assert.Equal(123, AtomicLong.Value);
            AtomicLong.Value = 123;
            Assert.Equal(123, AtomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Aligned_IEquatable_Of_Long_Should_Compare()
        {
            IEquatable<long> firstAtomic = new AtomicLong(long.MaxValue, align: true);
            IEquatable<long> secondAtomic = new AtomicLong(long.MinValue, align: true);

            Assert.False(firstAtomic.Equals(long.MinValue));
            Assert.False(secondAtomic.Equals(long.MaxValue));
        }

        [Fact]
        public void AtomicLong_Aligned_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new AtomicLong(long.MaxValue, align: true);
            var secondAtomic = new AtomicLong(long.MinValue, align: true);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void AtomicLong_Aligned_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new AtomicLong(long.MaxValue, align: true);
            var secondAtomic = firstAtomic;

            firstAtomic = long.MinValue;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }
    }
}
