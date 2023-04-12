using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOAP_Refactored
{
    public class ActionPlanner
    {
        public readonly HashSet<ActionBase> AllActions = new HashSet<ActionBase>(); //List of all actions
        public readonly HashSet<ActionBase> ViableActions = new HashSet<ActionBase>(); //List of viable actions

        //a list of worldstates that contain dictionaries with preconditions - as a way to save all preconditions by any action
        private readonly Dictionary<ActionBase, WorldState> _allPreConditions = new Dictionary<ActionBase, WorldState>();

        //a list of worldstates that contain dictionaries with postconditions - as a way to save all postconditions by any action
        private readonly Dictionary<ActionBase, WorldState> _allPostConditions = new Dictionary<ActionBase, WorldState>();

        private readonly AStar _aStar;
        private List<AStar.Node> _currentPossibleTransitions = new List<AStar.Node>();

        public PriorityQueue<AStar.Node> _openNodes = new PriorityQueue<AStar.Node>();
        public HashSet<AStar.Node> _closedNodes = new HashSet<AStar.Node>();

        private readonly StringBuilder _strBuilder = new StringBuilder();

        public ActionPlanner() { _aStar = AStar.GetInstance(); }

        #region Action list methods
        /// <summary>
        /// Adds an action to the list of actions
        /// </summary>
        public void AddAction(ActionBase action)
        {
            if (!AllActions.Add(action)) return;

            _allPreConditions.TryAdd(action, action.PreConditions);
            _allPostConditions.TryAdd(action, action.PostConditions);
        }

        /// <summary>
        /// Removes an action from the list of actions
        /// </summary>
        /// <param name="action"></param>
        public void RemoveAction(ActionBase action)
        {
            if (!AllActions.Remove(action)) return;

            _allPreConditions.Remove(action);
            _allPostConditions.Remove(action);
        }
        #endregion

        /// <summary>
        /// Retrieves a plan from the AStar algorithm and returns it
        /// </summary>
        /// <param name="startState"></param>
        /// <param name="goalState"></param>
        /// <returns></returns>
        public Stack<ActionBase> Plan(WorldState startState, WorldState goalState, ref Stack<ActionBase> actions)
        {
            if (startState.States == null || goalState.States == null) return null;

            ViableActions.Clear();
            foreach (ActionBase a in AllActions)
            {
                a.PreConditions.Apply();
                a.PostConditions.Apply();
                if (!a.Validate()) continue;

                ViableActions.Add(a);
            }

            return _aStar.Plan(this, startState, goalState, ref actions);
        }

        /// <summary>
        /// Retrieves the possible transitions based on the worldstate of a given node
        /// </summary>
        /// <param name="preconditions"></param>
        /// <returns></returns>
        internal List<AStar.Node> GetPossibleTransitions(WorldState preconditions, WorldState originState)
        {
            _currentPossibleTransitions.Clear();
            foreach (ActionBase viableAction in ViableActions)
            {
                //see if precondition is met
                if (_allPostConditions[viableAction].IsContained(preconditions)) //only check if the worldstate is contained in fromstate - only compare what we care for
                {
                    ///neighbor gets obtained from pooledNodes - the state of the node gets cleared and the states get copied from originstate.
                    ///The post conditions are then applied to the newly modified neighbor.state

                    AStar.Node neighbor = _aStar.PooledNodes.Obtain();

                    neighbor.Set(originState, viableAction, viableAction.CalculateCost(), 0, 0, null, 0); //copies the values from origin state into node.state
                    //ApplyPostConditions(viableAction, neighbor.State); //applies the post conditions to the node.state

                    foreach (string key in viableAction.PostConditions.States.Keys)
                        neighbor.State.CopyStateValue(key, viableAction.PostConditions.States[key]);

                    _currentPossibleTransitions.Add(neighbor);
                }
            }

            return _currentPossibleTransitions;
        }

        #region Debugging
        public override string ToString()
        {
            _strBuilder.Clear();
            _strBuilder.Append("---ACTIONS---\n");

            foreach (ActionBase actionBase in AllActions)
            {
                _strBuilder.Append(actionBase.ToString()).Append("\n");
            }

            return _strBuilder.ToString();
        }

        public string DescribePlan(Stack<ActionBase> plan, WorldState startState, WorldState goal)
        {
            int totalCost = 0;

            _strBuilder.Clear();
            _strBuilder.Append("\n----ActionPlanner Plan----\n");

            _strBuilder.AppendFormat("{0}{1}\n", "\nStart state:  ", startState.States == null ? "Null" : startState.ToString());
            _strBuilder.AppendFormat("{0}{1}\n", "\nGoal state:  ", goal.States == null ? "Null" : goal.ToString());
            _strBuilder.Append("\n");

            if (plan == null)
            {
                _strBuilder.Append("---NO PLAN---");
                return _strBuilder.ToString();
            }

            for (int i = 0; i < plan.Count; i++)
            {
                _strBuilder.AppendFormat("{0}: {1}\n", i, plan.ElementAt(i).ToString());
                totalCost += plan.ElementAt(i).CalculateCost();
            }

            _strBuilder.AppendFormat("Plan cost = {0}\n\n", totalCost);
            _strBuilder.Append("-----End-----\n");

            return _strBuilder.ToString();
        }
        #endregion
    }
}