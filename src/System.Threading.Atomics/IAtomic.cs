namespace System.Threading.Atomics
{
    interface IAtomic<T> : IAtomicOperators<T> where T : struct
    {
        /// <summary>
        /// Gets or sets atomically the underlying value
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        void Store(T value, MemoryOrder order);

        /// <summary>
        /// Gets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        /// <returns>The underlying value with provided <paramref name="order"/></returns>
        T Load(MemoryOrder order);

        /// <summary>
        /// Gets value whether the object is lock-free
        /// </summary>
        bool IsLockFree { get; }

        /// <summary>
        /// Atomically compares underlying value with <paramref name="comparand"/> for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="value">The value that replaces the underlying value if the comparison results in equality</param>
        /// <param name="comparand">The value that is compared to the underlying value.</param>
        /// <returns>The original underlying value</returns>
        T CompareExchange(T value, T comparand);
    }

    interface IAtomicRef<T> : IAtomic<T> where T : struct
    {
        /// <summary>
        /// Sets the underlying value with provided <paramref name="order"/>
        /// </summary>
        /// <param name="value">The value to store</param>
        /// <param name="order">The <see cref="MemoryOrder"/> to achieve</param>
        void Store(ref T value, MemoryOrder order);
    }
}