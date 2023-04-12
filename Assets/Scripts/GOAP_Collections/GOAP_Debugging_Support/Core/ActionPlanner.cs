using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP_Native_Debugging
{
    public class ActionPlanner
    {
        public readonly List<IAction> AllActions = new List<IAction>();

        public List<int> CurrentActionPlan = new List<int>();
        private List<int> _alternativePlan = new List<int>();

        public readonly HashSet<int> ViableActions = new HashSet<int>();

        public WorldState CurrentWorldState { get; private set; }
        public WorldState CurrentGoal { get; private set; }

        #region AStar references
        private readonly AStar _aStar;

        internal PriorityQueue<AStar.Node> _openNodes = new PriorityQueue<AStar.Node>();
        internal HashSet<AStar.Node> _closedNodes = new HashSet<AStar.Node>();
        #endregion

        public readonly Dictionary<string, WorldState> _worldStates = new Dictionary<string, WorldState>();
        public readonly Dictionary<string, WorldState> _goalStates = new Dictionary<string, WorldState>();

        private readonly PriorityQueue<WorldState> _validWorldStates = new PriorityQueue<WorldState>();
        private readonly PriorityQueue<WorldState> _validGoalStates = new PriorityQueue<WorldState>();

        public ActionPlanner() { _aStar = AStar.GetInstance(); }

        #region Action list methods
        /// <summary>
        /// Adds an action to the list of actions
        /// </summary>
        public void AddAction(IAction action) { AllActions.Add(action); }

        /// <summary>
        /// Removes an action from the list of actions
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAction(IAction action) { AllActions.Remove(action); }
        #endregion

        #region WorldState helper methods
        /// <summary>
        /// Creates a new world state and adds it to the list of worldstates
        /// </summary>
        public WorldState CreateWorldState(string name, Func<bool> validationMethod, Func<float> priority)
        {
            if (_worldStates.ContainsKey(name)) return _worldStates[name];

            WorldState state = new WorldState(name, validationMethod, priority);

            _worldStates.Add(name, state);

            return state;
        }

        /// <summary>
        /// Creates a new world state and adds it to the list of goalstates
        /// </summary>
        public WorldState CreateGoalState(string name, Func<bool> validationMethod, Func<float> priority)
        {
            if (_goalStates.ContainsKey(name)) return _goalStates[name];

            WorldState state = new WorldState(name, validationMethod, priority);

            _goalStates.Add(name, state);

            return state;
        }

        /// <summary>
        /// Removes a world state from the list of world states
        /// </summary>
        /// <param name="name"></param>
        public void RemoveWorldState(string name)
        {
            if (!_worldStates.Remove(name))
                Debug.Log(string.Format("The state {0} is not contained in the Worldstates", name));
        }

        /// <summary>
        /// Removes a goal state from the list of goal states
        /// </summary>
        /// <param name="name"></param>
        public void RemoveGoalState(string name)
        {
            if (!_goalStates.Remove(name))
                Debug.Log(string.Format("The state {0} is not contained in the Goalstates", name));
        }

        public bool GetGoalState(string key, out WorldState state) { return _worldStates.TryGetValue(key, out state); }

        public bool GetWorldState(string key, out WorldState state) { return _goalStates.TryGetValue(key, out state); }
        #endregion

        public void PopulateStates()
        {
            _validWorldStates.Clear();
            foreach (WorldState state in _worldStates.Values)
            {
                if (state.Validation())
                    _validWorldStates.Enqueue(state);
            }

            CurrentWorldState = _validWorldStates.Dequeue();

            CurrentWorldState.Apply();

            _validGoalStates.Clear();
            foreach (WorldState state in _goalStates.Values)
                if (state.Validation()) _validGoalStates.Enqueue(state);
        }

        /// <summary>
        /// Iterates through possible goals and returns the one that can be reached and that has the highest priority
        /// </summary>
        public void TryGetPlan(ref List<int> plan)
        {
            if (!_validGoalStates.Any()) return;

            CurrentGoal = _validGoalStates.Dequeue();

            Plan(CurrentWorldState, CurrentGoal, ref plan);

            if (plan != null && plan.Count > 0) return;

            TryGetPlan(ref plan);
        }

        /// <summary>
        /// Retrieves a plan from the AStar algorithm and returns it
        /// </summary>
        private List<int> Plan(WorldState startState, WorldState goalState, ref List<int> actions)
        {
            if (startState.States == null || goalState.States == null) return null;

            ViableActions.Clear();
            for (int i = 0; i < AllActions.Count; i++)
            {
                AllActions[i].Preconditions.Apply();
                AllActions[i].Postconditions.Apply();
                if (!AllActions[i].Validate()) continue;

                ViableActions.Add(i);
            }

            return _aStar.Plan(this, startState, goalState, ref actions);
        }

        public void Plan()
        {
            PopulateStates();

            TryGetPlan(ref CurrentActionPlan);
        }

        public bool ValidatePlan()
        {
            WorldState previousState = CurrentWorldState;
            WorldState previousGoal = CurrentGoal;

            PopulateStates();
            _alternativePlan.Clear();

            //If we went through all goals we assume that we are on the last one if there are no more left AND there are NO actions then our CURRENT plan is still valid!
            if (_alternativePlan != null && _alternativePlan.Count == 0 || (previousGoal.Equals(CurrentGoal) && previousState.Equals(CurrentWorldState))) return true;

            TryGetPlan(ref _alternativePlan);

            if (_alternativePlan.Count == 0) return true;

            CurrentActionPlan = _alternativePlan;

            return false;
        }
    }
}