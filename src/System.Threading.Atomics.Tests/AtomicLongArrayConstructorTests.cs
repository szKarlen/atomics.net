using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicLongArrayConstructorTests
    {
        public static IEnumerable<object[]> LengthValues
        {
            get
            {
                yield return new object[] { 0 };
                yield return new object[] { 123 };
                yield return new object[] { 10000 };
            }
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicLongArray_Initial_Length_With_MemoryOrder_Should_Fail(int length)
        {
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(length, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(length, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(length, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicLongArray_Initial_Length_With_MemoryOrder_Should_Success(int length)
        {
            GC.KeepAlive(new AtomicLongArray(length, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicLongArray(length, MemoryOrder.SeqCst));
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicLongArray_With_MemoryOrder_Default_Length_Should_Success(int length)
        {
            GC.KeepAlive(new AtomicLongArray(length));
        }

        public static IEnumerable<object[]> ArrayValues
        {
            get
            {
                yield return new object[] { new long[0] };
                yield return new object[] { new [] { 1L } };
                yield return new object[] { new [] {1L, 2L} };
            }
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicLongArray_With_Source_MemoryOrder_Should_Fail(long[] source)
        {
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(source, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(source, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(source, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicLongArray_With_Source_MemoryOrder_Should_Success(long[] source)
        {
            GC.KeepAlive(new AtomicLongArray(source, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicLongArray(source, MemoryOrder.SeqCst));
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicLongArray_With_Source_Default_MemoryOrder_Should_Success(long[] source)
        {
            GC.KeepAlive(new AtomicLongArray(source));
        }

        public static IEnumerable<object[]> MemoryOrderValues
        {
            get
            {
                yield return new object[] { MemoryOrder.Relaxed };
                yield return new object[] { MemoryOrder.AcqRel };
                yield return new object[] { MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Should_Copy_Source(MemoryOrder memoryOrder)
        {
            var item1 = 1L;
            var item2 = 2L;
            var source = new [] { item1, 0, item2, 0};
            var ar = new AtomicLongArray(source, memoryOrder);
            
            Assert.True(source.SequenceEqual(ar));
            Assert.Equal(0, source[1]);
            Assert.Equal(0, ar[1]);
            
            source[1] = -1;

            Assert.False(source.SequenceEqual(ar));

            Assert.Equal(0, source[3]);
            Assert.Equal(0, ar[3]);
            Assert.Equal(0, source[3]);
            Assert.Equal(0, ar[3]);

            source[3] = -1;

            Assert.Equal(-1, source[3]);
            Assert.Equal(0, ar[3]);
            Assert.False(source.SequenceEqual(ar));
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_With_Null_Source_Should_Fail(MemoryOrder memoryOrder)
        {
            Assert.Throws<ArgumentNullException>(() => new AtomicLongArray(null, memoryOrder));
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Ctor_Should_Track_Length(MemoryOrder order)
        {
            Assert.Throws<ArgumentException>(() => new AtomicLongArray(-1, order));
            GC.KeepAlive(new AtomicLongArray(0, order));
            GC.KeepAlive(new AtomicLongArray(1, order));
        }

        public static IEnumerable<object[]> LengthOrderValues
        {
            get
            {
                yield return new object[] { 0, MemoryOrder.Relaxed };
                yield return new object[] { 0, MemoryOrder.AcqRel };
                yield return new object[] { 0, MemoryOrder.SeqCst };
                yield return new object[] { 123, MemoryOrder.Relaxed };
                yield return new object[] { 123, MemoryOrder.AcqRel };
                yield return new object[] { 123, MemoryOrder.SeqCst };
                yield return new object[] { 10000, MemoryOrder.Relaxed };
                yield return new object[] { 10000, MemoryOrder.AcqRel };
                yield return new object[] { 10000, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("LengthOrderValues")]
        public void AtomicLongArray_Items_With_Length_Ctor_Should_Be_Null(int length, MemoryOrder memoryOrder)
        {
            var ar = new AtomicLongArray(length, memoryOrder);
            foreach (var o in ar)
            {
                Assert.Equal(0, o);
            }

            for (int i = 0; i < ar.Count; i++)
            {
                Assert.Equal(0, ar[i]);
            }
        }
    }
}
