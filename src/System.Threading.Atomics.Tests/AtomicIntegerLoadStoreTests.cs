using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicIntegerLoadStoreTests
    {
        [Fact]
        public void AtomicInteger_Load_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.Relaxed));
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.Acquire));
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.AcqRel));
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicInteger_Load_Should_Fail()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicInteger.Load(MemoryOrder.Release));
            Assert.Throws<NotSupportedException>(() => atomicInteger.Load(MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> StoreValues
        {
            get
            {
                yield return new object[] { int.MinValue, int.MaxValue, MemoryOrder.Release };
                yield return new object[] { int.MinValue, int.MaxValue, MemoryOrder.AcqRel };
                yield return new object[] { int.MinValue, int.MaxValue, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("StoreValues")]
        public void AtomicInteger_Store_Should_Success(int initialValue, int storeValue, MemoryOrder order)
        {
            var atomicInteger = new AtomicInteger(initialValue, order);
            atomicInteger.Store(storeValue, order);
            Assert.Equal(storeValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Store_Should_Fail()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicInteger.Store(int.MinValue, MemoryOrder.Acquire));
            Assert.Throws<NotSupportedException>(() => atomicInteger.Store(int.MinValue, MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> IsLockFreeValues
        {
            get
            {
                yield return new object[] { int.MinValue, MemoryOrder.Acquire, true };
                yield return new object[] { int.MinValue, MemoryOrder.Release, true };
                yield return new object[] { int.MinValue, MemoryOrder.AcqRel, true };
                yield return new object[] { int.MinValue, MemoryOrder.SeqCst, false };
            }
        }

        [Theory]
        [MemberData("IsLockFreeValues")]
        public void AtomicInteger_IsLockFree_Should_Success(int initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicInteger = new AtomicInteger(initialValue, order);
            Assert.Equal(atomicInteger.IsLockFree, isLockFree);
        }
    }
}
