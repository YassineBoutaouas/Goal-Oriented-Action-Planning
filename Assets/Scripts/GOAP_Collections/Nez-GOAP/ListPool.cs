using System.Collections.Generic;

namespace GOAP_Nez_Deprecated
{
    /// <summary>
    /// Static class that can be used to pool lists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ListPool<T> //could this also be of type pool<T>????
    {
        static readonly Queue<List<T>> _objectQueue = new Queue<List<T>>();

        /// <summary>
        /// Warms up the cache filling it with a max of cacheCount objects
        /// </summary>
        /// <param name="cacheCount"></param>
        public static void WarmCache(int cacheCount)
        {
            cacheCount -= _objectQueue.Count;
            if (cacheCount > 0)
            {
                for (int i = 0; i < cacheCount; i++)
                    _objectQueue.Enqueue(new List<T>());
            }
        }

        /// <summary>
        /// Trims the cache down to cacheCount items
        /// </summary>
        /// <param name="cacheCount"></param>
        public static void TrimCache(int cacheCount)
        {
            while (cacheCount > _objectQueue.Count)
                _objectQueue.Dequeue();
        }

        /// <summary>
        /// clears out the cache
        /// </summary>
        public static void ClearCache() { _objectQueue.Clear(); }

        /// <summary>
        /// Pops an item off the stack
        /// </summary>
        /// <returns></returns>
        public static List<T> Obtain()
        {
            if (_objectQueue.Count > 0)
                return _objectQueue.Dequeue();

            return new List<T>();
        }

        public static void Free(List<T> obj)
        {
            _objectQueue.Enqueue(obj);
            obj.Clear();
        }
    }
}