namespace System.Threading.Atomics
{
    interface IAtomic<T> : IAtomicsOperator<T> where T : struct
    {
        T Value { get; set; }
    }
}