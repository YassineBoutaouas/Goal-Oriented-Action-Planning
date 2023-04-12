using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP_Native_Debugging
{
    [CreateAssetMenu(menuName = "AI/GOAP/Blackboard", fileName = "New Blackboard")]
    public class Blackboard : ScriptableObject
    {
        [System.Serializable]
        public class Item : IEqualityComparer<Item>, IEquatable<Item>
        {
            public string Key;
            public ValueType Value;

            public Item(string key, ValueType value)
            {
                Key = key;
                Value = value;
            }

            public bool Equals(Item x, Item y)
            {
                return x.Key.Equals(y.Key);
            }

            public bool Equals(Item other)
            {
                return Key.Equals(other.Key);
            }

            public int GetHashCode(Item obj)
            {
                return obj.GetHashCode();
            }
        }

        public enum ValueType { Bool, Int, Float, String, Vector }

        public List<Item> Entries = new List<Item>();

        public Item CreateEntry(string key, ValueType type)
        {
            Item i = new Item(key, type);
            Entries.Add(i);

            return i;
        }

        public void RemoveEntry(string key)
        {
            Item i = Entries.Find(i => i.Key == key);
            if(i == null) return;

            Entries.Remove(i);
        }

        public void RemoveEntry(Item item)
        {
            if (!Entries.Contains(item)) return;

            Entries.Remove(item);
        }

        public void ClearEntries()
        {
            Entries.Clear(); 
        }
    }
}