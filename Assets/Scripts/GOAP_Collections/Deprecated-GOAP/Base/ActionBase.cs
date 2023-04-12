using System.Collections;
using System.Text;
using UnityEngine;

namespace GOAP_Refactored
{
    /// <summary>
    /// Class that takes in an optional object of type T for contextualization
    /// </summary>
    public abstract class Action<T> : ActionBase where T : Object
    {
        protected T _contextObject;
        public Action(T contextObject, string name, int cost) : base(name, cost) { _contextObject = contextObject; }
    }

    /// <summary>
    /// Class that provides basic functions for any action
    /// </summary>
    public abstract class ActionBase
    {
        public string Name { get; }

        public bool IsExecuting { get; private set; }

        private readonly int _defaultCost;

        public WorldState PreConditions = new WorldState("Preconditions: ", WorldState.True);
        public WorldState PostConditions = new WorldState("Postconditions: ", WorldState.True);

        private StringBuilder _stringBuilder = new StringBuilder();

        public ActionBase(string name, int cost)
        {
            Name = name;
            _defaultCost = cost;
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

        /// <summary>
        /// Serves as frame that calls all the other execution functions - the only function that needs to be called from outside
        /// </summary>
        /// <returns></returns>
        public IEnumerator Update()
        {
            IsExecuting = true;

            yield return OnActionStart();

            yield return OnActionExecute();

            yield return OnActionEnd();

            IsExecuting = false;
        }

        public void StopExecution()
        {
            IsExecuting = false;
        }
        #endregion

        /// <summary>
        /// Displays the action properties when called
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            _stringBuilder.Clear();

            _stringBuilder.AppendFormat("[{0}] - Cost: {1}\n", Name, CalculateCost());

            _stringBuilder.Append(PreConditions.ToString()).Append("\n\n");
            _stringBuilder.Append(PostConditions.ToString()).Append("\n");

            return _stringBuilder.ToString();
        }
    }
}