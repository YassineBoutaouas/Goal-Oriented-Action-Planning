using Extensions;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace GOAP_Native
{
    public abstract class Agent
    {
        #region Callbacks
        public event Action OnPlanRequested;
        public event Action OnPlanChanged;
        public event Action OnPlanEnd;

        public event Action OnEditorCallback;
        public void ReleaseEditorEvents() { OnEditorCallback = null; }
        #endregion

        protected GOAPAgent _controllerObject;
        public ActionPlanner Planner { get; private set; }

        public ActionBase CurrentAction { get; private set; }

        public bool CancellablePlan { private set; get; }

        private IEnumerator _currentPlanExecution;
        private IEnumerator _currentActionExecution;

        public Agent(GOAPAgent controller)
        {
            Planner = new ActionPlanner();
            _controllerObject = controller;
        }

        #region Action list
        /// <summary>
        /// Adds actions to the list of actions
        /// </summary>
        /// <param name="actions"></param>
        public ActionBase AddAction(ActionBase action)
        {
            Planner.AddAction(action);
            return action;
        }

        /// <summary>
        /// Removes an action from the list of actions
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAction(ActionBase action) { Planner.RemoveAction(action); }
        #endregion

        #region WorldState helper methods
        /// <summary>
        /// Creates a new world state and adds it to the list of worldstates
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public WorldState CreateWorldState(string name, Func<bool> validationMethod, Func<float> priority) { return Planner.CreateWorldState(name, validationMethod, priority); }

        /// <summary>
        /// Creates a new world state and adds it to the list of goalstates
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public WorldState CreateGoalState(string name, Func<bool> validationMethod, Func<float> priority) { return Planner.CreateGoalState(name, validationMethod, priority); }

        /// <summary>
        /// Removes a world state from the list of world states
        /// </summary>
        /// <param name="name"></param>
        public void RemoveWorldState(string name) { Planner.RemoveWorldState(name); }

        /// <summary>
        /// Removes a goal state from the list of goal states
        /// </summary>
        /// <param name="name"></param>
        public void RemoveGoalState(string name) { Planner.RemoveGoalState(name); }
        #endregion

        #region Execute plan
        /// <summary>
        /// Calls the planner to retrieve a plan
        /// </summary>
        /// <param name="cancellable"></param>
        public void Plan(bool cancellable = true)
        {
            OnPlanRequested?.Invoke();

            CancellablePlan = cancellable;

            float t = Time.realtimeSinceStartup;

            Planner.Plan();

            t = Time.realtimeSinceStartup - t;

            OnEditorCallback?.Invoke();

            if (HasActionPlan())
                _controllerObject.RestartCoroutine(ref _currentPlanExecution, ExecuteActions());

            UnityEngine.Debug.Log(ToString(t));
        }

        /// <summary>
        /// Method to validate a plan based on the highest priority world state and goal state
        /// If needed the planner will propose a new plan, cancelling the old one
        /// </summary>
        public bool ValidatePlan(bool cancellable = true)
        {
            if (!CancellablePlan) return true;

            CancellablePlan = cancellable;

            if(Planner.ValidatePlan()) return true;

            OnPlanChanged?.Invoke();
            
            OnEditorCallback?.Invoke();

            _controllerObject.StopCoroutine(_currentActionExecution);
            _controllerObject.RestartCoroutine(ref _currentPlanExecution, CancelPlan());

            return false;
        }

        #region Plan executions
        private IEnumerator ExecuteActions()
        {
            if (Planner.CurrentActionPlan.Count == 0)
            {
                OnPlanEnd?.Invoke();
                yield break;
            }

            CurrentAction = (ActionBase)Planner.AllActions[Planner.CurrentActionPlan[0]];

            _currentActionExecution = CurrentAction.OnActionStart();

            yield return _currentActionExecution;

            _currentActionExecution = CurrentAction.OnActionExecute();

            yield return _currentActionExecution;

            _currentActionExecution = CurrentAction.OnActionEnd();

            yield return _currentActionExecution;

            Planner.CurrentActionPlan.RemoveAt(0);

            _controllerObject.RestartCoroutine(ref _currentPlanExecution, ExecuteActions());
        }

        /// <summary>
        /// Method to cancel the current action plan
        /// </summary>
        private IEnumerator CancelPlan()
        {
            _currentActionExecution = CurrentAction.OnActionCancelled();
            yield return _currentActionExecution;

            _controllerObject.RestartCoroutine(ref _currentPlanExecution, ExecuteActions());
        }
        #endregion

        /// <returns>Returns true if there are actions available</returns>
        public bool HasActionPlan() { return Planner.CurrentActionPlan.Count > 0; }
        #endregion

        #region Get WorldState
        public bool GetGoalState(string key, out WorldState state) { return Planner.GetGoalState(key, out state); }

        public bool GetWorldState(string key, out WorldState state) { return Planner.GetWorldState(key, out state); }
        #endregion

        public string ToString(float t)
        {
            string result = string.Format(
                "{0}\n\n{1}",
                Planner.DescribePlan(Planner.CurrentActionPlan, Planner.CurrentWorldState, Planner.CurrentGoal),
                string.Format("Time taken: {0} ms\n\n", (t * 1000).ToString("f6"))
                );
            
            return string.Format("[{0}]\n{1}", _controllerObject.name, result);
        }
    }
}
