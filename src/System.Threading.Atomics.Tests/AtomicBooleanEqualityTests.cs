using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicBooleanEqualityTests
    {
        [Fact]
        public void AtomicBoolean_Value_Change_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(true);
            Assert.Equal(true, atomicBoolean.Value);

            atomicBoolean.Value = false;
            Assert.Equal(false, atomicBoolean.Value);
            
            // same value assignment
            atomicBoolean.Value = true;
            Assert.Equal(true, atomicBoolean.Value);
            
            atomicBoolean.Value = true;
            Assert.Equal(true, atomicBoolean.Value);
        }

        [Fact]
        public void AtomicBoolean_IEquatable_Of_Bool_Should_Compare()
        {
            IEquatable<bool> firstAtomic = new AtomicBoolean(true);
            IEquatable<bool> secondAtomic = new AtomicBoolean(false);

            Assert.False(firstAtomic.Equals(false));
            Assert.False(secondAtomic.Equals(true));
        }

        [Fact]
        public void AtomicBoolean_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new AtomicBoolean(true);
            var secondAtomic = new AtomicBoolean(false);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void AtomicBoolean_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new AtomicBoolean(true);
            var secondAtomic = firstAtomic;

            firstAtomic = false;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }

        [Fact]
        public void AtomicBoolean_Implicit_AcqRel_Should_Success()
        {
            AtomicBoolean atomicBoolean = true;

            Assert.NotNull(atomicBoolean);
            Assert.Equal(true, atomicBoolean.Value);
        }

        [Fact]
        public void AtomicBoolean_Implicit_Bool_AcqRel_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(true);

            var func = new Func<bool, bool>(i => i);

            Assert.Equal(true, func(atomicBoolean));
        }

        [Fact]
        public void AtomicBoolean_Equality_AcqRel_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(true);

            Assert.True(atomicBoolean == true);
        }

        [Fact]
        public void AtomicBoolean_Inequality_AcqRel_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(false);

            Assert.True(atomicBoolean != true);
        }

        // Sequential Consistency mode
        [Fact]
        public void AtomicBoolean_Implicit_Bool_SeqCst_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(true, MemoryOrder.SeqCst);

            var func = new Func<bool, bool>(i => i);

            Assert.Equal(true, func(atomicBoolean));
        }

        [Fact]
        public void AtomicBoolean_Equality_SeqCst_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(true, MemoryOrder.SeqCst);

            Assert.True(atomicBoolean == true);
        }

        [Fact]
        public void AtomicBoolean_Inequality_SeqCst_Should_Success()
        {
            var atomicBoolean = new AtomicBoolean(false, MemoryOrder.SeqCst);

            Assert.True(atomicBoolean != true);
        }
    }
}
