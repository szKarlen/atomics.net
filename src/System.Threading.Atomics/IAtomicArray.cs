namespace System.Threading.Atomics
{
    interface IAtomicArray<T> where T : struct
    {
        /// <summary>
        /// Gets or sets the element at specified <paramref name="index"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        T this[int index] { get; set; }

        /// <summary>
        /// Sets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        void Store(int index, T value, MemoryOrder order);

        /// <summary>
        /// Gets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element from which to load</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        T Load(int index, MemoryOrder order);

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        bool IsLockFree { get; }

        /// <summary>
        /// Atomically compares underlying element at specified <paramref name="index"/> with <paramref name="comparand"/> for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="index">The index of element to compare and set</param>
        /// <param name="value">The value that replaces the underlying value if the comparison results in equality</param>
        /// <param name="comparand">The value that is compared to the underlying value.</param>
        /// <returns>The original underlying value</returns>
        T CompareExchange(int index, T value, T comparand);
    }

    interface IAtomicRefArray<T> : IAtomicArray<T> where T : struct
    {
        /// <summary>
        /// Sets an element at <paramref name="index"/> with provided <paramref name="order"/>
        /// </summary>
        /// <param name="index">The index of element at which to store</param>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <remarks>Providing <see cref="MemoryOrder.Relaxed"/> writes the value as <see cref="MemoryOrder.Acquire"/></remarks>
        void Store(int index, ref T value, MemoryOrder order);
    }
}