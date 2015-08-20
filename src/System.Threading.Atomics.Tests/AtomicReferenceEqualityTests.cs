using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Threading.Atomics.Tests
{
    public class AtomicReferenceEqualityTests
    {
        [Fact]
        public void AtomicReference_Value_Change_Should_Success()
        {
            var source = new object();
            var atomicReference = new AtomicReference<object>(source);
            Assert.Equal(source, atomicReference.Value);

            atomicReference.Value = null;
            Assert.Null(atomicReference.Value);
            
            // same value assignment
            atomicReference.Value = source;
            Assert.Equal(source, atomicReference.Value);
            
            atomicReference.Value = source;
            Assert.Equal(source, atomicReference.Value);
        }

        [Fact]
        public void AtomicReference_IEquatable_Of_Ref_Should_Compare()
        {
            IEquatable<object> firstAtomic = new AtomicReference<object>(new object());
            IEquatable<object> secondAtomic = new AtomicReference<object>(null);

            Assert.False(firstAtomic.Equals(false));
            Assert.False(secondAtomic.Equals(true));
        }

        [Fact]
        public void AtomicReference_Should_Implement_Reference_Equality()
        {
            var firstAtomic = new AtomicReference<object>(new object());
            var secondAtomic = new AtomicReference<object>(null);

            Assert.False(firstAtomic.Equals(secondAtomic));
            Assert.False(secondAtomic.Equals(firstAtomic));

            // self equality
            Assert.True(firstAtomic.Equals(firstAtomic));
            Assert.True(secondAtomic.Equals(secondAtomic));
        }

        [Fact]
        public void AtomicReference_Implicit_Reassignment_Should_Change_Reference()
        {
            var firstAtomic = new AtomicReference<object>(new object());
            var secondAtomic = firstAtomic;

            firstAtomic = false;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }

        [Fact]
        public void AtomicReference_Implicit_AcqRel_Should_Success()
        {
            AtomicBoolean atomicReference = true;

            Assert.NotNull(atomicReference);
            Assert.Equal(true, atomicReference.Value);
        }

        [Fact]
        public void AtomicReference_Implicit_Ref_AcqRel_Should_Success()
        {
            var source = new object();
            var atomicReference = new AtomicReference<object>(source);

            var func = new Func<object, object>(i => i);

            Assert.NotEqual(source, func(atomicReference));
            Assert.Equal(source, func(atomicReference.Value));
        }

        [Fact]
        public void AtomicReference_Equality_AcqRel_Should_Success()
        {
            var source = new object();
            var atomicReference = new AtomicReference<object>(source);

            Assert.True(atomicReference == source);
        }

        [Fact]
        public void AtomicReference_Inequality_AcqRel_Should_Success()
        {
            var atomicReference = new AtomicReference<object>(null);

            Assert.True(atomicReference == null);
            Assert.False(object.ReferenceEquals(atomicReference, null));
        }

        // Sequental Consistency mode
        [Fact]
        public void AtomicReference_Implicit_Ref_SeqCst_Should_Success()
        {
            var source = new object();
            var atomicReference = new AtomicReference<object>(source, MemoryOrder.SeqCst);

            var func = new Func<object, object>(i => i);

            Assert.NotEqual(source, func(atomicReference));
            Assert.Equal(source, func(atomicReference.Value));
        }

        [Fact]
        public void AtomicReference_Equality_SeqCst_Should_Success()
        {
            var source = new object();
            var atomicReference = new AtomicReference<object>(source, MemoryOrder.SeqCst);

            Assert.True(atomicReference == source);
        }

        [Fact]
        public void AtomicReference_Inequality_SeqCst_Should_Success()
        {
            var atomicReference = new AtomicReference<object>(null, MemoryOrder.SeqCst);

            Assert.False(atomicReference != null);
        }
    }
}
