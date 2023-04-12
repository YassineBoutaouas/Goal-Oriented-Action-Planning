using System;
using System.Collections.Generic;
using System.Linq;

namespace GOAP_Native
{
    public class AStar
    {
        public const int Max_Iterations = 10000;

        public Pool<Node> PooledNodes = new Pool<Node>(50);

        /// <summary>
        /// Class that holds members and values to represent an item within the graph
        /// </summary>
        public class Node : IComparable, IEquatable<Node>, IPoolingObject
        {
            /// <summary>
            /// The world state that is associated with this node
            /// </summary>
            public WorldState State;

            /// <summary>
            /// The cost so far - distance from starting node (g cost)
            /// </summary>
            public int CostSoFar;

            /// <summary>
            /// The heuristic cost - distance to the end node (h cost)
            /// </summary>
            public int HeuristicCost;

            /// <summary>
            /// The combined costs of distance from the starting node and distance to the end node (g + h)
            /// </summary>
            public int TotalCost;

            /// <summary>
            /// The action associated with this node
            /// </summary>
            public int ActionIndex;

            /// <summary>
            /// Reference to the parent node - needed for graph reconstruction
            /// </summary>
            public Node ParentNode;

            /// <summary>
            /// The depth of the node within the graph - index
            /// </summary>
            public int Depth;

            #region Constructors
            public Node() { State = new WorldState(); }

            public void Set(WorldState state, int actionIndex, int costSoFar, int heuristicCost, int totalCost, Node parentNode, int depth)
            {
                State.States.Clear();

                foreach (var item in state.States)
                    State.States.Add(item.Key, item.Value);

                CostSoFar = costSoFar;
                HeuristicCost = heuristicCost;
                TotalCost = totalCost;
                ActionIndex = actionIndex;
                ParentNode = parentNode;
                Depth = depth;
            }

            public void Set(int actionIndex, int costSoFar, int heuristicCost, int totalCost, Node parentNode, int depth)
            {
                State.States.Clear();

                CostSoFar = costSoFar;
                HeuristicCost = heuristicCost;
                TotalCost = totalCost;
                ActionIndex = actionIndex;
                ParentNode = parentNode;
                Depth = depth;
            }
            #endregion

            #region IEquatable/IComparable
            /// <summary>
            /// Compares the total cost of two nodes to determine the sorting order of the nodes
            /// </summary>
            /// <param name="other"></param>
            /// <returns> smaller than 0 the node comes before the other node, ==0 the node is at the same position, bigger than 0 the node comes after the other node </returns>
            public int CompareTo(object obj) { return TotalCost.CompareTo(((Node)obj).TotalCost); }

            /// <summary>
            /// Comparing two nodes by comparing their associated world states
            /// </summary>
            public bool Equals(Node other) { return State.Equals(other.State); }

            /// <summary>
            /// Method to reset all the values held by this node
            /// </summary>
            public void Reset()
            {
                ActionIndex = -1;
                ParentNode = null;

                CostSoFar = 0;
                HeuristicCost = 0;
                TotalCost = 0;

                Depth = 0;
            }
            #endregion
        }

        public static AStar Instance;

        public static AStar GetInstance()
        {
            Instance ??= new AStar();
            return Instance;
        }

        /// <summary>
        /// Method that retrieves a plan
        /// </summary>
        public List<int> Plan(ActionPlanner planner, WorldState worldState, WorldState goal, ref List<int> actions)
        {
            Node currentNode;

            GetEntryNodes(planner, goal, worldState);

            if (planner._openNodes.Count == 0)
                return null;

            int counter = 0;

            do
            {
                currentNode = planner._openNodes.Dequeue();
                planner._closedNodes.Add(currentNode);

                if (currentNode.State.States.Count == 0) return ReconstructPlan(currentNode, planner, actions);

                for (int i = 0; i < planner.ViableActions.Count; i++)
                {
                    int actionIndex = planner.ViableActions.ElementAt(i);
                    IAction viableAction = planner.AllActions[actionIndex];

                    if (!viableAction.Effects.IsContained(currentNode.State)) continue;

                    Node newNeighbor = PooledNodes.Obtain();
                    newNeighbor.Set(actionIndex, 0, 0, 0, currentNode, currentNode.Depth + 1);

                    InheritPreconditions(newNeighbor, currentNode.State, worldState, viableAction);

                    if (planner._closedNodes.Contains(newNeighbor))
                    {
                        PooledNodes.Free(newNeighbor);
                        continue;
                    }

                    int combinedCost = currentNode.CostSoFar + viableAction.CalculateCost();
                    int neighborHeuristic = CalculateHeuristicCost(newNeighbor);
                    int totalNeighborcost = combinedCost + neighborHeuristic;

                    bool openNode = planner._openNodes.Contains(newNeighbor);

                    if (!openNode || currentNode.CostSoFar + neighborHeuristic < combinedCost)
                    {
                        newNeighbor.CostSoFar = combinedCost;
                        newNeighbor.HeuristicCost = neighborHeuristic;
                        newNeighbor.TotalCost = totalNeighborcost;
                        newNeighbor.ParentNode = currentNode;
                        newNeighbor.Depth = currentNode.Depth + 1;

                        if (!openNode)
                        {
                            planner._openNodes.Enqueue(newNeighbor);
                            continue;
                        }

                        planner._openNodes.Remove(newNeighbor);
                        planner._openNodes.Enqueue(newNeighbor);
                    }

                }
            } while (planner._openNodes.Any() && ++counter < Max_Iterations);

            Clear(planner);
            return null;
        }

        private void GetEntryNodes(ActionPlanner planner, WorldState goal, WorldState worldState)
        {
            foreach (int actionIndex in planner.ViableActions)
            {
                IAction action = planner.AllActions[actionIndex];

                if (!goal.IsContained(action.Effects)) continue;

                Node goalNode = PooledNodes.Obtain();
                goalNode.Set(actionIndex, action.CalculateCost(), 0, 0, null, 1);

                AccumulatePreconditions(action, goalNode.State, worldState);

                goalNode.HeuristicCost = CalculateHeuristicCost(goalNode);

                planner._openNodes.Enqueue(goalNode);
            }
        }

        /// <summary>
        /// Adds the preconditions of an action to the current nodes world state
        /// </summary>
        private void AccumulatePreconditions(IAction action, WorldState goalState, WorldState originState)
        {
            foreach (string key in action.Preconditions.States.Keys)
            {
                bool contains = action.Effects.States.TryGetValue(key, out IState state);

                if (contains && action.Preconditions.States[key].Equals(state))
                    continue;

                if (!contains && ContainsKeyValue(originState, key, action.Preconditions.States[key]))
                    continue;

                if (!goalState.States.TryAdd(key, action.Preconditions.States[key]))
                    goalState.States[key] = action.Preconditions.States[key];
            }
        }

        /// <summary>
        /// Method that passes the preconditions from a previous action the given node
        /// </summary>
        private void InheritPreconditions(Node node, WorldState parentState, WorldState originState, IAction action)
        {
            foreach (string key in parentState.States.Keys)
            {
                if (ContainsKeyValue(action.Effects, key, parentState.States[key]))
                    continue;

                if (!node.State.States.TryAdd(key, parentState.States[key]))
                    node.State.States[key] = parentState.States[key];
            }

            AccumulatePreconditions(action, node.State, originState);
        }

        /// <summary>
        /// Method to check if a world state contains a key and a value for the given key
        /// </summary>
        private bool ContainsKeyValue(WorldState state, string key, IState value)
        {
            if (!state.States.TryGetValue(key, out IState mappedValue)) return false;

            if (!value.Equals(mappedValue)) return false;

            return true;
        }

        /// <summary>
        /// Method to reconstruct the action plan
        /// </summary>
        private List<int> ReconstructPlan(Node goalNode, ActionPlanner planner, List<int> actions)
        {
            int totalActionsInPlan = goalNode.Depth;

            actions.Clear();

            Node currentNode = goalNode;

            for (int i = 0; i < totalActionsInPlan; i++)
            {
                actions.Add(currentNode.ActionIndex);

                currentNode = currentNode.ParentNode;
            }

            Clear(planner);

            return actions;
        }

        /// <summary>
        /// Calculates the heuristic cost from a starting worldstate to a goalstate
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private int CalculateHeuristicCost(Node node) { return node.State.States.Count; }

        /// <summary>
        /// Clears the cached planners list of opennodes and closednodes
        /// </summary>
        private void Clear(ActionPlanner planner)
        {
            while (planner._openNodes.Any())
                PooledNodes.Free(planner._openNodes.Dequeue());

            foreach (Node closed in planner._closedNodes)
                PooledNodes.Free(closed);

            planner._closedNodes.Clear();
        }
    }
}