using System;
using System.Collections.Generic;
using System.Text;

namespace GOAP.Utilities
{
    /// <summary>
    /// Class that implements a generic priority queue
    /// See: https://visualstudiomagazine.com/Articles/2012/11/01/Priority-Queues-with-C.aspx
    /// Lower priority values (lower numerical values) represent higher conceptual priorities
    /// The algorithm sorts from small priority values to big priority values
    /// </summary>
    public class PriorityQueue<T> where T : IComparable
    {
        /// <summary>
        /// The list represents the heap
        /// </summary>
        private List<T> _data;

        /// <returns>Returns the count of the object in the queue</returns>
        public int Count { get { return _data.Count; } }

        public PriorityQueue() { _data = new List<T>(); }

        private void Swap(int index1, int index2)
        {
            T tmp = _data[index1];
            _data[index1] = _data[index2];
            _data[index2] = tmp;
        }

        /// <summary>
        /// Adds an item back to the queue
        /// </summary>
        /// <param name="obj"></param>
        public void Enqueue(T obj)
        {
            _data.Add(obj);
            int childIndex = _data.Count - 1;
            int parentIndex;

            //bubbling up
            while (childIndex > 0)
            {
                parentIndex = (childIndex - 1) / 2; //O(lg n) instead of nested loop without use of binary heap -> O(n lg n)

                //if child item is larger than (or equal to) parent --> done
                if (_data[childIndex].CompareTo(_data[parentIndex]) >= 0) break;

                //Else --> Triangle swap
                Swap(childIndex, parentIndex);

                childIndex = parentIndex;
            }
        }

        /// <summary>
        /// Pop an item of type T off the queue
        /// </summary>
        /// <returns>object in front of the queue - has lowest priority value</returns>
        public T Dequeue()
        {
            if (_data.Count < 1) return default;

            int lastIndex = _data.Count - 1;
            T frontObject = _data[0]; //fetch front

            _data[0] = _data[lastIndex]; //override current front object
            _data.RemoveAt(lastIndex); //remove item at the last index

            --lastIndex; //last index after removal

            int parentIndex = 0; //start at the front of the queue

            //bubble up
            while (true)
            {
                int leftChildIndex = parentIndex * 2 + 1; //left childIndex of parent
                int rightChildIndex = leftChildIndex + 1; //right childIndex of parent

                if (leftChildIndex > lastIndex) break; //no more children - done

                //if there is a right child and its compare value is smaller than leftchild, use the right child
                if (rightChildIndex <= lastIndex && _data[rightChildIndex].CompareTo(_data[leftChildIndex]) < 0)
                    leftChildIndex = rightChildIndex;

                if (_data[parentIndex].CompareTo(_data[leftChildIndex]) <= 0) break; //parent is smaller (or equal to) smallest child - done

                //Swap items
                Swap(parentIndex, leftChildIndex);

                parentIndex = leftChildIndex;
            }

            return frontObject;
        }

        /// <summary>
        /// Removes a specific items from the list
        /// </summary>
        public void Remove(T item)
        {
            _data.Remove(item);
        }

        /// <summary>
        /// Checks if the order of items is consistent - if they are ordered correctly from small to big
        /// </summary>
        /// <returns></returns>
        public bool IsConsistent()
        {
            if (_data.Count == 0) return true;

            int lastIndex = _data.Count - 1;

            for (int parentIndex = 0; parentIndex < _data.Count; ++parentIndex)
            {
                int leftChildIndex = parentIndex * 2 + 1;
                int rightChildIndex = leftChildIndex + 1;

                //if item at childIndex exists and its priority value is bigger than that of the parent then return false
                if (leftChildIndex <= lastIndex && _data[parentIndex].CompareTo(_data[leftChildIndex]) > 0) return false;
                if (rightChildIndex <= lastIndex && _data[parentIndex].CompareTo(_data[rightChildIndex]) > 0) return false;
            }

            return true;
        }

        /// <returns>Returns the first item in the queue which has the lowest priority value</returns>
        public T Peek() { return _data[0]; }

        /// <returns>Returns true if there are any items in the queue</returns>
        public bool Any() { return _data.Count > 0; }

        /// <summary> Clears the queue </summary>
        public void Clear() { _data.Clear(); }

        /// <returns>Returns true if the queue contains the given item</returns>
        public bool Contains(T item) { return _data.Contains(item); }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendFormat("[PriorityQueue] with {0} items: ", _data.Count);

            for (int i = 0; i < _data.Count; i++)
            {
                result.Append(_data[i].ToString()).Append(i == _data.Count - 1 ? ". " : ", ");
            }

            result.AppendFormat("IsConsistent: {0}", IsConsistent());

            return result.ToString();
        }
    }
}