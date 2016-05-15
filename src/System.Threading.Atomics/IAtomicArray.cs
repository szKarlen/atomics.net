namespace System.Threading.Atomics
{
    interface IAtomicArray<T> where T : struct
    {
        T this[int i] { get; set; }
        void Store(int index, T value, MemoryOrder order);
        T Load(int index, MemoryOrder order);
        bool IsLockFree { get; }
        T CompareExchange(int index, T value, T comparand);
    }

    interface IAtomicRefArray<T> : IAtomicArray<T> where T : struct
    {
        void Store(int index, ref T value, MemoryOrder order);
    }

    interface IAtomicArrayCASProvider<TType> where TType : struct, IEquatable<TType>
    {
        TType CompareExchange(int i, TType value, TType comparand);
    }
}