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
}