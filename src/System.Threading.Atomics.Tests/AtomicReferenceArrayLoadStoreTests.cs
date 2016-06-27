using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicReferenceArrayLoadStoreTests
    {
        [Fact]
        public void AtomicReferenceArray_Load_Should_Success()
        {
            var obj = new object();
            var atomicReferenceArray = new AtomicReferenceArray<object>(new[] { obj });
            Assert.Equal(obj, atomicReferenceArray.Load(0, MemoryOrder.Relaxed));
            Assert.Equal(obj, atomicReferenceArray.Load(0, MemoryOrder.Acquire));
            Assert.Equal(obj, atomicReferenceArray.Load(0, MemoryOrder.AcqRel));
            Assert.Equal(obj, atomicReferenceArray.Load(0, MemoryOrder.SeqCst));
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
        public void AtomicReferenceArray_Load_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var source = Enumerable.Range(0, 3).Select(x => new object()).ToArray();
            var atomicReferenceArray = new AtomicReferenceArray<object>(source.Length, order);

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(null, atomicReferenceArray.Load(i, order));
                atomicReferenceArray.Store(i, source[i], order);
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray.Load(i, order));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicReferenceArray_Load_Acquire_Should_Success(MemoryOrder order)
        {
            var source = Enumerable.Range(0, 3).Select(x => new object()).ToArray();
            var atomicReferenceArray = new AtomicReferenceArray<object>(source.Length, order);

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(null, atomicReferenceArray.Load(i, MemoryOrder.Acquire));
                atomicReferenceArray.Store(i, source[i], MemoryOrder.Release);
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray.Load(i, MemoryOrder.Acquire));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicReferenceArray_Indexer_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var source = Enumerable.Range(0, 3).Select(x => new object()).ToArray();
            var atomicReferenceArray = new AtomicReferenceArray<object>(source.Length, order);

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(null, atomicReferenceArray[i]);
                atomicReferenceArray[i] = source[i];
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray[i]);
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicReferenceArray_Store_MemoryOrder_Should_Success(MemoryOrder order)
        {
            var source = Enumerable.Range(0, 3).Select(x => new object()).ToArray();
            var atomicReferenceArray = new AtomicReferenceArray<object>(source.Length, order);

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                atomicReferenceArray.Store(i, source[i], order);
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray.Load(i, order));
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                atomicReferenceArray.Store(i, source[i], order);
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray.Load(i, order));
            }
        }

        [Theory]
        [MemberData("MemoryOrderValues")]
        public void AtomicReferenceArray_Store_Release_Should_Success(MemoryOrder order)
        {
            var source = Enumerable.Range(0, 3).Select(x => new object()).ToArray();
            var atomicReferenceArray = new AtomicReferenceArray<object>(source.Length, order);

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                atomicReferenceArray.Store(i, source[i], MemoryOrder.Release);
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray.Load(i, MemoryOrder.Acquire));
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                atomicReferenceArray.Store(i, source[i], MemoryOrder.Release);
            }

            for (int i = 0; i < atomicReferenceArray.Count; i++)
            {
                Assert.Equal(source[i], atomicReferenceArray.Load(i, MemoryOrder.Acquire));
            }
        }

        [Fact]
        public void AtomicReferenceArray_Load_Should_Fail()
        {
            var atomicReferenceArray = new AtomicReferenceArray<object>(new []{new object()});
            Assert.Throws<InvalidOperationException>(() => atomicReferenceArray.Load(0, MemoryOrder.Release));
#pragma warning disable 618
            Assert.Throws<NotSupportedException>(() => atomicReferenceArray.Load(0, MemoryOrder.Consume));
#pragma warning restore 618
        }

        public static IEnumerable<object[]> StoreValues
        {
            get
            {
                yield return new object[] { null, new object(), MemoryOrder.Release };
                yield return new object[] { null, new object(), MemoryOrder.AcqRel };
                yield return new object[] { null, new object(), MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("StoreValues")]
        public void AtomicReferenceArray_Store_Should_Success(object initialValue, object storeValue, MemoryOrder order)
        {
            var atomicReferenceArray = new AtomicReferenceArray<object>(new object[1], MemoryOrder.Relaxed);
            atomicReferenceArray.Store(0, storeValue, order);
            Assert.Equal(storeValue, atomicReferenceArray[0]);
        }

        [Fact]
        public void AtomicReferenceArray_Store_Should_Fail()
        {
            var atomicReferenceArray = new AtomicReferenceArray<object>(new object[1]);
            Assert.Throws<InvalidOperationException>(() => atomicReferenceArray.Store(0, new object(), MemoryOrder.Acquire));
#pragma warning disable 618
            Assert.Throws<NotSupportedException>(() => atomicReferenceArray.Store(0, new object(), MemoryOrder.Consume));
#pragma warning restore 618
        }

        public static IEnumerable<object[]> IsLockFreeValues
        {
            get
            {
                yield return new object[] { new object(), MemoryOrder.AcqRel, true };
                yield return new object[] { new object(), MemoryOrder.SeqCst, true };
            }
        }

        [Theory]
        [MemberData("IsLockFreeValues")]
        public void AtomicReferenceArray_IsLockFree_Should_Success(object initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicReferenceArray = new AtomicReferenceArray<object>(new[] { initialValue }, order);
            Assert.Equal(atomicReferenceArray.IsLockFree, isLockFree);
        }
    }
}
