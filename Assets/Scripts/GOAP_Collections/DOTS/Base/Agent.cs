using System;
using System.Collections;
using System.Text;
using Unity.Collections;
using Extensions;

namespace GOAP_DOTS
{
    /// <summary>
    /// Class that contains methods to execute actions and methods that provide wrapper classes for the planner
    /// </summary>
    public abstract class Agent
    {
        public event Action OnChanged;

        public event Action OnPlanRequested;
        public event Action OnPlanChanged;
        public event Action OnPlanEnd;

        #region Editor callbacks
        public event Action OnEditorCallback;
        public void ReleaseEditorEvents() { OnEditorCallback = null; }
        #endregion

        public ActionPlanner _planner { private set; get; }
        public readonly GOAPAgent _controller;

        private ActionBase _currentAction;
        private IEnumerator _currentPlanExecution;
        private IEnumerator _currentActionExecution;

        public bool CancellablePlan { get; private set; }

        public Agent(GOAPAgent controller)
        {
            _controller = controller;
            _planner = new ActionPlanner();
        }

        #region Action adding/removing
        /// <summary>
        /// Adds an action to the planners list of all actions
        /// </summary>
        public ActionBase AddAction(ActionBase action)
        {
            _planner.AddAction(action);
            OnChanged?.Invoke();
            return action;
        }

        /// <summary>
        /// Removes an action from the planners list of all actions
        /// </summary>
        public void RemoveAction(ActionBase action) { _planner.RemoveAction(action); OnChanged?.Invoke(); }
        #endregion

        #region WorldState/ Goals helper methods
        public void AddWorldState(WorldState state) { _planner.AddWorldState(state); OnChanged?.Invoke(); }

        public void AddGoal(WorldState state) { _planner.AddGoal(state); OnChanged?.Invoke(); }

        public void RemoveWorldState(FixedString32Bytes key) { _planner.RemoveWorldState(key); OnChanged?.Invoke(); }

        public void RemoveGoalState(FixedString32Bytes key) { _planner.RemoveGoal(key); OnChanged?.Invoke(); }
        #endregion

        /// <summary>
        /// Method that requests a plan
        /// </summary>
        public void Plan(StringBuilder strBuilder, bool cancellablePlan = true)
        {
            OnPlanRequested?.Invoke();

            CancellablePlan = cancellablePlan;
            
            UpdateWorldStatesAndGoals();
            
            _planner.Plan();

            OnEditorCallback?.Invoke();

            //Debug.Log(_planner.DescribePlan(timeStamp, strBuilder));
            
            if(HasActionPlan())
                _controller.RestartCoroutine(ref _currentPlanExecution, ExecuteActions());
        }

        /// <summary>
        /// Method to validate the current plan and request a new one
        /// </summary>
        public bool ValidatePlan(StringBuilder strBuilder, bool cancellable = true)
        {
            if (!CancellablePlan) return true;

            CancellablePlan = cancellable;

            UpdateWorldStatesAndGoals();

            if (_planner.ValidatePlan()) return true;

            OnPlanChanged?.Invoke();

            OnEditorCallback?.Invoke();

            //Debug.Log(_planner.DescribePlan(timeStamp, strBuilder));

            _controller.StopCoroutine(_currentActionExecution);

            _controller.RestartCoroutine(ref _currentPlanExecution, CancelPlan());

            return false;
        }

        public bool HasActionPlan() { return !_planner.CurrentActionPlan.IsEmpty; }

        /// <summary>
        /// Method to execute the actions within the current plan
        /// </summary>
        private IEnumerator ExecuteActions()
        {
            if (_planner.CurrentActionPlan.IsEmpty)
            {
                OnPlanEnd?.Invoke();
                yield break;
            }

            _currentAction = (ActionBase)_planner.AllActions[_planner.CurrentActionPlan[0]];

            _currentActionExecution = _currentAction.OnActionStart();

            yield return _currentActionExecution;

            _currentActionExecution = _currentAction.OnActionExecute();

            yield return _currentActionExecution;

            _currentActionExecution = _currentAction.OnActionEnd();

            yield return _currentActionExecution;

            _planner.CurrentActionPlan.RemoveAt(0);

            _controller.RestartCoroutine(ref _currentPlanExecution, ExecuteActions());
        }

        /// <summary>
        /// Method to cancel the current action plan
        /// </summary>
        private IEnumerator CancelPlan()
        {
            _currentActionExecution = _currentAction.OnActionCancelled();
            yield return _currentActionExecution;

            _controller.RestartCoroutine(ref _currentPlanExecution, ExecuteActions());
        }

        #region Modify World States and Goals
        /// <summary>
        /// Method to update the world states and goals
        /// </summary>
        public virtual void UpdateWorldStatesAndGoals() { }

        /// <summary>
        /// Validates a given world state
        /// </summary>
        public void ValidateWorldState(FixedString32Bytes key, float priority) { _planner.UpdateWorldState(key, priority);  OnChanged?.Invoke(); }

        /// <summary>
        /// Validates a given goal
        /// </summary>
        public void ValidateGoal(FixedString32Bytes key, bool valid, float priority) { _planner.UpdateGoal(key, valid, priority); OnChanged?.Invoke(); }

        /// <summary>
        /// Method that flattens the preconditions and postconditions of actions
        /// </summary>
        public void BakeActionconditions() { _planner.BakeActionConditions(); }

        /// <summary>
        /// Method that flattens the nested list of goals
        /// </summary>
        public void BakeGoals() { _planner.BakeGoals(); }
        #endregion

        public virtual void Dispose() 
        {
            for (int i = 0; i < _planner.AllActions.Count; i++)
                _planner.AllActions[i].Dispose();

            _planner.Dispose(); 
        }
    }
}