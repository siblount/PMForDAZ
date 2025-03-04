namespace DAZ_Installer.UI
{
    /// <summary>
    /// Defines a object pool that manages reusable instances of <see cref="LibraryItem"/>s
    /// </summary>
    public class LibraryItemPool
    {
        private readonly Stack<LibraryItem> pool;
        private readonly int maxPoolSize;

        public LibraryItemPool(int initialSize = 25, int maxPoolSize = 25)
        {
            this.maxPoolSize = maxPoolSize;
            pool = new Stack<LibraryItem>(maxPoolSize);

            for (int i = 0; i < initialSize; i++)
            {
                pool.Push(CreateNewItem());
            }
        }

        /// <summary>
        /// Rents an <see cref="LibraryItem"/> from the pool. If the pool is empty, a new instance may be created.
        /// </summary>
        /// <returns>An instance of type T from the pool.</returns>
        public LibraryItem Rent()
        {
            if (pool.Count > 0)
            {
                var item = pool.Pop();
                item.Visible = true;
                return item;
            }
            return CreateNewItem();
        }

        /// <summary>
        /// Returns an <see cref="LibraryItem"/> to the pool, making it available for future use.
        /// </summary>
        /// <remarks>
        /// Additionally, the item's visibility is hidden.
        /// </remarks>
        /// <param name="item">The object to return to the pool. Must not be null.</param>
        public void Return(LibraryItem item)
        {
            if (item == null) return;
            if (pool.Count >= maxPoolSize) return;

            // Reset the item state
            item.Reset(false);
            item.Visible = false;
            pool.Push(item);
        }

        private static LibraryItem CreateNewItem()
        {
            return new LibraryItem();
        }
    }
}
