using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Extensions;

namespace GOAP_DOTS
{
    public class ActionPlanner : IDisposable
    {
        public readonly List<IAction> AllActions = new List<IAction>();

        public NativeList<int> CurrentActionPlan;
        private NativeList<int> _alternativePlan;

        public WorldState CurrentWorldState;
        public WorldState CurrentGoal;

        public NativeHashMap<FixedString32Bytes, WorldState> NestedWorldStates;
        public NativeHashMap<FixedString32Bytes, WorldState> NestedGoals;

        #region Flat arrays
        public NativeArray<(FixedString32Bytes, bool)> AllGoals;
        internal int _goalsWidth;

        public NativeArray<(FixedString32Bytes, bool)> AllPreconditions;
        internal int _preconditionsWidth;

        public NativeArray<(FixedString32Bytes, bool)> AllPostconditions;
        internal int _postconditionsWidth;
        #endregion

        public readonly NativeList<ViableAction> ViableActions;
        public readonly NativePriorityQueue<ViableWorldState> ViableGoals;

        public ActionPlanner()
        {
            ViableActions = new NativeList<ViableAction>(1, Allocator.Persistent);

            CurrentActionPlan = new NativeList<int>(1, Allocator.Persistent);
            _alternativePlan = new NativeList<int>(1, Allocator.Persistent);

            NestedWorldStates = new NativeHashMap<FixedString32Bytes, WorldState>(1, Allocator.Persistent);
            NestedGoals = new NativeHashMap<FixedString32Bytes, WorldState>(1, Allocator.Persistent);

            CurrentGoal = new WorldState("", 1, Allocator.Persistent, true); //can be eliminated?

            ViableGoals = new NativePriorityQueue<ViableWorldState>(1, Allocator.Persistent);
        }

        #region Action list methods
        /// <summary>
        /// Adds an action to the list of all actions
        /// </summary>
        public void AddAction(IAction action)
        {
            if (AllActions.Contains(action)) return;

            AllActions.Add(action);
        }

        /// <summary>
        /// Remove an action from the list of all actions
        /// </summary>
        public void RemoveAction(IAction action)
        {
            int index = AllActions.IndexOf(action);
            if (index == -1) return;
            AllActions.RemoveAt(index);
        }
        #endregion

        #region WorldState/ Goals helper methods
        public void AddWorldState(WorldState state) { NestedWorldStates.Add(state.Name, state); }

        public void RemoveWorldState(FixedString32Bytes key) { NestedWorldStates.Remove(key); }

        public void AddGoal(WorldState state) { if (!NestedGoals.TryAdd(state.Name, state)) return; }

        public void RemoveGoal(FixedString32Bytes key) { NestedGoals.Remove(key); }
        #endregion

        /// <summary>
        /// Method to request a plan
        /// </summary>
        public void Plan()
        {
            ValidateActions();

            ValidateWorldStatesAndGoals();

            AStar.Plan(this, CurrentActionPlan);
        }

        /// <summary>
        /// Method to validate the current plan
        /// </summary>
        public bool ValidatePlan()
        {
            WorldState previousWorldState = CurrentWorldState;
            WorldState previousGoal = CurrentGoal;

            ValidateWorldStatesAndGoals();

            if (previousWorldState.Equals(CurrentWorldState) && previousGoal.Equals(NestedGoals[ViableGoals.Peek().Name])) return true;

            ValidateActions();

            AStar.Plan(this, _alternativePlan);

            if (_alternativePlan.IsEmpty) return true;

            CurrentActionPlan = _alternativePlan;

            return false;
        }

        /// <summary>
        /// Method to validate the actions before a plan is requested
        /// </summary>
        public void ValidateActions()
        {
            ViableActions.Clear();

            for (int i = 0; i < AllActions.Count; i++)
            {
                //Apply pre and post conditions
                if (!AllActions[i].Validate()) continue;

                AllActions[i].UpdatePreconditions();
                AllActions[i].UpdatePostconditions();

                ViableActions.Add(new ViableAction(i, AllActions[i].CalculateCost(), AllActions[i].Name));
            }
        }

        #region Flatten/bake nested lists
        /// <summary>
        /// Method to flatten the nested list of goals
        /// </summary>
        public void BakeGoals()
        {
            if(AllGoals.Length > 0) AllGoals.Dispose();

            NativeArray<FixedString32Bytes> nestedgoalKeys = NestedGoals.GetKeyArray(Allocator.Temp);

            foreach (FixedString32Bytes key in nestedgoalKeys)
                _goalsWidth = math.max(_goalsWidth, NestedGoals[key].States.Count);

            AllGoals = new NativeArray<(FixedString32Bytes, bool)>(NestedGoals.Count * _goalsWidth, Allocator.Persistent);

            for (int i = 0; i < NestedGoals.Count; i++)
            {
                NativeArray<FixedString32Bytes> goalKeys = NestedGoals[nestedgoalKeys[i]].States.GetKeyArray(Allocator.Temp);
                int flatIndex = 0.CalculateFlatIndex(i, _goalsWidth);

                for (int j = 0; j < goalKeys.Length; j++)
                {
                    AllGoals[flatIndex] = (goalKeys[j], NestedGoals[nestedgoalKeys[i]].States[goalKeys[j]]);
                    flatIndex++;
                }
                goalKeys.Dispose();
            }

            nestedgoalKeys.Dispose();
        }
        
        /// <summary>
        /// Method to flatten the nested list of preconditions and postconditions
        /// </summary>
        public void BakeActionConditions()
        { 
            if(AllPreconditions.Length > 0) AllPreconditions.Dispose();
            if(AllPostconditions.Length > 0) AllPostconditions.Dispose();

            for (int i = 0; i < AllActions.Count; i++)
            {
                _preconditionsWidth = math.max(AllActions[i].Preconditions.States.Count, _preconditionsWidth);
                _postconditionsWidth = math.max(AllActions[i].Postconditions.States.Count, _postconditionsWidth);
            }

            AllPreconditions = new NativeArray<(FixedString32Bytes, bool)>(AllActions.Count * _preconditionsWidth, Allocator.Persistent);
            AllPostconditions = new NativeArray<(FixedString32Bytes, bool)>(AllActions.Count * _postconditionsWidth, Allocator.Persistent);

            for (int i = 0; i < AllActions.Count; i++)
            {
                NativeArray<FixedString32Bytes> prekeys = AllActions[i].Preconditions.States.GetKeyArray(Allocator.Temp);
                int flatIndex = 0.CalculateFlatIndex(i, _preconditionsWidth);

                for (int j = 0; j < prekeys.Length; j++)
                {
                    AllPreconditions[flatIndex] = (prekeys[j], AllActions[i].Preconditions.States[prekeys[j]]);
                    flatIndex++;
                }
                prekeys.Dispose();

                NativeArray<FixedString32Bytes> postkeys = AllActions[i].Postconditions.States.GetKeyArray(Allocator.Temp);
                flatIndex = 0.CalculateFlatIndex(i, _postconditionsWidth);

                for (int j = 0; j < postkeys.Length; j++)
                {
                    AllPostconditions[flatIndex] = (postkeys[j], AllActions[i].Postconditions.States[postkeys[j]]);
                    flatIndex++;
                }
                postkeys.Dispose();
            }
        }
        #endregion

        #region Modify and validate states
        /// <summary>
        /// Method to fetch the highest world state and validate all the goals
        /// </summary>
        public void ValidateWorldStatesAndGoals()
        {
            ViableGoals.Clear();

            NativeArray<FixedString32Bytes> wsKeys = NestedWorldStates.GetKeyArray(Allocator.Temp);
            FixedString32Bytes highestWSKey = wsKeys[0];

            //---Get highest priority worldState---
            for (int i = 0; i < wsKeys.Length; i++)
            {
                if (NestedWorldStates[wsKeys[i]].Priority > NestedWorldStates[highestWSKey].Priority)
                    highestWSKey = wsKeys[i];
            }
            wsKeys.Dispose();

            CurrentWorldState = NestedWorldStates[highestWSKey];

            //---Validate all goals---
            NativeArray<FixedString32Bytes> keys = NestedGoals.GetKeyArray(Allocator.Temp);
            foreach (FixedString32Bytes key in keys)
            {
                if (!NestedGoals[key].IsValid) continue;

                ViableGoals.Enqueue(new ViableWorldState(keys.IndexOf(key), NestedGoals[key].Priority, key));
            }
            keys.Dispose();
        }

        /// <summary>
        /// Updates a given world state priority
        /// </summary>
        public void UpdateWorldState(FixedString32Bytes key, float priority)
        {
            WorldState state = NestedWorldStates[key];
            state.SetPriority(priority);
            NestedWorldStates[key] = state;
        }

        /// <summary>
        /// Updates a given goals valid flag and priority
        /// </summary>
        public void UpdateGoal(FixedString32Bytes key, bool valid, float priority)
        {
            WorldState goal = NestedGoals[key];
            goal.Validate(valid);
            goal.SetPriority(priority);
            NestedGoals[key] = goal;
        }
        #endregion

        public void Dispose()
        {
            if (ViableActions.IsCreated)
                ViableActions.Dispose();

            if (!CurrentActionPlan.IsEmpty)
                CurrentActionPlan.Dispose();

            if (!_alternativePlan.IsEmpty)
                _alternativePlan.Dispose();

            if (NestedWorldStates.IsCreated) NestedWorldStates.Dispose();
            if (NestedGoals.IsCreated) NestedGoals.Dispose();

            if(ViableGoals.IsCreated) ViableGoals.Dispose();
            if (AllGoals.Length > 0) AllGoals.Dispose();

            CurrentWorldState.Dispose();
            CurrentGoal.Dispose();
        }

        #region Debugging
        public string DescribePlan(float timeTaken, StringBuilder strBuilder)
        {
            int totalCost = 0;

            string ws = CurrentWorldState.ToString(strBuilder);
            string goal = CurrentGoal.ToString(strBuilder);

            strBuilder.Clear();
            strBuilder.Append("\n----ActionPlanner Plan----\n");

            strBuilder.AppendFormat("{0}{1}\n", "\nStart state:  ", !CurrentWorldState.States.IsCreated ? "Null" : ws);

            strBuilder.AppendFormat("{0}{1}\n", "\nGoal state:  ", !CurrentGoal.States.IsCreated ? "Null" : goal);
            strBuilder.Append("\n");

            if (CurrentActionPlan.IsEmpty)
            {
                strBuilder.Append(string.Format("Time taken: {0} ms\n\n", (timeTaken).ToString("f6")));
                strBuilder.Append("---NO PLAN---");
                return strBuilder.ToString();
            }

            string result = strBuilder.ToString();

            strBuilder.Clear();

            for (int i = 0; i < CurrentActionPlan.Length; i++)
            {
                result += string.Format("{0}: {1}\n", i, AllActions[CurrentActionPlan[i]].ToString(strBuilder));
                totalCost += AllActions[CurrentActionPlan[i]].CalculateCost();
            }

            strBuilder.Clear();

            strBuilder.AppendFormat("Plan cost = {0}\n\n", totalCost);
            strBuilder.Append("-----End-----\n\n\n");

            strBuilder.Append(string.Format("Time taken: {0} ms\n\n", (timeTaken).ToString("f6")));

            result += strBuilder.ToString();

            return result;
        }
        #endregion
    }

    /// <summary>
    /// Stores information of a certain action - its index within the list, its cost and its name
    /// </summary>
    public struct ViableAction
    {
        public int Index;
        public FixedString32Bytes Name;
        public int Cost;

        public ViableAction(int index, int cost, FixedString32Bytes name)
        {
            Index = index;
            Cost = cost;
            Name = name;
        }
    }

    /// <summary>
    /// Stores information of a certain world state instance - its index, priority and name
    /// </summary>
    public struct ViableWorldState : IEquatable<ViableWorldState>, IComparable<ViableWorldState>
    {
        public float Priority;
        public int ListIndex;
        public FixedString32Bytes Name;

        public ViableWorldState(int index, float priority, FixedString32Bytes name)
        {
            ListIndex = index;
            Priority = priority;
            Name = name;
        }

        public int CompareTo(ViableWorldState other)
        {
            return other.Priority.CompareTo(Priority);
        }

        public bool Equals(ViableWorldState other)
        {
            return other.Priority == Priority && other.Name == Name && other.ListIndex == ListIndex;
        }
    }
}