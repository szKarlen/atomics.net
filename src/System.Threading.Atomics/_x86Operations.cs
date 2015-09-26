using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Atomics.Interop;
using System.Threading.Tasks;

namespace System.Threading.Atomics
{
    interface IOperations
    {
        T Read<T>(ref T location);
        T ReadAcquire<T>(ref T location);
        T ReadSeqCst<T>(ref T location);

        void Write<T>(ref T location, ref T value);
        void WriteRelease<T>(ref T location, ref T value);
        void WriteSeqCst<T>(ref T location, ref T value);
    }

    static class Platform
    {
        private static readonly IOperations _Operations = GetOperations();

        private static IOperations GetOperations()
        {
            return NativeMethods.HasStrongMM() ? new _x86Operations() : (IOperations)new _ARMOperations();
        }

        public static IOperations Operations
        {
            get { return _Operations; }
        }
    }

    class _x86Operations : IOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadAcquire<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadSeqCst<T>(ref T location)
        {
            var tmp = location;
            Interlocked.MemoryBarrier();
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ref T location, ref T value)
        {
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRelease<T>(ref T location, ref T value)
        {
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSeqCst<T>(ref T location, ref T value)
        {
            Interlocked.MemoryBarrier();
            location = value;
        }
    }

    class _ARMOperations : IOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ref T location)
        {
            return location;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadAcquire<T>(ref T location)
        {
            var tmp = location;
            Interlocked.MemoryBarrier();
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadSeqCst<T>(ref T location)
        {
            Interlocked.MemoryBarrier();
            var tmp = location;
            Interlocked.MemoryBarrier();
            return tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ref T location, ref T value)
        {
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRelease<T>(ref T location, ref T value)
        {
            Interlocked.MemoryBarrier();
            location = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSeqCst<T>(ref T location, ref T value)
        {
            Interlocked.MemoryBarrier();
            var tmp = value;
            Interlocked.MemoryBarrier();
            location = tmp;
        }
    }
}
