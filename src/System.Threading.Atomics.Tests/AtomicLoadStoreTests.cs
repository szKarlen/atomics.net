using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicLoadStoreTests
    {
        [Fact]
        public void Atomic_Int_Load_Should_Success()
        {
            var atomicInteger = new Atomic<int>(int.MaxValue);
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.Relaxed));
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.Acquire));
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.AcqRel));
            Assert.Equal(int.MaxValue, atomicInteger.Load(MemoryOrder.SeqCst));
        }

        [Fact]
        public void Atomic_Int_Load_Should_Fail()
        {
            var atomicInteger = new Atomic<int>(int.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicInteger.Load(MemoryOrder.Release));
            Assert.Throws<NotSupportedException>(() => atomicInteger.Load(MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> Int_StoreValues
        {
            get
            {
                yield return new object[] { int.MinValue, int.MaxValue, MemoryOrder.Release };
                yield return new object[] { int.MinValue, int.MaxValue, MemoryOrder.AcqRel };
                yield return new object[] { int.MinValue, int.MaxValue, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("Int_StoreValues")]
        public void Atomic_Int_Store_Should_Success(int initialValue, int storeValue, MemoryOrder order)
        {
            var atomicInteger = new Atomic<int>(initialValue, order);
            atomicInteger.Store(storeValue, order);
            Assert.Equal(storeValue, atomicInteger.Value);
        }

        [Fact]
        public void Atomic_Int_Store_Should_Fail()
        {
            var atomicInteger = new Atomic<int>(int.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicInteger.Store(int.MinValue, MemoryOrder.Acquire));
            Assert.Throws<NotSupportedException>(() => atomicInteger.Store(int.MinValue, MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> Int_IsLockFreeValues
        {
            get
            {
                yield return new object[] { int.MinValue, MemoryOrder.Acquire, true };
                yield return new object[] { int.MinValue, MemoryOrder.Release, true };
                yield return new object[] { int.MinValue, MemoryOrder.AcqRel, true };
                yield return new object[] { int.MinValue, MemoryOrder.SeqCst, true };
            }
        }

        [Theory]
        [MemberData("Int_IsLockFreeValues")]
        public void Atomic_Int_IsLockFree_Should_Success(int initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicInteger = new Atomic<int>(initialValue, order);
            Assert.Equal(atomicInteger.IsLockFree, isLockFree);
        }

        [Fact]
        public void Atomic_Long_Load_Should_Success()
        {
            var atomicLong = new Atomic<long>(long.MaxValue);
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.Relaxed));
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.Acquire));
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.AcqRel));
            Assert.Equal(long.MaxValue, atomicLong.Load(MemoryOrder.SeqCst));
        }

        [Fact]
        public void Atomic_Long_Load_Should_Fail()
        {
            var atomicLong = new Atomic<long>(long.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicLong.Load(MemoryOrder.Release));
            Assert.Throws<NotSupportedException>(() => atomicLong.Load(MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> Long_StoreValues
        {
            get
            {
                yield return new object[] { long.MinValue, long.MaxValue, MemoryOrder.Release };
                yield return new object[] { long.MinValue, long.MaxValue, MemoryOrder.AcqRel };
                yield return new object[] { long.MinValue, long.MaxValue, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("Long_StoreValues")]
        public void Atomic_Long_Store_Should_Success(long initialValue, long storeValue, MemoryOrder order)
        {
            var atomicLong = new Atomic<long>(initialValue, order);
            atomicLong.Store(storeValue, order);
            Assert.Equal(storeValue, atomicLong.Value);
        }

        [Fact]
        public void Atomic_Long_Store_Should_Fail()
        {
            var atomicLong = new Atomic<long>(long.MaxValue);
            Assert.Throws<InvalidOperationException>(() => atomicLong.Store(long.MinValue, MemoryOrder.Acquire));
            Assert.Throws<NotSupportedException>(() => atomicLong.Store(long.MinValue, MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> Long_IsLockFreeValues
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
        [MemberData("Long_IsLockFreeValues")]
        public void Atomic_Long_IsLockFree_Should_Success(long initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicLong = new Atomic<long>(initialValue, order);
            Assert.Equal(atomicLong.IsLockFree, isLockFree);
        }

        [Fact]
        public void Atomic_Bool_Load_Should_Success()
        {
            var atomicBoolean = new Atomic<bool>(true);
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.Relaxed));
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.Acquire));
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.AcqRel));
            Assert.Equal(true, atomicBoolean.Load(MemoryOrder.SeqCst));
        }

        [Fact]
        public void Atomic_Bool_Load_Should_Fail()
        {
            var atomicBoolean = new Atomic<bool>(true);
            Assert.Throws<InvalidOperationException>(() => atomicBoolean.Load(MemoryOrder.Release));
            Assert.Throws<NotSupportedException>(() => atomicBoolean.Load(MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> Bool_StoreValues
        {
            get
            {
                yield return new object[] { false, true, MemoryOrder.Release };
                yield return new object[] { false, true, MemoryOrder.AcqRel };
                yield return new object[] { false, true, MemoryOrder.SeqCst };
            }
        }

        [Theory]
        [MemberData("Bool_StoreValues")]
        public void Atomic_Bool_Store_Should_Success(bool initialValue, bool storeValue, MemoryOrder order)
        {
            var atomicBoolean = new Atomic<bool>(initialValue, order);
            atomicBoolean.Store(storeValue, order);
            Assert.Equal(storeValue, atomicBoolean.Value);
        }

        [Fact]
        public void Atomic_Bool_Store_Should_Fail()
        {
            var atomicBoolean = new Atomic<bool>(true);
            Assert.Throws<InvalidOperationException>(() => atomicBoolean.Store(false, MemoryOrder.Acquire));
            Assert.Throws<NotSupportedException>(() => atomicBoolean.Store(false, MemoryOrder.Consume));
        }

        public static IEnumerable<object[]> Bool_IsLockFreeValues
        {
            get
            {
                yield return new object[] { false, MemoryOrder.Acquire, true };
                yield return new object[] { false, MemoryOrder.Release, true };
                yield return new object[] { false, MemoryOrder.AcqRel, true };
                yield return new object[] { false, MemoryOrder.SeqCst, true };
            }
        }

        [Theory]
        [MemberData("Bool_IsLockFreeValues")]
        public void Atomic_Bool_IsLockFree_Should_Success(bool initialValue, MemoryOrder order, bool isLockFree)
        {
            var atomicBoolean = new Atomic<bool>(initialValue, order);
            Assert.Equal(atomicBoolean.IsLockFree, isLockFree);
        }
    }
}
