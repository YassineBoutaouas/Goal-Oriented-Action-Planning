using System;
using System.Collections;
using System.Text;
using Unity.Collections;

namespace GOAP_DOTS
{
    public interface IAction : IDisposable
    {
        public string Name { get; set; }
        public WorldState Preconditions { get; set; }
        public WorldState Postconditions { get; set; }
        public bool IsExecuting { get; set; }

        public bool Validate();
        public int CalculateCost();

        public void UpdatePreconditions();
        public void UpdatePostconditions();

        public string ToString(StringBuilder strBuilder);
    }

    public class Action<T> : ActionBase where T : Agent
    {
        protected T _contextObject;

        public Action(T contextObject, string name, int cost) : base(name, cost) { _contextObject = contextObject; }
    }

    public class ActionBase : IAction
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

            Preconditions = new WorldState("Pre-conditions", 1, Allocator.Persistent, true);
            Postconditions = new WorldState("Post-conditions", 1, Allocator.Persistent, true);
        }

        #region Validation
        /// <summary>
        /// Called before the plan is proposed - gives the action an opportunity to opt out if it cant be used
        /// </summary>
        public virtual bool Validate() { return true; }

        /// <summary>
        /// Calculates the cost of a given action - returns the default cost set in the constructor if no value is given
        /// </summary>
        public virtual int CalculateCost() { return _defaultCost; }
        #endregion

        /// <summary>
        /// Method to update preconditions
        /// </summary>
        public virtual void UpdatePreconditions() { }
        /// <summary>
        /// Method to update postconditions
        /// </summary>
        public virtual void UpdatePostconditions() { }

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

        public void Dispose()
        {
            Preconditions.Dispose();
            Postconditions.Dispose();
        }

        /// <summary>
        /// Displays the action properties when called
        /// </summary>
        /// <returns></returns>
        public string ToString(StringBuilder strBuilder)
        {
            string preconditions = Preconditions.ToString(strBuilder, false);
            string postconditions = Postconditions.ToString(strBuilder, false);

            strBuilder.Clear();

            strBuilder.AppendFormat("[{0}] - Cost: {1}\n", Name, CalculateCost());

            strBuilder.Append(preconditions).Append("\n\n");
            strBuilder.Append(postconditions).Append("\n");

            return strBuilder.ToString();
        }
    }
}