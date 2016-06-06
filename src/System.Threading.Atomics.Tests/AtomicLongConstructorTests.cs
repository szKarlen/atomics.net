using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Atomics;
using Xunit;
namespace System.Threading.Atomics.Tests
{
    public class AtomicLongConstructorTests
    {
        [Fact]
        public void AtomicLong_MemoryOrder_Should_Fail()
        {
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicLong(MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void AtomicLong_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new AtomicLong(MemoryOrder.Acquire));
            GC.KeepAlive(new AtomicLong(MemoryOrder.Release));
            GC.KeepAlive(new AtomicLong(MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicLong(MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicLong_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new AtomicLong());
        }

        [Fact]
        public void AtomicLong_InitialValue_With_MemoryOrder_Should_Fail()
        {
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicLong(long.MaxValue, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void AtomicLong_InitialValue_With_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new AtomicLong(long.MaxValue, MemoryOrder.Acquire));
            GC.KeepAlive(new AtomicLong(long.MaxValue, MemoryOrder.Release));
            GC.KeepAlive(new AtomicLong(long.MaxValue, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicLong(long.MaxValue, MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicLong_InitialValue_With_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new AtomicLong(long.MaxValue));
        }
    }
}
