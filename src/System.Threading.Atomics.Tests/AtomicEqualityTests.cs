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

        /************************/

        public static IEnumerable<object[]> Primitive_Value_Change_Data
        {
            get
            {
                yield return new object[] {byte.MinValue, byte.MaxValue, (byte) 0, (byte) 123};
                yield return new object[] { sbyte.MinValue, sbyte.MaxValue, (sbyte)0, (sbyte)123 };
                yield return new object[] { short.MinValue, short.MaxValue, (short)0, (short)123 };
                yield return new object[] { ushort.MinValue, ushort.MaxValue, (ushort)0, (ushort)123 };
                yield return new object[] { double.MinValue, double.MaxValue, 0d, 123d };
                yield return new object[] { float.MinValue, float.MaxValue, 0f, 123f };
                yield return new object[] { ulong.MinValue, ulong.MaxValue, 0ul, 123ul };
            }
        }

        [Theory]
        [MemberData("Primitive_Value_Change_Data")]
        public void Atomic_Primitive_Value_Change_Should_Success<T>(T minValue, T maxValue, T defaultValue, T comparand) where T : struct, IEquatable<T>
        {
            var atomicInteger = new Atomic<T>(maxValue);
            Assert.Equal(maxValue, atomicInteger.Value);

            atomicInteger.Value = minValue;
            Assert.Equal(minValue, atomicInteger.Value);

            atomicInteger.Value = defaultValue;
            Assert.Equal(defaultValue, atomicInteger.Value);

            // same value assignment
            atomicInteger.Value = defaultValue;
            Assert.Equal(defaultValue, atomicInteger.Value);

            atomicInteger.Value = comparand;
            Assert.Equal(comparand, atomicInteger.Value);
            atomicInteger.Value = comparand;
            Assert.Equal(comparand, atomicInteger.Value);
        }

        public static IEnumerable<object[]> Primitive_IEquatable_Of_Primitive_Data
        {
            get
            {
                yield return new object[] { byte.MinValue, byte.MaxValue };
                yield return new object[] { sbyte.MinValue, sbyte.MaxValue };
                yield return new object[] { short.MinValue, short.MaxValue };
                yield return new object[] { ushort.MinValue, ushort.MaxValue };
                yield return new object[] { double.MinValue, double.MaxValue };
                yield return new object[] { float.MinValue, float.MaxValue };
                yield return new object[] { ulong.MinValue, ulong.MaxValue };
            }
        }

        [Theory]
        [MemberData("Primitive_IEquatable_Of_Primitive_Data")]
        public void Atomic_Primitive_IEquatable_Of_Primitive_Should_Compare<T>(T minValue, T maxValue) where T : struct, IEquatable<T>
        {
            IEquatable<T> firstAtomic = new Atomic<T>(maxValue);
            IEquatable<T> secondAtomic = new Atomic<T>(minValue);

            Assert.False(firstAtomic.Equals(minValue));
            Assert.False(secondAtomic.Equals(maxValue));
        }

        [Theory]
        [MemberData("Primitive_IEquatable_Of_Primitive_Data")]
        public void Atomic_Primitive_Implicit_Reassignment_Should_Change_Reference<T>(T minValue, T maxValue) where T : struct, IEquatable<T>
        {
            var firstAtomic = new Atomic<T>(maxValue);
            var secondAtomic = firstAtomic;

            firstAtomic = minValue;

            Assert.NotEqual(secondAtomic, firstAtomic);
            Assert.False(object.ReferenceEquals(secondAtomic, firstAtomic));
        }
    }
}
