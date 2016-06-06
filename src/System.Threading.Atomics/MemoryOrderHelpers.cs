namespace System.Threading.Atomics
{
    static class MemoryModelHelpers
    {
        public static bool IsAcquireRelease(this MemoryOrder order)
        {
            return order >= MemoryOrder.Acquire && order <= MemoryOrder.AcqRel;
        }

        public static bool IsSpported(this MemoryOrder order)
        {
            return order != MemoryOrder.Consume;
        }

        public static int ToInt32(this bool target)
        {
            return target ? 1 : 0;
        }
    }
}
