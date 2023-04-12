using System;
using Unity.Collections;

namespace GOAP_DOTS
{
    public struct NativePriorityQueue<T> : IDisposable where T : unmanaged, IEquatable<T>, IComparable<T>
    {
        internal NativeList<T> _data;

        public int Length { get { return _data.IsCreated ? _data.Length : 0; } }

        public int Capacity { get { return _data.IsCreated ? _data.Capacity : 0; } }

        public bool IsEmpty { get { return _data.IsCreated == false || _data.Length == 0; } }

        public bool IsCreated => _data.IsCreated;

        public NativePriorityQueue(int capacity, Allocator allocator) { _data = new NativeList<T>(capacity, allocator); }

        public T this[int index] { get { return _data[index]; } }

        //Enqueue and dequeue could be implemented as jobs

        public void Enqueue(T item)
        {
            _data.Add(item);

            int childIndex = _data.Length - 1;
            int parentIndex;

            while (childIndex > 0)
            {
                parentIndex = (childIndex - 1) >> 1;
                if (_data[childIndex].CompareTo(_data[parentIndex]) >= 0) break;
                Swap(childIndex, parentIndex);

                childIndex = parentIndex;
            }
        }

        public T Dequeue()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Cannot dequeue item from empty queue");

            int lastIndex = _data.Length - 1;
            T frontObject = _data[0];

            _data[0] = _data[lastIndex];
            _data.RemoveAt(lastIndex);

            --lastIndex;

            int parentIndex = 0;

            int leftChildIndex;
            int rightChildIndex;

            while (true)
            {
                leftChildIndex = (parentIndex << 1) + 1;
                rightChildIndex = leftChildIndex + 1;

                if (leftChildIndex > lastIndex) break;

                if (rightChildIndex <= lastIndex && _data[rightChildIndex].CompareTo(_data[leftChildIndex]) < 0)
                    leftChildIndex = rightChildIndex;

                if (_data[parentIndex].CompareTo(_data[leftChildIndex]) <= 0) break;

                Swap(parentIndex, leftChildIndex);

                parentIndex = leftChildIndex;
            }

            return frontObject;
        }

        public void RemoveAt(int index) { _data.RemoveAt(index); }

        private void Swap(int index1, int index2) { (_data[index2], _data[index1]) = (_data[index1], _data[index2]); }

        public T Peek()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Cannot peek item, queue is empty");

            return _data[0];
        }

        public bool IsConsistent()
        {
            if (_data.Length == 0) return true;

            int lastIndex = _data.Length - 1;

            int leftChildIndex;
            int rightChildIndex;

            for (int parentIndex = 0; parentIndex < _data.Length; parentIndex++)
            {
                leftChildIndex = (parentIndex << 1) + 1;
                rightChildIndex = leftChildIndex + 1;

                if (leftChildIndex <= lastIndex && _data[parentIndex].CompareTo(_data[leftChildIndex]) > 0) return false;
                if (rightChildIndex <= lastIndex && _data[parentIndex].CompareTo(_data[rightChildIndex]) > 0) return false;
            }

            return true;
        }

        public bool Contains(T item, out int indexResult)
        {
            indexResult = -1;

            for (int i = 0; i < _data.Length; i++)
            {
                if (!_data[i].Equals(item)) continue;

                indexResult = i;
                return true;
            }

            return false;
        }

        public T[] ToArray()
        {
            var length = _data.Length;

            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = _data[i];
            }

            return result;
        }

        public void Clear() { _data.Clear(); }

        public void Dispose()
        {
            if (_data.IsCreated)
                _data.Dispose();
        }
    }
}