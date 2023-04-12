using System;
using System.Collections.Generic;
using System.Text;

namespace GOAP_Native
{
    /// <summary>
    /// A class holding values needed to represent a desired world state like goals or beliefs that can be used by the action planner
    /// </summary>
    public class WorldState : IEquatable<WorldState>, IComparable
    {
        public string Name;

        public Func<bool> Validation;
        public Func<float> Priority;

        /// <summary>
        /// States that are held by the WorldState instance
        /// </summary>
        public Dictionary<string, IState> States;

        private StringBuilder _stringBuilder;

        public WorldState() : this("", True, DefaultPriority) { }

        public WorldState(string name, WorldState stateValues, Func<bool> validationMethod, Func<float> priorityMethod = default) : this(name, validationMethod, priorityMethod)
        {
            foreach (string key in stateValues.States.Keys)
                States.Add(key, stateValues.States[key]);
        }

        public WorldState(string name, Func<bool> validationMethod, Func<float> priorityMethod = default)
        {
            _stringBuilder = new StringBuilder();
            Name = name;
            Validation = validationMethod;
            Priority = priorityMethod;
            States = new Dictionary<string, IState>();
        }

        #region Static Methods
        /// <summary>
        /// Helper method that returns true to reduce the amount of validationMethods
        /// </summary>
        /// <returns></returns>
        public static bool True() { return true; }
        /// <summary>
        /// Helper method that returns false to reduce the amount of validationMethods
        /// </summary>
        /// <returns></returns>
        public static bool False() { return false; }
        /// <summary>
        /// Helper method that defines a default priority if there is nothing given
        /// </summary>
        /// <returns></returns>
        public static float DefaultPriority() { return 1; }
        #endregion

        #region Modifying states
        /// <summary>
        /// Adds a state to the list of states and sets its initial value
        /// </summary>W
        public void Add<T>(string key, T value, Func<T> bindingMethod = null) where T : IEquatable<T>
        { 
            States.TryAdd(key, new State<T>(value, bindingMethod));
        }

        /// <summary>
        /// Removes a state from the list of states
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key) { States.Remove(key); }

        /// <summary>
        /// Sets the value of a state within States
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        public void CopyStateValue(string key, IState other)
        {
            if (!States.ContainsKey(key))
                return;

            States[key] = other;
        }

        /// <summary>
        /// Iterates through the States and updates the values based on the given binding methods
        /// </summary>
        public void Apply()
        {
            foreach (IState state in States.Values)
                state.Apply();
        }
        #endregion

        #region IEquatable/IComparable
        /// <summary>
        /// Compares two worldstates based on their priorities
        /// Order from high to low
        /// </summary>
        public int CompareTo(object obj)
        {
            WorldState other = (WorldState)obj;

            if (Priority() < other.Priority())
                return 1;
            else if (Priority() == other.Priority())
                return 0;

            return -1;
        }

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
        /// Returns true if the worldstate states are contained in another worldstate
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsContained(WorldState other)
        {
            if (other.States.Count == 0 || States.Count == 0) return false;

            foreach (string key in States.Keys)
            {
                if (!other.States.TryGetValue(key, out IState otherState)) return false;

                if (!otherState.Equals(States[key])) return false;
            }

            return true;
        }
        #endregion

        public override string ToString()
        {
            _stringBuilder.Clear();

            if (Name != null && Name.Length > 0)
                _stringBuilder.AppendFormat("{0} - ", Name);

            if (States.Count == 0)
            {
                _stringBuilder.Append("None");
                return _stringBuilder.ToString();
            }

            _stringBuilder.Append("States: ");

            foreach (KeyValuePair<string, IState> state in States)
                _stringBuilder.AppendFormat("{0} ", state.ToString()); ///* : {1} */ //state.Key,

            if (Priority != null)
                _stringBuilder.AppendFormat("Priority: {0}", Priority());

            return _stringBuilder.ToString();
        }
    }

    public interface IState : IEquatable<IState>
    {
        public object CastValue { get; set; }
        public void Apply();
    }

    /// <summary>
    /// Represents a single state within the WorldState
    /// Each state can only represent a single value - Either a vector, float, string or a bool
    /// </summary>
    public struct State<T> : IState where T : IEquatable<T>
    {
        public T Value;
        public Func<T> ValueFunc;
        public object CastValue { get { return Value; } set { Value = (T)value; } }

        public State(T val, Func<T> valFunc)
        {
            Value = val;
            ValueFunc = valFunc;
        }

        public void Apply()
        {
            if (ValueFunc == null) return;

            Value = ValueFunc();
        }

        public bool Equals(IState other) { return CastValue.Equals(other.CastValue); }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
