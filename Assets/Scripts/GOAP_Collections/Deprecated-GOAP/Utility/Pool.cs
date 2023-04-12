using System.Collections.Generic;

namespace GOAP_Refactored
{
    public class Pool<T> where T : new()
    {
        private Queue<T> _objectQueue;

        public readonly int CacheCount;

        public Pool(int cachecount)
        {
            _objectQueue = new Queue<T>(cachecount);
        }

        /// <summary>
        /// Warms up the cache by filling the list with a max of cache count objects
        /// </summary>
        /// <param name="cacheCount"></param>
        public void WarmCache(int cacheCount)
        {
            cacheCount -= _objectQueue.Count;

            if (cacheCount <= 0) return;

            for (int i = 0; i < cacheCount; i++)
                _objectQueue.Enqueue(new T());
        }

        /// <summary>
        /// Trims the cache to the given count of objects
        /// </summary>
        /// <param name="cacheCount"></param>
        public void TrimCache(int cacheCount)
        {
            while (cacheCount > _objectQueue.Count)
                _objectQueue.Dequeue();
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public void ClearCache()
        {
            _objectQueue.Clear();
        }

        /// <summary>
        /// Takes an item out of the queue
        /// </summary>
        /// <returns></returns>
        public T Obtain()
        {
            if (_objectQueue.Count > 0)
                return _objectQueue.Dequeue();

            return new T();
        }

        /// <summary>
        /// Returns an item to the queue
        /// </summary>
        /// <param name="obj"></param>
        public void Free(T obj)
        {
            _objectQueue.Enqueue(obj);

            if (obj is IPoolingObject @object)
                @object.Reset();
        }
    }

    /// <summary>
    /// Objects implementing this interface will have {@link #reset()} called when passed to {@link #push(Object)}
    /// </summary>
    public interface IPoolingObject
    {
        /// <summary>
        /// Resets object for reuse. Object references should be nulled and fields may be set to default values
        /// </summary>
        void Reset();
    }
}