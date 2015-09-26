namespace System.Threading.Atomics
{
    interface IAtomicsOperator<T> where T : struct
    {
        T CompareExchange(ref T location1, T value, T comparand);
        T Read(ref T location1);

        bool Supports<TType>() where TType : struct;
    }
}