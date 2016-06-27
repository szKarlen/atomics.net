namespace System.Threading.Atomics
{
    static class MemoryModelHelpers
    {
        public static bool IsAcquireRelease(this MemoryOrder order)
        {
            return order >= MemoryOrder.Acquire && order <= MemoryOrder.AcqRel;
        }

        public static void ThrowIfNotSupported(this MemoryOrder order)
        {
            if (order == MemoryOrder.SeqCst || order == MemoryOrder.AcqRel || order == MemoryOrder.Relaxed) return;
            
            if (order == MemoryOrder.Acquire || order == MemoryOrder.Release)
            {
                throw new ArgumentException("Using Acquire or Release only in constructor is not allowed. Please specify either Relaxed, AcqRel or SeqCst.", "order");
            }
            throw new ArgumentException("Consume memory ordering semantics is not supported", "order");
        }

        public static int ToInt32(this bool target)
        {
            return target ? 1 : 0;
        }
    }
}
