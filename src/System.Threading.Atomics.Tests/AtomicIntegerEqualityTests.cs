using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicIntegerEqualityTests
    {
        [Fact]
        public void AtomicInteger_Value_Change_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);
            Assert.Equal(int.MaxValue, atomicInteger.Value);

            atomicInteger.Value = int.MinValue;
            Assert.Equal(int.MinValue, atomicInteger.Value);

            atomicInteger.Value = 0;
            Assert.Equal(0, atomicInteger.Value);

            // same value assignment
            atomicInteger.Value = 0;
            Assert.Equal(0, atomicInteger.Value);

            atomicInteger.Value = 123;
            Assert.Equal(123, atomicInteger.Value);
            atomicInteger.Value = 123;
            Assert.Equal(123, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_IEquatable_Of_Int_Should_Compare()
        {
            IEquatable<int> firstAtomic = new AtomicInteger(int.MaxValue);
            IEquatable<int> secondAtomic = new AtomicInteger(int.MinValue);

            Assert.False(firstAtomic.Equals(int.MinValue));
            Assert.False(secondAtomic.Equals(int.MaxValue));
        }

        [Fact]
        public void AtomicInteger_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new AtomicInteger(int.MaxValue);
            var secondAtomic = new AtomicInteger(int.MinValue);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void AtomicInteger_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new AtomicInteger(int.MaxValue);
            var secondAtomic = firstAtomic;

            firstAtomic = int.MinValue;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }

        [Fact]
        public void AtomicInteger_Aligned_Value_Change_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue, align: true);
            Assert.Equal(int.MaxValue, atomicInteger.Value);

            atomicInteger.Value = int.MinValue;
            Assert.Equal(int.MinValue, atomicInteger.Value);

            atomicInteger.Value = 0;
            Assert.Equal(0, atomicInteger.Value);

            // same value assignment
            atomicInteger.Value = 0;
            Assert.Equal(0, atomicInteger.Value);

            atomicInteger.Value = 123;
            Assert.Equal(123, atomicInteger.Value);
            atomicInteger.Value = 123;
            Assert.Equal(123, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Aligned_IEquatable_Of_Int_Should_Compare()
        {
            IEquatable<int> firstAtomic = new AtomicInteger(int.MaxValue, align: true);
            IEquatable<int> secondAtomic = new AtomicInteger(int.MinValue, align: true);

            Assert.False(firstAtomic.Equals(int.MinValue));
            Assert.False(secondAtomic.Equals(int.MaxValue));
        }

        [Fact]
        public void AtomicInteger_Aligned_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new AtomicInteger(int.MaxValue, align: true);
            var secondAtomic = new AtomicInteger(int.MinValue, align: true);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void AtomicInteger_Aligned_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new AtomicInteger(int.MaxValue, align: true);
            var secondAtomic = firstAtomic;

            firstAtomic = int.MinValue;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }
    }
}
