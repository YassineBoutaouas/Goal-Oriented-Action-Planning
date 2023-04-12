using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP_Refactored
{
    // public class Agent<T> where T : MonoBehaviour
    // {
    //     public event Action OnPlanEnd;
    //     public event Action OnPlanChanged;

    //     protected T _controllerObject;

    //     public ActionBase CurrentAction { get; private set; }
    //     public WorldState CurrentWorldState { get; private set; }
    //     public WorldState CurrentGoal { get; private set; }

    //     #region Private fields
    //     private Stack<ActionBase> _currentActions = new Stack<ActionBase>();
    //     private Stack<ActionBase> _alternativePlan = new Stack<ActionBase>();

    //     private readonly Dictionary<string, WorldState> _worldStates = new Dictionary<string, WorldState>();
    //     private readonly Dictionary<string, WorldState> _goalStates = new Dictionary<string, WorldState>();

    //     private readonly PriorityQueue<WorldState> _validWorldStates = new PriorityQueue<WorldState>();
    //     private readonly PriorityQueue<WorldState> _validGoalStates = new PriorityQueue<WorldState>();

    //     private readonly ActionPlanner _planner;
    //     private bool _cancellablePlan;
    //     private IEnumerator _currentPlanExecution;
    //     #endregion

    //     public Agent(T controller)
    //     {
    //         _planner = new ActionPlanner();
    //         _controllerObject = controller;
    //     }

    //     #region Action list
    //     /// <summary>
    //     /// Adds actions to the list of actions
    //     /// </summary>
    //     /// <param name="actions"></param>
    //     public ActionBase AddAction(ActionBase action)
    //     {
    //         _planner.AddAction(action);
    //         return action;
    //     }

    //     /// <summary>
    //     /// Removes an action from the list of actions
    //     /// </summary>
    //     /// <param name="action"></param>
    //     public void RemoveAction(ActionBase action) { _planner.RemoveAction(action); }
    //     #endregion

    //     #region WorldState helper methods
    //     /// <summary>
    //     /// Creates a new world state and adds it to the list of worldstates
    //     /// </summary>
    //     /// <param name="name"></param>
    //     /// <returns></returns>
    //     public WorldState CreateWorldState(string name, Func<bool> validationMethod, Func<float> priority)
    //     {
    //         if (_worldStates.ContainsKey(name)) return _worldStates[name];

    //         WorldState state = new WorldState(name, validationMethod, priority);

    //         _worldStates.Add(name, state);

    //         return state;
    //     }

    //     /// <summary>
    //     /// Creates a new world state and adds it to the list of goalstates
    //     /// </summary>
    //     /// <param name="name"></param>
    //     /// <returns></returns>
    //     public WorldState CreateGoalState(string name, Func<bool> validationMethod, Func<float> priority)
    //     {
    //         if (_goalStates.ContainsKey(name)) return _goalStates[name];

    //         WorldState state = new WorldState(name, validationMethod, priority);

    //         _goalStates.Add(name, state);

    //         return state;
    //     }

    //     /// <summary>
    //     /// Removes a world state from the list of world states
    //     /// </summary>
    //     /// <param name="name"></param>
    //     public void RemoveWorldState(string name)
    //     {
    //         if (!_worldStates.Remove(name))
    //             Debug.Log(string.Format("The state {0} is not contained in the Worldstates", name));
    //     }

    //     /// <summary>
    //     /// Removes a goal state from the list of goal states
    //     /// </summary>
    //     /// <param name="name"></param>
    //     public void RemoveGoalState(string name)
    //     {
    //         if (!_goalStates.Remove(name))
    //             Debug.Log(string.Format("The state {0} is not contained in the Goalstates", name));
    //     }
    //     #endregion

    //     #region Execute plan
    //     /// <summary>
    //     /// Calls the planner to retrieve a plan
    //     /// </summary>
    //     /// <param name="cancellable"></param>
    //     public void Plan(bool cancellable = true)
    //     {
    //         _cancellablePlan = cancellable;
    //         float timeStart = Time.realtimeSinceStartup;

    //         PopulateStates();

    //         TryGetPlan(ref _currentActions);

    //         Debug.Log(ToString(Time.realtimeSinceStartup - timeStart));

    //         if(HasActionPlan()) _controllerObject.RestartCoroutine(ref _currentPlanExecution, PopAction());
    //         //Stop routine, start routine
    //     }

    //     /// <summary>
    //     /// Method to validate a plan based on the highest priority world state and goal state
    //     /// If needed the planner will propose a new plan, cancelling the old one
    //     /// </summary>
    //     public bool ValidatePlan(bool cancellable = true)
    //     {
    //         if (!_cancellablePlan) return true;

    //         _cancellablePlan = cancellable;
    //         float timeStamp = Time.realtimeSinceStartup;

    //         WorldState previousState = CurrentWorldState;
    //         WorldState previousGoal = CurrentGoal;

    //         PopulateStates();
    //         _alternativePlan.Clear();
    //         TryGetPlan(ref _alternativePlan);

    //         //If we went through all goals we assume that we are on the last one if there are no more left AND there are NO actions then our CURRENT plan is still valid!
    //         if (_alternativePlan != null && _alternativePlan.Count == 0 || (previousGoal.Equals(CurrentGoal) && previousState.Equals(CurrentWorldState))) return true;

    //         OnPlanChanged?.Invoke();

    //         //Stop routine

    //         _currentActions = _alternativePlan;

    //         Debug.Log(ToString(Time.realtimeSinceStartup - timeStamp));

    //         if (HasActionPlan()) _controllerObject.RestartCoroutine(ref _currentPlanExecution, PopAction());

    //         return false;
    //     }

    //     /// <returns>Returns true if there are actions available</returns>
    //     public bool HasActionPlan() { return _currentActions != null && _currentActions.Count > 0; }

    //     /// <summary>
    //     /// Queues the actions and returns them when the previous one has finished
    //     /// </summary>
    //     /// <returns></returns>
    //     private IEnumerator PopAction()
    //     {
    //         CurrentAction = _currentActions.Pop();
    //         yield return CurrentAction.Update();

    //         //Wait until done
    //         if (HasActionPlan())
    //             yield return PopAction();
    //         else
    //         {
    //             yield return null;
    //             OnPlanEnd?.Invoke();
    //         }
    //     }
    //     #endregion

    //     #region Get plan
    //     public bool GetGoalState(string key, out WorldState state)
    //     {
    //         return _worldStates.TryGetValue(key, out state);
    //     }

    //     public bool GetWorldState(string key, out WorldState state)
    //     {
    //         return _goalStates.TryGetValue(key, out state);
    //     }

    //     /// <summary>
    //     /// Populates the pq with valid goal states and sets the current world state with the highest priority
    //     /// </summary>
    //     private void PopulateStates()
    //     {
    //         _validWorldStates.Clear();
    //         foreach (WorldState state in _worldStates.Values)
    //         {
    //             if (state.Validation())
    //                 _validWorldStates.Enqueue(state);
    //         }

    //         CurrentWorldState = _validWorldStates.Dequeue();
    //         //World state is a given! There can still be more important or invalid states BUT it does not change when the goal cant be reached!
    //         //We try to satisfy our world state by reaching our goal NOT the other way around

    //         //We only need to do it like this for the WS because 1. the goal stays static and 2. the post conditions get applied at another point!
    //         CurrentWorldState.Apply();

    //         _validGoalStates.Clear();
    //         foreach (WorldState state in _goalStates.Values)
    //             if (state.Validation()) _validGoalStates.Enqueue(state);
    //     }

    //     /// <summary>
    //     /// Iterates through possible goals and returns the one that can be reached and that has the highest priority
    //     /// </summary>
    //     private void TryGetPlan(ref Stack<ActionBase> plan)
    //     {
    //         if (!_validGoalStates.Any()) return;

    //         CurrentGoal = _validGoalStates.Dequeue();

    //         _planner.Plan(CurrentWorldState, CurrentGoal, ref plan);

    //         if (plan != null && plan.Count > 0) return;

    //         TryGetPlan(ref plan);
    //     }
    //     #endregion

    //     public string ToString(float t)
    //     {
    //         string result = string.Format(
    //             "{0}\n\n{1}",
    //             _planner.DescribePlan(_currentActions, CurrentWorldState, CurrentGoal),
    //             string.Format("Time taken: {0} ms\n\n", (t * 1000).ToString("f6"))
    //             );

    //         return string.Format("[{0}]\n{1}", _controllerObject.name, result);
    //     }
    // }
}
