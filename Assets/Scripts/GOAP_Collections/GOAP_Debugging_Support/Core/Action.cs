using System.Collections;

namespace GOAP_Native_Debugging
{
    public interface IAction
    {
        public string Name { get; set; }
        public WorldState Preconditions { get; set; }
        public WorldState Postconditions { get; set; }
        public bool IsExecuting { get; set; }

        public bool Validate();
        public int CalculateCost();

        public string ToString();
    }

    /// <summary>
    /// Class that takes in an optional object of type T for contextualization
    /// </summary>
    public abstract class Action<T> : ActionBase where T : Agent
    {
        protected T _contextObject;
        public Action(T contextObject, string name, int cost) : base(name, cost) { _contextObject = contextObject; }
    }

    /// <summary>
    /// Class that provides basic functions for any action
    /// </summary>
    public abstract class ActionBase : IAction
    {
        public string Name { get; set; }
        public WorldState Preconditions { get; set; }
        public WorldState Postconditions { get; set; }
        public bool IsExecuting { get; set; }

        private readonly int _defaultCost;

        public ActionBase(string name, int cost)
        {
            Name = name;
            _defaultCost = cost;

            Preconditions = new WorldState("Preconditions: ", WorldState.True);
            Postconditions = new WorldState("Postconditions: ", WorldState.True);
        }

        #region Validation
        /// <summary>
        /// Called before the plan is proposed - gives the action an opportunity to opt out if it cant be used
        /// </summary>
        /// <returns></returns>
        public virtual bool Validate() { return true; }

        /// <summary>
        /// Calculates the cost of a given action - returns the default cost set in the constructor if no value is given
        /// </summary>
        /// <returns></returns>
        public virtual int CalculateCost() { return _defaultCost; }
        #endregion

        #region Action execution
        /// <summary>
        /// Called when the action is first started
        /// </summary>
        public virtual IEnumerator OnActionStart() { yield return null; }

        /// <summary>
        /// Used to determine what happens while the action is executed
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnActionExecute() { yield return null; }

        /// <summary>
        /// Called when the action is ended
        /// </summary>
        public virtual IEnumerator OnActionEnd() { yield return null; }

        /// <summary>
        /// Called when an action is cancelled
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnActionCancelled() { yield return null; }
        #endregion
    }
}