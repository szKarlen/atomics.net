using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicIntegerArrayLoadStoreTests
    {
        [Fact]
        public void AtomicIntegerArray_Load_Should_Success()
        {
            var atomicIntegerArray = new AtomicIntegerArray(new []{100});
            Assert.Equal(100, atomicIntegerArray.Load(0, MemoryOrder.Relaxed));
            Assert.Equal(100, atomicIntegerArray.Load(0, MemoryOrder.Acquire));
            Assert.Equal(100, atomicIntegerArray.Load(0, MemoryOrder.AcqRel));
            Assert.Equal(100, atomicIntegerArray.Load(0, MemoryOrder.SeqCst));
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
        public void AtomicIntegerArray_Load_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new int[3], order);

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(0, atomicIntegerArray.Load(i, order));
                atomicIntegerArray.Store(i, i, order);
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray.Load(i, order));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Load_Acquire_Should_Success(MemoryOrder order)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new int[3], order);

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(0, atomicIntegerArray.Load(i, MemoryOrder.Acquire));
                atomicIntegerArray.Store(i, i, MemoryOrder.Release);
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray.Load(i, MemoryOrder.Acquire));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Indexer_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new int[3], order);

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(0, atomicIntegerArray[i]);
                atomicIntegerArray[i] = i;
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray[i]);
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Store_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new int[3], order);

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                atomicIntegerArray.Store(i, i, order);
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray.Load(i, order));
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                atomicIntegerArray.Store(i, i, order);
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray.Load(i, order));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Store_Release_Should_Success(MemoryOrder order)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new int[3], order);

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                atomicIntegerArray.Store(i, i, MemoryOrder.Release);
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray.Load(i, MemoryOrder.Acquire));
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                atomicIntegerArray.Store(i, i, MemoryOrder.Release);
            }

            for (int i = 0; i < atomicIntegerArray.Count; i++)
            {
                Assert.Equal(i, atomicIntegerArray.Load(i, MemoryOrder.Acquire));
            }
        }

        [Fact]
        public void AtomicIntegerArray_Load_Should_Fail()
        {
            var atomicIntegerArray = new AtomicIntegerArray(new []{100});
            Assert.Throws<InvalidOperationException>(() => atomicIntegerArray.Load(0, MemoryOrder.Release));
#pragma warning disable 618
            Assert.Throws<NotSupportedException>(() => atomicIntegerArray.Load(0, MemoryOrder.Consume));
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
        public void AtomicIntegerArray_Store_Should_Success(int initialValue, int storeValue, MemoryOrder order)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new int[] {initialValue}, MemoryOrder.Relaxed);
            atomicIntegerArray.Store(0, storeValue, order);
            Assert.Equal(storeValue, atomicIntegerArray[0]);
        }

        [Fact]
        public void AtomicIntegerArray_Store_Should_Fail()
        {
            var atomicIntegerArray = new AtomicIntegerArray(new []{100});
            Assert.Throws<InvalidOperationException>(() => atomicIntegerArray.Store(0, int.MinValue, MemoryOrder.Acquire));
#pragma warning disable 618
            Assert.Throws<NotSupportedException>(() => atomicIntegerArray.Store(0, int.MinValue, MemoryOrder.Consume));
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
        public void AtomicIntegerArray_IsLockFree_Should_Success(int initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicIntegerArray = new AtomicIntegerArray(new[] { initialValue }, order);
            Assert.Equal(atomicIntegerArray.IsLockFree, isLockFree);
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Increment_Should_Success(MemoryOrder memoryOrder)
        {
            var ar = new AtomicIntegerArray(10, memoryOrder);
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

            Assert.Equal(Enumerable.Range(1, 10), ar);
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicIntegerArray_Decrement_Should_Success(MemoryOrder memoryOrder)
        {
            var ar = new AtomicIntegerArray(10, memoryOrder);
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

            Assert.Equal(Enumerable.Range(-1, 10), ar);
        }
    }
}
