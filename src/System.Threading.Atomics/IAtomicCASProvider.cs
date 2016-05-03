namespace System.Threading.Atomics
{
    interface IAtomicCASProvider<T> where T : struct
    {
        T CompareExchange(ref T location1, T value, T comparand);

        bool Supports<TType>() where TType : struct;
    }
}