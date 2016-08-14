using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading.Atomics
{
#if NET40
    class Volatile
    {
        public static T Read<T>(ref T value)
        {
            return Platform.Read(ref value);
        }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    public sealed class MethodImplAttribute : Attribute
    {
        public MethodImplOptions Value { get; private set; }

        public MethodImplAttribute(MethodImplOptions methodImplOptions)
        {
            this.Value = methodImplOptions;
        }
    }

    [Flags]
    public enum MethodImplOptions
    {
        NoInlining = 8,
        NoOptimization = 64,
        PreserveSig = 128,
        AggressiveInlining = 256
    }

    
#endif
}

namespace System
{
    public struct TypedReference
    {

    }
}
