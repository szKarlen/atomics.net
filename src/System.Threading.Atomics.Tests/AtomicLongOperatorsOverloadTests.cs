using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicLongOperatorsOverloadTests
    {
        [Fact]
        public void AtomicLong_Pre_Increment_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue-1);
            ++atomicLong;

            Assert.Equal(long.MaxValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Post_Increment_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue - 1);
            atomicLong++;

            Assert.Equal(long.MaxValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Pre_Decrement_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue);
            --atomicLong;

            Assert.Equal(long.MaxValue-1, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Post_Decrement_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue);
            atomicLong--;

            Assert.Equal(long.MaxValue-1, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Addition_AcqRel_Should_Success()
        {
            const long initialValue = long.MaxValue - 1;
            var atomicLong = new AtomicLong(initialValue);

            var result = atomicLong+1;

            Assert.Equal(long.MaxValue, result);
            Assert.Equal(initialValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Subtraction_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue);

            var result = atomicLong - 1;

            Assert.Equal(long.MaxValue-1, result);
            Assert.Equal(long.MaxValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Multiplication_AcqRel_Should_Success()
        {
            const long initialValue = 123;
            var atomicLong = new AtomicLong(initialValue);

            var result = atomicLong * 2;

            Assert.Equal(123*2, result);
            Assert.Equal(initialValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Division_AcqRel_Should_Success()
        {
            const long initialValue = 256;
            var atomicLong = new AtomicLong(256);

            var result = atomicLong / 2;

            Assert.Equal(256 / 2, result);
            Assert.Equal(initialValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Division_AcqRel_Should_Fail()
        {
            var atomicLong = new AtomicLong(256);

            Assert.Throws<DivideByZeroException>(() => atomicLong/0);
        }

        [Fact]
        public void AtomicLong_Implicit_AcqRel_Should_Success()
        {
            AtomicLong atomicLong = long.MaxValue;

            Assert.NotNull(atomicLong);
            Assert.Equal(long.MaxValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Implicit_Long_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue);

            var func = new Func<long, long>(i => i);

            Assert.Equal(long.MaxValue, func(atomicLong));
        }

        [Fact]
        public void AtomicLong_Equality_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue);

            Assert.True(atomicLong == long.MaxValue);
        }

        [Fact]
        public void AtomicLong_Inequality_AcqRel_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MinValue);

            Assert.True(atomicLong != long.MaxValue);
        }

        // Sequential Consistency mode
        [Fact]
        public void AtomicLong_Pre_Increment_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue - 1, MemoryOrder.SeqCst);
            ++atomicLong;

            Assert.Equal(long.MaxValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Post_Increment_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue - 1, MemoryOrder.SeqCst);
            atomicLong++;

            Assert.Equal(long.MaxValue, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Pre_Decrement_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue, MemoryOrder.SeqCst);
            --atomicLong;

            Assert.Equal(long.MaxValue - 1, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Post_Decrement_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue, MemoryOrder.SeqCst);
            atomicLong--;

            Assert.Equal(long.MaxValue - 1, atomicLong.Value);
        }

        [Fact]
        public void AtomicLong_Addition_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue - 1, MemoryOrder.SeqCst);

            var result = atomicLong + 1;

            Assert.Equal(long.MaxValue, result);
        }

        [Fact]
        public void AtomicLong_Subtraction_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue, MemoryOrder.SeqCst);

            var result = atomicLong - 1;

            Assert.Equal(long.MaxValue - 1, result);
        }

        [Fact]
        public void AtomicLong_Multiplication_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(123, MemoryOrder.SeqCst);

            var result = atomicLong * 2;

            Assert.Equal(123 * 2, result);
        }

        [Fact]
        public void AtomicLong_Division_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(256, MemoryOrder.SeqCst);

            var result = atomicLong / 2;

            Assert.Equal(256 / 2, result);
        }

        [Fact]
        public void AtomicLong_Division_SeqCst_Should_Fail()
        {
            var atomicLong = new AtomicLong(256, MemoryOrder.SeqCst);

            Assert.Throws<DivideByZeroException>(() => atomicLong / 0);
        }

        [Fact]
        public void AtomicLong_Implicit_Long_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue, MemoryOrder.SeqCst);

            var func = new Func<long, long>(i => i);

            Assert.Equal(long.MaxValue, func(atomicLong));
        }

        [Fact]
        public void AtomicLong_Equality_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MaxValue, MemoryOrder.SeqCst);

            Assert.True(atomicLong == long.MaxValue);
        }

        [Fact]
        public void AtomicLong_Inequality_SeqCst_Should_Success()
        {
            var atomicLong = new AtomicLong(long.MinValue, MemoryOrder.SeqCst);

            Assert.True(atomicLong != long.MaxValue);
        }
    }
}
