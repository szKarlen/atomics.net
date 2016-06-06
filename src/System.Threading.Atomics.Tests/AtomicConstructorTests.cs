using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicConstructorTests
    {
        private static readonly Guid SessionValue = Guid.NewGuid();

        [Fact]
        public void Atomic_MemoryOrder_Should_Fail()
        {
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new Atomic<Guid>(MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void Atomic_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new Atomic<Guid>(MemoryOrder.Acquire));
            GC.KeepAlive(new Atomic<Guid>(MemoryOrder.Release));
            GC.KeepAlive(new Atomic<Guid>(MemoryOrder.AcqRel));
            GC.KeepAlive(new Atomic<Guid>(MemoryOrder.SeqCst));
        }

        [Fact]
        public void Atomic_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new Atomic<Guid>());
        }

        [Fact]
        public void Atomic_InitialValue_With_MemoryOrder_Should_Fail()
        {
#pragma warning disable 612, 618
            Assert.Throws<ArgumentException>(() => new Atomic<Guid>(SessionValue, MemoryOrder.Consume));
#pragma warning restore 612, 618
        }

        [Fact]
        public void Atomic_InitialValue_With_MemoryOrder_Should_Success()
        {
            GC.KeepAlive(new Atomic<Guid>(SessionValue, MemoryOrder.Acquire));
            GC.KeepAlive(new Atomic<Guid>(SessionValue, MemoryOrder.Release));
            GC.KeepAlive(new Atomic<Guid>(SessionValue, MemoryOrder.AcqRel));
            GC.KeepAlive(new Atomic<Guid>(SessionValue, MemoryOrder.SeqCst));
        }

        [Fact]
        public void Atomic_InitialValue_With_MemoryOrder_Default_Should_Success()
        {
            GC.KeepAlive(new Atomic<Guid>(SessionValue));
        }
    }
}
