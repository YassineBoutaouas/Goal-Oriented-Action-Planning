using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GOAP_Nez_Deprecated
{
    public class ActionPlanner
    {
        public const int MAX_CONDITIONS = 64; //corresponds to the length of the bitmasks stored within the world state

        /// <summary>
        /// The names associated with all the world states
        /// </summary>
        public string[] ConditionNames = new string[MAX_CONDITIONS];

        private List<GOAPAction> _actions = new List<GOAPAction>(); //should also not exceed 64 actions!

        private List<GOAPAction> _viableActions = new List<GOAPAction>();

        /// <summary>
        /// Preconditions for all actions - 64 possible world states with 64 possible conditions each????
        /// </summary>
        private NezWorldState[] _preConditions = new NezWorldState[MAX_CONDITIONS];

        /// <summary>
        /// Postconditions for all actions
        /// </summary>
        private NezWorldState[] _postConditions = new NezWorldState[MAX_CONDITIONS];

        /// <summary>
        /// Number of world state atoms
        /// </summary>
        private int _numConditionNames;

        public ActionPlanner()
        {
            _numConditionNames = 0;

            for (int i = 0; i < MAX_CONDITIONS; ++i)
            {
                ConditionNames[i] = null;
                _preConditions[i] = NezWorldState.CreateWorldState(this);
                _postConditions[i] = NezWorldState.CreateWorldState(this);
            }
        }

        /// <summary>
        /// Convenient wrapper method for fetching a world state
        /// </summary>
        /// <returns></returns>
        public NezWorldState CreateWorldState() { return NezWorldState.CreateWorldState(this); }

        public void AddAction(GOAPAction action)
        {
            int actionID = FindActionIndex(action);
            if (actionID == -1)
            {
                Debug.LogWarning("Could not find or create action!");
                return;
            }//throw new KeyNotFoundException("Could not find or create action");

            foreach (Tuple<string, bool> precondition in action._preConditions)
            {
                int conditionID = FindConditionNameIndex(precondition.Item1); //Item1 in this case is the string value stored in the tuple, Item2 is the bool itself
                if (conditionID == -1)
                {
                    Debug.LogWarning("Could not find or create conditionName!");
                    continue;
                }

                _preConditions[actionID].SetConditionValues(conditionID, precondition.Item2);
            }

            foreach (Tuple<string, bool> postCondition in action._postConditions)
            {
                int conditionID = FindConditionNameIndex(postCondition.Item1); //Item1 in this case is the string value stored in the tuple, Item2 is the bool itself
                if (conditionID == -1)
                {
                    Debug.LogWarning("Could not find or create conditionName!");
                    continue;
                }

                _preConditions[actionID].SetConditionValues(conditionID, postCondition.Item2);
            }
        }

        public Stack<GOAPAction> Plan(NezWorldState startState, NezWorldState goalState, List<AStarNode> selectedNodes = null)
        {
            _viableActions.Clear();
            for (int i = 0; i < _actions.Count; i++)
            {
                if (_actions[i].Validate())
                    _viableActions.Add(_actions[i]);
            }

            return NezAStar.Plan(this, startState, goalState, selectedNodes);
        }

        internal NezWorldState ApplyPostConditions(ActionPlanner planner, int actionNumber, NezWorldState fromState)
        {
            NezWorldState postCondition = planner._postConditions[actionNumber];
            long unaffectedState = postCondition.DontCare;
            long affectedState = ~unaffectedState;

            fromState.Values = (fromState.Values & unaffectedState) | (postCondition.Values & affectedState);
            fromState.Values &= postCondition.DontCare;

            return fromState;
        }

        internal List<AStarNode> GetPossibleTransitions(NezWorldState fromState)
        {
            List<AStarNode> possibleTransitions = ListPool<AStarNode>.Obtain();
            for (int i = 0; i < _viableActions.Count; ++i)
            {
                //see if precondition is met
                NezWorldState precondition = _preConditions[i];
                long care = ~precondition.DontCare;
                bool met = ((precondition.Values & care) == (fromState.Values & care));
                if (met)
                {
                    AStarNode node = Pool<AStarNode>.Obtain();
                    node.Action = _viableActions[i];
                    node.CostSoFar = _viableActions[i].Cost;
                    node.WorldState = ApplyPostConditions(this, i, fromState);
                    possibleTransitions.Add(node);
                }
            }

            return possibleTransitions;
        }

        internal int FindConditionNameIndex(string conditionName)
        {
            int index;
            for (index = 0; index < _numConditionNames; ++index)
            {
                if (string.Equals(ConditionNames[index], conditionName))
                    return index;
            }

            if (index < MAX_CONDITIONS - 1)
            {
                //if it doesn't exist create a new one!
                ConditionNames[index] = conditionName;
                _numConditionNames++;
                return index;
            }

            return -1;
        }

        internal int FindActionIndex(GOAPAction action)
        {
            int index = _actions.IndexOf(action);
            if (index > -1)
                return index;

            _actions.Add(action);

            return _actions.Count - 1;
        }

        /// <summary>
        /// Describe the Action planner
        /// </summary>
        /// <returns></returns>
        public string Describe()
        {
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < _actions.Count; ++i)
            {
                strBuilder.AppendLine(_actions[i].GetType().Name);

                NezWorldState precondition = _preConditions[i];
                NezWorldState postcondition = _postConditions[i];
                for (int j = 0; j < MAX_CONDITIONS; ++j)
                {
                    if ((precondition.DontCare & (1L << j)) == 0)
                    {
                        bool v = (precondition.Values & (1L << j)) != 0;
                        strBuilder.AppendFormat(" {0}=={1}\n", ConditionNames[j], v ? 1 : 0);
                    }
                }

                for (int j = 0; j < MAX_CONDITIONS; ++j)
                {
                    if ((postcondition.DontCare & (1L << j)) == 0)
                    {
                        bool v = (postcondition.Values & (1L << j)) != 0;
                        strBuilder.AppendFormat(" {0}=={1}\n", ConditionNames[j], v ? 1 : 0);
                    }
                }
            }

            return strBuilder.ToString();
        }
    }
}