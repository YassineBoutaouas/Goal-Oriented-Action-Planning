using System;
using System.Text;
using Unity.Collections;

namespace GOAP_DOTS
{
    public struct WorldState : IEquatable<WorldState>, IComparable<WorldState>, IDisposable
    {
        public FixedString32Bytes Name;

        public bool IsValid;
        public float Priority;

        public NativeHashMap<FixedString32Bytes, bool> States;

        public WorldState(FixedString32Bytes name, NativeHashMap<FixedString32Bytes, bool> stateValues, int capacity, Allocator allocator, bool valid, float priority = 0) : this(name, capacity, allocator, valid, priority)
        {
            NativeArray<FixedString32Bytes> keys = stateValues.GetKeyArray(allocator);

            foreach (FixedString32Bytes key in keys)
            {
                States.Add(key, stateValues[key]);
            }

            keys.Dispose();
        }

        public WorldState(FixedString32Bytes name, int capacity, Allocator allocator, bool validationMethod, float priority = 0)
        {
            Name = new FixedString32Bytes(name);
            IsValid = validationMethod;
            Priority = priority;

            States = new NativeHashMap<FixedString32Bytes, bool>(capacity, allocator);
        }

        public void Add(FixedString32Bytes key, bool value) { States.TryAdd(key, value); }

        public void Remove(FixedString32Bytes key) { States.Remove(key); }

        public void SetStateValue(FixedString32Bytes key, bool other)
        {
            if (!States.ContainsKey(key)) return;

            States[key] = other;
        }

        public void Validate(bool valid) { IsValid = valid; }

        public void SetPriority(float priority)
        {
            Priority = priority;
        }

        #region IComparable, IEquatable, IDisposable
        /// <summary>
        /// Compares two worldstates based on their priorities
        /// Order from high to low
        /// </summary>
        public int CompareTo(WorldState other) { return Priority.CompareTo(other.Priority); }

        /// <summary>
        /// Compares two world states based on the state values that they store - exact comparison
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(WorldState other)
        {
            if (other.States.Count != States.Count) return false;

            return IsContained(other);
        }

        /// <summary>
        /// Returns true if a given world state states are contained in the own states
        /// </summary>
        public bool IsContained(WorldState other)
        {
            if (other.States.IsEmpty || States.IsEmpty) return false;

            NativeArray<FixedString32Bytes> keys = States.GetKeyArray(Allocator.Temp);

            foreach (FixedString32Bytes key in keys)
            {
                if (!other.States.TryGetValue(key, out bool otherState))
                {
                    keys.Dispose();
                    return false;
                }

                if (!otherState.Equals(States[key]))
                {
                    keys.Dispose();
                    return false;
                }
            }

            keys.Dispose();
            return true;
        }

        public void Dispose()
        {
            if (States.IsCreated) States.Dispose();

            IsValid = false;
            Priority = 0;

            Name.Clear();
        }
        #endregion

        public string ToString(StringBuilder strBuilder, bool showPriority = true)
        {
            strBuilder.Clear();

            if(!Name.IsEmpty)
                strBuilder.AppendFormat("{0} - ", Name);

            if (States.Count == 0)
            {
                strBuilder.Append("None");
                return strBuilder.ToString();
            }

            strBuilder.Append("States: ");

            NativeArray<FixedString32Bytes> keys = States.GetKeyArray(Allocator.Temp);

            foreach (FixedString32Bytes key in keys)
                strBuilder.AppendFormat("({0} : {1}) ", key, States[key].ToString()); ///* : {1} */ //state.Key,

            keys.Dispose();

            if(showPriority)
                strBuilder.AppendFormat("Priority: {0}", Priority);

            return strBuilder.ToString();
        }
    }
}