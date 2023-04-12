using System;
using System.Collections.Generic;

/* from: http://theory.stanford.edu/~amitp/GameProgramming/ImplementationNotes.html
		OPEN = priority queue containing START
		CLOSED = empty set
		while lowest rank in OPEN is not the GOAL:
		  current = remove lowest rank item from OPEN
		  add current to CLOSED
		  for neighbors of current:
		    cost = g(current) + movementcost(current, neighbor)
		    if neighbor in OPEN and cost less than g(neighbor):
		      remove neighbor from OPEN, because new path is better
		    if neighbor in CLOSED and cost less than g(neighbor): **
		      remove neighbor from CLOSED
		    if neighbor not in OPEN and neighbor not in CLOSED:
		      set g(neighbor) to cost
		      add neighbor to OPEN
		      set priority queue rank to g(neighbor) + h(neighbor)
		      set neighbor's parent to current
        */

namespace GOAP_Refactored
{
    public partial class AStar
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
            public ActionBase ActionBase;

            /// <summary>
            /// Reference to the parent node - needed for graph reconstruction
            /// </summary>
            public Node ParentNode;

            /// <summary>
            /// The depth of the node within the graph - index
            /// </summary>
            public int Depth;

            #region Constructors
            public Node()
            {
                State = new WorldState();
            }

            public void Set(WorldState state, ActionBase actionBase, int costSoFar, int heuristicCost, int totalCost, Node parentNode, int depth)
            {
                State.States.Clear();

                foreach (var item in state.States)
                    State.States.Add(item.Key, item.Value);

                CostSoFar = costSoFar;
                HeuristicCost = heuristicCost;
                TotalCost = totalCost;
                ActionBase = actionBase;
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
            public int CompareTo(object obj)
            {
                return TotalCost.CompareTo(((Node)obj).TotalCost);
            }

            /// <summary>
            /// Comparing two nodes by comparing their associated world states
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(Node other)
            {
                return State.Equals(other.State);
            }

            /// <summary>
            /// Method to reset all the values held by this node
            /// </summary>
            public void Reset()
            {
                ActionBase = null;
                ParentNode = null;

                CostSoFar = 0;
                HeuristicCost = 0;
                TotalCost = 0;

                Depth = 0;
            }

            public override string ToString()
            {
                return string.Format(ActionBase != null ? ActionBase.Name : "EmptyNode");
            }
            #endregion
        }

        public static AStar Instance;

        public static AStar GetInstance()
        {
            Instance ??= new AStar();
            return Instance;
        }

        public Stack<ActionBase> Plan(ActionPlanner planner, WorldState worldState, WorldState goal, ref Stack<ActionBase> actions)
        {
            Node currentNode;

            WorldState currentGoal = new WorldState("", WorldState.True);
            WorldState worldStateCopy = new WorldState(worldState.Name, worldState, worldState.Validation, worldState.Priority);

            int startingHeuristic = CalculateHeuristicCost(worldStateCopy, goal);

            foreach (ActionBase action in planner.ViableActions)
            {
                if (goal.IsContained(action.PostConditions))
                {
                    Node goalNode = PooledNodes.Obtain();
                    goalNode.Set(worldStateCopy, action, action.CalculateCost(), 0, action.CalculateCost(), null, 1);

                    planner._openNodes.Enqueue(goalNode); //get node from pool/ obtain //new Node(worldState, action, action.CalculateCost(), 0, action.CalculateCost(), null, 1)
                }
            }

            if (planner._openNodes.Count == 0)
                return null;

            int counter = 0;

            //run AStar
            do
            {
                currentNode = planner._openNodes.Dequeue();
                planner._closedNodes.Add(currentNode);

                foreach (string key in currentNode.ActionBase.PreConditions.States.Keys)
                {
                    if (worldState.States.ContainsKey(key))
                        if (worldState.States[key].Equals(currentNode.ActionBase.PreConditions.States[key]))
                            continue;

                    currentGoal.Add(key, currentNode.ActionBase.PreConditions.States[key]);
                }

                //Have we reached our goal
                if (currentGoal.IsContained(currentNode.State))
                {
                    return ReconstructPlan(currentNode, planner, actions);
                }

                //Get neighbors!
                List<Node> neighbors = planner.GetPossibleTransitions(currentGoal, currentNode.State); // pool the entire list / obtain

                for (int i = 0; i < neighbors.Count; i++)
                {
                    Node currentNeighbor = neighbors[i];

                    int combinedCost = currentNode.CostSoFar + currentNeighbor.CostSoFar;

                    int neighborheuristic = CalculateHeuristicCost(currentNeighbor.State, currentGoal);
                    int currentCostAndNeighborHeuristic = currentNode.CostSoFar + neighborheuristic;

                    //if the neighbor node is in neither of the lists - we add it to the list of nodes that need to be processed
                    if (!(planner._openNodes.Contains(currentNeighbor) || planner._closedNodes.Contains(currentNeighbor)))
                    {
                        Node newNeighbor = PooledNodes.Obtain();
                        newNeighbor.Set(currentNeighbor.State, currentNeighbor.ActionBase, combinedCost, neighborheuristic, combinedCost + neighborheuristic, currentNode, currentNode.Depth + 1); //get a node from the pool/ obtain

                        planner._openNodes.Enqueue(newNeighbor);
                        continue;
                    }

                    if (currentNeighbor.CostSoFar <= currentCostAndNeighborHeuristic) continue; //adjacentVertex.Distance > currentVertex.Distance + weight

                    //if the neighbor is in open AND it is more expensive than the currentNode.Cost and the neighbor heuristic
                    //we know there is a better node so we remove it from the open list
                    if (planner._openNodes.Contains(currentNeighbor))
                    {
                        planner._openNodes.Remove(currentNeighbor);
                        PooledNodes.Free(currentNeighbor);
                        continue;
                    }

                    //if neighbor is in closed list and cost more than g(neighbor)
                    PooledNodes.Free(currentNeighbor);

                    planner._closedNodes.Remove(currentNeighbor);
                }

                foreach (Node node in neighbors)
                    PooledNodes.Free(node);

                neighbors.Clear(); //return neighbors to pool/ free

            } while (planner._openNodes.Any() && ++counter < Max_Iterations);

            Clear(planner);

            return null;
        }

        private Stack<ActionBase> ReconstructPlan(Node goalNode, ActionPlanner planner, Stack<ActionBase> actions)
        {
            int totalActionsInPlan = goalNode.Depth;
            Stack<ActionBase> backwardsPlan = new Stack<ActionBase>(totalActionsInPlan);

            actions.Clear();

            Node currentNode = goalNode;

            for (int i = 0; i < totalActionsInPlan; i++) //we run the algorithm backwards - we need to get the plan forwards
            {
                backwardsPlan.Push(currentNode.ActionBase);
                currentNode = currentNode.ParentNode;
            }

            for (int i = 0; i < totalActionsInPlan; i++)
            {
                actions.Push(backwardsPlan.Pop());
            }

            backwardsPlan.Clear();
            Clear(planner);

            return actions;
        }

        /// <summary>
        /// Calculates the heuristic cost from a starting worldstate to a goalstate
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private int CalculateHeuristicCost(WorldState from, WorldState to)
        {
            int distance = 0;

            ///in order to compare the values of both world states the amount of states within both of them have to be the same!
            ///OR: they have to be compared if they have the same keys - see if they align with the && operator
            ///
            ///every key that is contained in to state but NOT in from state - +1 distance
            ///if the key is in both - check their value - if the bool is fromState.Equals(toState) == false - +1 to distance
            foreach (string key in to.States.Keys)
            {
                if (!from.States.ContainsKey(key)) { distance++; continue; }

                if (to.States[key].Equals(from.States[key]) == false) distance++;
            }

            return distance;
        }

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