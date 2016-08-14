using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicLongArrayLoadStoreTests
    {
        [Fact]
        public void AtomicLongArray_Load_Should_Success()
        {
            var atomicLongArray = new AtomicLongArray(new []{100L});
            Assert.Equal(100, atomicLongArray.Load(0, MemoryOrder.Relaxed));
            Assert.Equal(100, atomicLongArray.Load(0, MemoryOrder.Acquire));
            Assert.Equal(100, atomicLongArray.Load(0, MemoryOrder.AcqRel));
            Assert.Equal(100, atomicLongArray.Load(0, MemoryOrder.SeqCst));
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
        public void AtomicLongArray_Load_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var atomicLongArray = new AtomicLongArray(new long[3], order);

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(0, atomicLongArray.Load(i, order));
                atomicLongArray.Store(i, i, order);
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray.Load(i, order));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Load_Acquire_Should_Success(MemoryOrder order)
        {
            var atomicLongArray = new AtomicLongArray(new long[3], order);

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(0, atomicLongArray.Load(i, MemoryOrder.Acquire));
                atomicLongArray.Store(i, i, MemoryOrder.Release);
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray.Load(i, MemoryOrder.Acquire));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Indexer_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var atomicLongArray = new AtomicLongArray(new long[3], order);

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(0, atomicLongArray[i]);
                atomicLongArray[i] = i;
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray[i]);
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Store_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var atomicLongArray = new AtomicLongArray(new long[3], order);

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                atomicLongArray.Store(i, i, order);
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray.Load(i, order));
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                atomicLongArray.Store(i, i, order);
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray.Load(i, order));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Store_Release_Should_Success(MemoryOrder order)
        {
            var atomicLongArray = new AtomicLongArray(new long[3], order);

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                atomicLongArray.Store(i, i, MemoryOrder.Release);
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray.Load(i, MemoryOrder.Acquire));
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                atomicLongArray.Store(i, i, MemoryOrder.Release);
            }

            for (int i = 0; i < atomicLongArray.Count; i++)
            {
                Assert.Equal(i, atomicLongArray.Load(i, MemoryOrder.Acquire));
            }
        }

        [Fact]
        public void AtomicLongArray_Load_Should_Fail()
        {
            var atomicLongArray = new AtomicLongArray(new []{100L});
            Assert.Throws<InvalidOperationException>(() => atomicLongArray.Load(0, MemoryOrder.Release));
#pragma warning disable 618
            Assert.Throws<NotSupportedException>(() => atomicLongArray.Load(0, MemoryOrder.Consume));
#pragma warning restore 618
        }

        public static IEnumerable<object[]> StoreValues
        {
            get
            {
                yield return new object[] { int.MinValue, 100, MemoryOrder.Release };
                yield return new object[] { int.MinValue, 100, MemoryOrder.AcqRel };
                yield return new object[] { int.MinValue, 100, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("StoreValues")]
        public void AtomicLongArray_Store_Should_Success(int initialValue, int storeValue, MemoryOrder order)
        {
            var atomicLongArray = new AtomicLongArray(new long[] {initialValue}, MemoryOrder.Relaxed);
            atomicLongArray.Store(0, storeValue, order);
            Assert.Equal(storeValue, atomicLongArray[0]);
        }

        [Fact]
        public void AtomicLongArray_Store_Should_Fail()
        {
            var atomicLongArray = new AtomicLongArray(new []{100L});
            Assert.Throws<InvalidOperationException>(() => atomicLongArray.Store(0, int.MinValue, MemoryOrder.Acquire));
#pragma warning disable 618
            Assert.Throws<NotSupportedException>(() => atomicLongArray.Store(0, int.MinValue, MemoryOrder.Consume));
#pragma warning restore 618
        }

        public static IEnumerable<object[]> IsLockFreeValues
        {
            get
            {
                yield return new object[] { int.MinValue, MemoryOrder.AcqRel, true };
                yield return new object[] { int.MinValue, MemoryOrder.SeqCst, true };
            }
        }

        [Theory]
        [MemberData("IsLockFreeValues")]
        public void AtomicLongArray_IsLockFree_Should_Success(long initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicLongArray = new AtomicLongArray(new[] { initialValue }, order);
            Assert.Equal(atomicLongArray.IsLockFree, isLockFree);
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Increment_Should_Success(MemoryOrder memoryOrder)
        {
            var ar = new AtomicLongArray(10, memoryOrder);
            foreach (var o in ar)
            {
                Assert.Equal(0, o);
            }

            for (int i = 0; i < ar.Count; i++)
            {
                Assert.Equal(0, ar[i]);
            }

            for (int i = 0; i < ar.Count; i++)
            {
                ar[i] = i;
                ar.IncrementAt(i);
            }

            Assert.Equal(Enumerable.Range(1, 10).Select(x => (long)x), ar);
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicLongArray_Decrement_Should_Success(MemoryOrder memoryOrder)
        {
            var ar = new AtomicLongArray(10, memoryOrder);
            foreach (var o in ar)
            {
                Assert.Equal(0, o);
            }

            for (int i = 0; i < ar.Count; i++)
            {
                Assert.Equal(0, ar[i]);
            }

            for (int i = 0; i < ar.Count; i++)
            {
                ar[i] = i;
                ar.DecrementAt(i);
            }

            Assert.Equal(Enumerable.Range(-1, 10).Select(x => (long)x), ar);
        }
    }
}
