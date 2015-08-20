using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicEqualityTests
    {
        private static readonly Guid SessionValue = Guid.NewGuid();
        private static readonly Guid SessionNullValue = Guid.NewGuid();

        [Fact]
        public void Atomic_Value_Change_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionValue);
            Assert.Equal(SessionValue, atomicBoolean.Value);

            atomicBoolean.Value = SessionNullValue;
            Assert.Equal(SessionNullValue, atomicBoolean.Value);
            
            // same value assignment
            atomicBoolean.Value = SessionValue;
            Assert.Equal(SessionValue, atomicBoolean.Value);
            
            atomicBoolean.Value = SessionValue;
            Assert.Equal(SessionValue, atomicBoolean.Value);
        }

        [Fact]
        public void Atomic_IEquatable_Of_Guid_Should_Compare()
        {
            IEquatable<Guid> firstAtomic = new Atomic<Guid>(SessionValue);
            IEquatable<Guid> secondAtomic = new Atomic<Guid>(SessionNullValue);

            Assert.False(firstAtomic.Equals(SessionNullValue));
            Assert.False(secondAtomic.Equals(SessionValue));
        }

        [Fact]
        public void Atomic_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new Atomic<Guid>(SessionValue);
            var secondAtomic = new Atomic<Guid>(SessionNullValue);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void Atomic_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new Atomic<Guid>(SessionValue);
            var secondAtomic = firstAtomic;

            firstAtomic = SessionNullValue;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }

        [Fact]
        public void Atomic_Implicit_AcqRel_Should_Success()
        {
            Atomic<Guid> atomicBoolean = SessionValue;

            Assert.NotNull(atomicBoolean);
            Assert.Equal(SessionValue, atomicBoolean.Value);
        }

        [Fact]
        public void Atomic_Implicit_Bool_AcqRel_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionValue);

            var func = new Func<Guid, Guid>(i => i);

            Assert.Equal(SessionValue, func(atomicBoolean));
        }

        [Fact]
        public void Atomic_Equality_AcqRel_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionValue);

            Assert.True(atomicBoolean == SessionValue);
        }

        [Fact]
        public void Atomic_Inequality_AcqRel_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionNullValue);

            Assert.True(atomicBoolean != SessionValue);
        }

        // Sequential Consistency mode
        [Fact]
        public void Atomic_Implicit_Bool_SeqCst_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionValue, MemoryOrder.SeqCst);

            var func = new Func<Guid, Guid>(i => i);

            Assert.Equal(SessionValue, func(atomicBoolean));
        }

        [Fact]
        public void Atomic_Equality_SeqCst_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionValue, MemoryOrder.SeqCst);

            Assert.True(atomicBoolean == SessionValue);
        }

        [Fact]
        public void Atomic_Inequality_SeqCst_Should_Success()
        {
            var atomicBoolean = new Atomic<Guid>(SessionNullValue, MemoryOrder.SeqCst);

            Assert.True(atomicBoolean != SessionValue);
        }
    }
}
