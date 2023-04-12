using System.Collections.Generic;

namespace GOAP_Nez_Deprecated
{

    /// <summary>
    /// Class used to pool any object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Pool<T> where T : new()
    {
        private static Queue<T> _objectQueue = new Queue<T>(10);

        /// <summary>
        /// Warm up cache - filling it with a max of cacheCount objects of type T
        /// </summary>
        /// <param name="cacheCount"></param>
        public static void WarmCache(int cacheCount)
        {
            cacheCount -= _objectQueue.Count;

            if (cacheCount <= 0) return;

            for (var i = 0; i < cacheCount; i++)
                _objectQueue.Enqueue(new T());
        }

        /// <summary>
        /// Trims cache down to cacheCount items
        /// </summary>
        /// <param name="cacheCount"></param>
        public static void TrimCache(int cacheCount)
        {
            while (cacheCount > _objectQueue.Count)
                _objectQueue.Dequeue();
        }

        public static void ClearCache() { _objectQueue.Clear(); }

        /// <summary>
        /// Pops am item off the stack - creates a new one if none are available
        /// </summary>
        /// <returns></returns>
        public static T Obtain()
        {
            if (_objectQueue.Count > 0)
                return _objectQueue.Dequeue();

            return new T();
        }

        /// <summary>
        /// Return an item back to the queue - calling release to reset all values
        /// </summary>
        /// <param name="obj"></param>
        public static void Free(T obj)
        {
            _objectQueue.Enqueue(obj);

            if (obj is IPoolingObject)
                ((IPoolingObject)obj).Reset();
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