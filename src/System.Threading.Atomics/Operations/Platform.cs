using System.Threading.Atomics.Interop;

namespace System.Threading.Atomics.Operations
{
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
}