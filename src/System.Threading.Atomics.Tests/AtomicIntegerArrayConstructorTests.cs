using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicIntegerArrayConstructorTests
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
        public void AtomicIntegerArray_Initial_Length_With_MemoryOrder_Should_Fail(int length)
        {
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(length, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(length, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(length, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicIntegerArray_Initial_Length_With_MemoryOrder_Should_Success(int length)
        {
            GC.KeepAlive(new AtomicIntegerArray(length, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicIntegerArray(length, MemoryOrder.SeqCst));
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicIntegerArray_With_MemoryOrder_Default_Length_Should_Success(int length)
        {
            GC.KeepAlive(new AtomicIntegerArray(length));
        }

        public static IEnumerable<object[]> ArrayValues
        {
            get
            {
                yield return new object[] { new int[0] };
                yield return new object[] { new [] { 1 } };
                yield return new object[] { new [] {1, 2} };
            }
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicIntegerArray_With_Source_MemoryOrder_Should_Fail(int[] source)
        {
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(source, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(source, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(source, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicIntegerArray_With_Source_MemoryOrder_Should_Success(int[] source)
        {
            GC.KeepAlive(new AtomicIntegerArray(source, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicIntegerArray(source, MemoryOrder.SeqCst));
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicIntegerArray_With_Source_Default_MemoryOrder_Should_Success(int[] source)
        {
            GC.KeepAlive(new AtomicIntegerArray(source));
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
        public void AtomicIntegerArray_Should_Copy_Source(MemoryOrder memoryOrder)
        {
            var item1 = 1;
            var item2 = 2;
            var source = new [] { item1, 0, item2, 0};
            var ar = new AtomicIntegerArray(source, memoryOrder);
            
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
        public void AtomicIntegerArray_With_Null_Source_Should_Fail(MemoryOrder memoryOrder)
        {
            Assert.Throws<ArgumentNullException>(() => new AtomicIntegerArray(null, memoryOrder));
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Ctor_Should_Track_Length(MemoryOrder order)
        {
            Assert.Throws<ArgumentException>(() => new AtomicIntegerArray(-1, order));
            GC.KeepAlive(new AtomicIntegerArray(0, order));
            GC.KeepAlive(new AtomicIntegerArray(1, order));
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
        public void AtomicIntegerArray_Items_With_Length_Ctor_Should_Be_Null(int length, MemoryOrder memoryOrder)
        {
            var ar = new AtomicIntegerArray(length, memoryOrder);
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
