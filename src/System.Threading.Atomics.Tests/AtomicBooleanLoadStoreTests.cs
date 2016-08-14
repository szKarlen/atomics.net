using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicBooleanLoadStoreTests
    {
        [Fact]
        public void AtomicBoolean_Load_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(true);
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.Relaxed));
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.Acquire));
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.AcqRel));
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicBoolean_Load_Should_Fail()
        {
            var atomicBoolean = new AtomicBoolean(true);
            Assert.Throws<InvalidOperationException>(() => atomicBoolean.Load(MemoryOrder.Release));
            Assert.Throws<NotSupportedException>(() => atomicBoolean.Load(MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> StoreValues
        {
            get
            {
                yield return new object[] { false, true, MemoryOrder.Release };
                yield return new object[] { false, true, MemoryOrder.AcqRel };
                yield return new object[] { false, true, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("StoreValues")]
        public void AtomicBoolean_Store_Should_Success(bool initialValue, bool storeValue, MemoryOrder order)
        {
            var atomicBoolean = new AtomicBoolean(initialValue, MemoryOrder.Relaxed);
            atomicBoolean.Store(storeValue, order);
            Assert.Equal(storeValue, atomicBoolean.Value);
        }

        [Fact]
        public void AtomicBoolean_Store_Should_Fail()
        {
            var atomicBoolean = new AtomicBoolean(true);
            Assert.Throws<InvalidOperationException>(() => atomicBoolean.Store(false, MemoryOrder.Acquire));
            Assert.Throws<NotSupportedException>(() => atomicBoolean.Store(false, MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> IsLockFreeValues
        {
            get
            {
                yield return new object[] { false, MemoryOrder.AcqRel, true };
                yield return new object[] { false, MemoryOrder.SeqCst, true };
            }
        }

        [Theory]
        [MemberData("IsLockFreeValues")]
        public void AtomicBoolean_IsLockFree_Should_Success(bool initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicBoolean = new AtomicBoolean(initialValue, order);
            Assert.Equal(atomicBoolean.IsLockFree, isLockFree);
        }
    }
}
