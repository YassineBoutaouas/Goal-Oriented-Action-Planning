using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GOAP_Nez_Deprecated
{
    public abstract class NAgent
    {
        public Stack<GOAPAction> Actions;
        protected ActionPlanner _planner;

        private StringBuilder _stringBuilder;

        public NAgent()
        {
            _planner = new ActionPlanner();
            _stringBuilder = new StringBuilder();
        }

        public bool Plan(bool debugPlan = false)
        {
            List<AStarNode> nodes = null;
            if (debugPlan)
                nodes = new List<AStarNode>();

            Actions = _planner.Plan(GetWorldState(), GetGoalState(), nodes); //gets the actions it should execute from the planner

            if (nodes != null && nodes.Count > 0)
            {
                _stringBuilder.Clear();
                _stringBuilder.Append("----ActionPlanner plan----\n");
                _stringBuilder.AppendFormat("plan cost = {0}\n", nodes[nodes.Count - 1].CostSoFar);
                _stringBuilder.AppendFormat("{0}\t{1}\n", "start".PadRight(15), GetWorldState().Describe(_planner));

                for (int i = 0; i < nodes.Count; i++)
                {
                    _stringBuilder.AppendFormat("{0}: {1}\t{2}", i, nodes[i].Action.GetType().Name.PadRight(15), nodes[i].WorldState.Describe(_planner));
                    Pool<AStarNode>.Free(nodes[i]);
                }

                Debug.Log(_stringBuilder.ToString());
                _stringBuilder.Clear();
            }

            return HasActionPlan();
        }

        /// <summary>
        /// Does the agent have any actions
        /// </summary>
        /// <returns></returns>
        public bool HasActionPlan() { return Actions != null && Actions.Count > 0; }

        /// <summary>
        /// returns the current world state
        /// </summary>
        /// <returns></returns>
        public abstract NezWorldState GetWorldState(); //world state has to be specified by the agent whenever a plan is proposed by the planner

        /// <summary>
        /// returns the goal state that the agent wants to achieve
        /// </summary>
        /// <returns></returns>
        public abstract NezWorldState GetGoalState();
    }
}