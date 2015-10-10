using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicLongLoadStoreTests
    {
        [Fact]
        public void AtomicLong_Load_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue);
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.Relaxed));
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.Acquire));
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.AcqRel));
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicLong_Load_Should_Fail()
        {
            var atomicLong = new AtomicLong(long.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicLong.Load(MemoryOrder.Release));
            Assert.Throws<NotSupportedException>(() => atomicLong.Load(MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> StoreValues
        {
            get
            {
                yield return new object[] { long.MinValue, long.MaxValue, MemoryOrder.Release };
                yield return new object[] { long.MinValue, long.MaxValue, MemoryOrder.AcqRel };
                yield return new object[] { long.MinValue, long.MaxValue, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("StoreValues")]
        public void AtomicLong_Store_Should_Success(long initialValue, long storeValue, MemoryOrder order)
        {
            var atomicLong = new AtomicLong(initialValue, order);
            atomicLong.Store(storeValue, order);
            Assert.Equal(storeValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Store_Should_Fail()
        {
            var atomicLong = new AtomicLong(long.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicLong.Store(long.MinValue, MemoryOrder.Acquire));
            Assert.Throws<NotSupportedException>(() => atomicLong.Store(long.MinValue, MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> IsLockFreeValues
        {
            get
            {
                yield return new object[] { long.MinValue, MemoryOrder.Acquire, true };
                yield return new object[] { long.MinValue, MemoryOrder.Release, true };
                yield return new object[] { long.MinValue, MemoryOrder.AcqRel, true };
                yield return new object[] { long.MinValue, MemoryOrder.SeqCst, true };
            }
        }

        [Theory]
        [MemberData("IsLockFreeValues")]
        public void AtomicLong_IsLockFree_Should_Success(long initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicLong = new AtomicLong(initialValue, order);
            Assert.Equal(atomicLong.IsLockFree, isLockFree);
        }
    }
}
