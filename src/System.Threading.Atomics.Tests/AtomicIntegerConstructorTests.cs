using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Atomics;
using Xunit;
namespace System.Threading.Atomics.Tests
{
    public class AtomicIntegerConstructorTests
    {
        [Fact]
        public void AtomicInteger_MemoryOrder_Should_Fail()
        {
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicInteger(MemoryOrder.Relaxed));
            Assert.Throws<ArgumentException>(() => new AtomicInteger(MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void AtomicInteger_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new AtomicInteger(MemoryOrder.Acquire));
            GC.KeepAlive(new AtomicInteger(MemoryOrder.Release));
            GC.KeepAlive(new AtomicInteger(MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicInteger(MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicInteger_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new AtomicInteger());
        }

        [Fact]
        public void AtomicInteger_InitialValue_With_MemoryOrder_Should_Fail()
        {
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicInteger(int.MaxValue, MemoryOrder.Relaxed));
            Assert.Throws<ArgumentException>(() => new AtomicInteger(int.MaxValue, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void AtomicInteger_InitialValue_With_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new AtomicInteger(int.MaxValue, MemoryOrder.Acquire));
            GC.KeepAlive(new AtomicInteger(int.MaxValue, MemoryOrder.Release));
            GC.KeepAlive(new AtomicInteger(int.MaxValue, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicInteger(int.MaxValue, MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicInteger_InitialValue_With_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new AtomicInteger(int.MaxValue));
        }
    }
}
