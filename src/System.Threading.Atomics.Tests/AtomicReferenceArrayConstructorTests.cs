using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicReferenceArrayConstructorTests
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
        public void AtomicReferenceArray_Initial_Length_With_MemoryOrder_Should_Fail(int length)
        {
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(length, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(length, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(length, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicReferenceArray_Initial_Length_With_MemoryOrder_Should_Success(int length)
        {
            GC.KeepAlive(new AtomicReferenceArray<object>(length, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicReferenceArray<object>(length, MemoryOrder.SeqCst));
        }

        [Theory]
        [MemberData("LengthValues")]
        public void AtomicReferenceArray_With_MemoryOrder_Default_Length_Should_Success(int length)
        {
            GC.KeepAlive(new AtomicReferenceArray<object>(length));
        }

        public static IEnumerable<object[]> ArrayValues
        {
            get
            {
                yield return new object[] { new object[0] };
                yield return new object[] { new object[] {new object() } };
                yield return new object[] { new object[] {new object(), new object()} };
            }
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicReferenceArray_With_Source_MemoryOrder_Should_Fail(object[] source)
        {
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(source, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(source, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(source, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicReferenceArray_With_Source_MemoryOrder_Should_Success(object[] source)
        {
            GC.KeepAlive(new AtomicReferenceArray<object>(source, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicReferenceArray<object>(source, MemoryOrder.SeqCst));
        }

        [Theory]
        [MemberData("ArrayValues")]
        public void AtomicReferenceArray_With_Source_Default_MemoryOrder_Should_Success(object[] source)
        {
            GC.KeepAlive(new AtomicReferenceArray<object>(source));
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
        public void AtomicReferenceArray_Should_Copy_Source(MemoryOrder memoryOrder)
        {
            var item1 = new object();
            var item2 = new object();
            var source = new object[] { item1, null, item2, null};
            var ar = new AtomicReferenceArray<object>(source, memoryOrder);
            
            Assert.True(source.SequenceEqual(ar));
            Assert.Null(source[1]);
            Assert.Null(ar[1]);
            
            source[1] = new object();

            Assert.False(source.SequenceEqual(ar));

            Assert.Null(source[3]);
            Assert.Null(ar[3]);
            Assert.Null(source[3]);
            Assert.Null(ar[3]);

            source[3] = new object();

            Assert.NotNull(source[3]);
            Assert.Null(ar[3]);
            Assert.False(source.SequenceEqual(ar));
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicReferenceArray_With_Null_Source_Should_Fail(MemoryOrder memoryOrder)
        {
            Assert.Throws<ArgumentNullException>(() => new AtomicReferenceArray<object>(null, memoryOrder));
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicReferenceArray_Ctor_Should_Track_Length(MemoryOrder order)
        {
            Assert.Throws<ArgumentException>(() => new AtomicReferenceArray<object>(-1, order));
            GC.KeepAlive(new AtomicReferenceArray<object>(0, order));
            GC.KeepAlive(new AtomicReferenceArray<object>(1, order));
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
        public void AtomicReferenceArray_Items_With_Length_Ctor_Should_Be_Null(int length, MemoryOrder memoryOrder)
        {
            var ar = new AtomicReferenceArray<object>(length, memoryOrder);
            foreach (var o in ar)
            {
                Assert.Null(o);
            }

            for (int i = 0; i < ar.Count; i++)
            {
                Assert.Null(ar[i]);
            }
        }
    }
}
