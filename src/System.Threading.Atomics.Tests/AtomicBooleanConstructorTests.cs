using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicBooleanConstructorTests
    {
        [Fact]
        public void AtomicBoolean_MemoryOrder_Should_Fail()
        {
            Assert.Throws<ArgumentException>(() => new AtomicBoolean(MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicBoolean(MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicBoolean(MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void AtomicBoolean_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new AtomicBoolean(MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicBoolean(MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicBoolean_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new AtomicBoolean());
        }

        [Fact]
        public void AtomicBoolean_InitialValue_With_MemoryOrder_Should_Fail()
        {
            Assert.Throws<ArgumentException>(() => new AtomicBoolean(true, MemoryOrder.Acquire));
            Assert.Throws<ArgumentException>(() => new AtomicBoolean(true, MemoryOrder.Release));
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new AtomicBoolean(true, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void AtomicBoolean_InitialValue_With_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new AtomicBoolean(true, MemoryOrder.AcqRel));
            GC.KeepAlive(new AtomicBoolean(true, MemoryOrder.SeqCst));
        }

        [Fact]
        public void AtomicBoolean_InitialValue_With_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new AtomicBoolean(true));
        }
    }
}
