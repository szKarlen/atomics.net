using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicIntegerOperatorsOverloadTests
    {
        [Fact]
        public void AtomicInteger_Pre_Increment_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue-1);
            ++atomicInteger;

            Assert.Equal(int.MaxValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Post_Increment_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue - 1);
            atomicInteger++;

            Assert.Equal(int.MaxValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Pre_Decrement_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);
            --atomicInteger;

            Assert.Equal(int.MaxValue-1, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Post_Decrement_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);
            atomicInteger--;

            Assert.Equal(int.MaxValue-1, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Addition_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue - 1);

            var result = atomicInteger+1;

            Assert.Equal(int.MaxValue, result);
        }

        [Fact]
        public void AtomicInteger_Subtraction_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);

            var result = atomicInteger - 1;

            Assert.Equal(int.MaxValue-1, result);
        }

        [Fact]
        public void AtomicInteger_Multiplication_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(123);

            var result = atomicInteger * 2;

            Assert.Equal(123*2, result);
        }

        [Fact]
        public void AtomicInteger_Division_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(256);

            var result = atomicInteger / 2;

            Assert.Equal(256 / 2, result);
        }

        [Fact]
        public void AtomicInteger_Division_AcqRel_Should_Fail()
        {
            var atomicInteger = new AtomicInteger(256);

            Assert.Throws<DivideByZeroException>(() => atomicInteger/0);
        }

        [Fact]
        public void AtomicInteger_Implicit_AcqRel_Should_Success()
        {
            AtomicInteger atomicInteger = int.MaxValue;

            Assert.NotNull(atomicInteger);
            Assert.Equal(int.MaxValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Implicit_Int_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);

            var func = new Func<int, int>(i => i);

            Assert.Equal(int.MaxValue, func(atomicInteger));
        }

        [Fact]
        public void AtomicInteger_Equality_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);

            Assert.True(atomicInteger == int.MaxValue);
        }

        [Fact]
        public void AtomicInteger_Inequality_AcqRel_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MinValue);

            Assert.True(atomicInteger != int.MaxValue);
        }

        // Sequential Consistency mode
        [Fact]
        public void AtomicInteger_Pre_Increment_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue - 1, MemoryOrder.SeqCst);
            ++atomicInteger;

            Assert.Equal(int.MaxValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Post_Increment_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue - 1, MemoryOrder.SeqCst);
            atomicInteger++;

            Assert.Equal(int.MaxValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Pre_Decrement_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue, MemoryOrder.SeqCst);
            --atomicInteger;

            Assert.Equal(int.MaxValue - 1, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Post_Decrement_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue, MemoryOrder.SeqCst);
            atomicInteger--;

            Assert.Equal(int.MaxValue - 1, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Addition_SeqCst_Should_Success()
        {
            const int initialValue = int.MaxValue - 1;
            var atomicInteger = new AtomicInteger(initialValue);

            var result = atomicInteger + 1;

            Assert.Equal(int.MaxValue, result);
            Assert.Equal(initialValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Subtraction_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue);

            var result = atomicInteger - 1;

            Assert.Equal(int.MaxValue - 1, result);
            Assert.Equal(int.MaxValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Multiplication_SeqCst_Should_Success()
        {
            const int initialValue = 123;
            var atomicInteger = new AtomicInteger(initialValue);

            var result = atomicInteger * 2;

            Assert.Equal(initialValue * 2, result);
            Assert.Equal(initialValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Division_SeqCst_Should_Success()
        {
            const int initialValue = 256;
            var atomicInteger = new AtomicInteger(initialValue);

            var result = atomicInteger / 2;

            Assert.Equal(initialValue / 2, result);
            Assert.Equal(initialValue, atomicInteger.Value);
        }

        [Fact]
        public void AtomicInteger_Division_SeqCst_Should_Fail()
        {
            var atomicInteger = new AtomicInteger(256, MemoryOrder.SeqCst);

            Assert.Throws<DivideByZeroException>(() => atomicInteger / 0);
        }

        [Fact]
        public void AtomicInteger_Implicit_Int_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue, MemoryOrder.SeqCst);

            var func = new Func<int, int>(i => i);

            Assert.Equal(int.MaxValue, func(atomicInteger));
        }

        [Fact]
        public void AtomicInteger_Equality_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MaxValue, MemoryOrder.SeqCst);

            Assert.True(atomicInteger == int.MaxValue);
        }

        [Fact]
        public void AtomicInteger_Inequality_SeqCst_Should_Success()
        {
            var atomicInteger = new AtomicInteger(int.MinValue, MemoryOrder.SeqCst);

            Assert.True(atomicInteger != int.MaxValue);
        }
    }
}
