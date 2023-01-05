// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System.Collections.Generic;

namespace DAZ_Installer.WinApp
{
    /// <summary>
    /// A special dictionary that extends Dictionary and includes a Queue to restrict cache size.
    /// Objects do not save based on get calls but rather add calls. If the capacity is reached,
    /// the last inserted item will be removed even if it has been called alot.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    // TODO: Make so objects are reserved by access calls.
    internal class DPCache<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private const byte MAX_CACHE_SIZE = 25;
        private Queue<TKey> _cache = new Queue<TKey>(MAX_CACHE_SIZE);
        public DPCache() : base(MAX_CACHE_SIZE) { }

        // Hiding is intended: use new.
        public new void Add(TKey key, TValue value)
        {
            if (Count == MAX_CACHE_SIZE)
            {
                var keyToRemove = _cache.Dequeue();
                Remove(keyToRemove);
            }
            base.Add(key, value);
            _cache.Enqueue(key);
        }

        // Hiding is intended: use new.
        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                if (ContainsKey(key)) base[key] = value;
                else
                {
                    this.Add(key, value);
                }

            }
        }

        // Hiding is intended: use new.
        public new void Clear()
        {
            Clear();
            _cache.Clear();
        }


    }
}
