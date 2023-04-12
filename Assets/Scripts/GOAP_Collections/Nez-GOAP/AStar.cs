using System;
using System.Collections.Generic;

namespace GOAP_Nez_Deprecated
{
    public class AStarNode : IComparable<AStarNode>, IEquatable<AStarNode>, IPoolingObject
    {
        /// <summary>
        /// The state of the world at this node
        /// </summary>
        public NezWorldState WorldState;

        /// <summary>
        /// The cost so far
        /// </summary>
        public int CostSoFar;

        //(don't overestimate)
        /// <summary>
        /// The heuristic cost for remaining cost calculations
        /// </summary>
        public int HeuristicCost;

        /// <summary>
        /// The combined costs of CurrentCost + HeuristicCost
        /// </summary>
        public int CostSoFarAndHeuristicCost;

        /// <summary>
        /// The associated action of this node
        /// </summary>
        public GOAPAction Action;

        //for graph building - needed node and world state to know where this node is connected
        public AStarNode ParentNode;
        public NezWorldState ParentWorldState;
        public int Depth; //ID? - How deep are we into the graph?? ´What position are we at?

        #region Comparing
        /// <summary>
        /// Comparing two nodes by comparing their associated world states
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AStarNode other)
        {
            //long care = ~WorldState.DontCare; //invert dontcare bitmask
            //(WorldState.Values & care) == (other.WorldState.Values & care);
            return WorldState.Equals(other.WorldState);
        }

        /// <summary>
        /// Compare two nodes by comparing their associated current heuristic costs
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(AStarNode other) { return CostSoFarAndHeuristicCost.CompareTo(other.CostSoFarAndHeuristicCost); }
        #endregion

        /// <summary>
        /// Reset all the values held by this node
        /// </summary>
        public void Reset()
        {
            Action = null;
            ParentNode = null;

            WorldState = null;
            CostSoFar = 0;
            CostSoFarAndHeuristicCost = 0;
            HeuristicCost = 0;
            ParentWorldState = null;
            Depth = 0;
        }

        /// <summary>
        /// Clone a node based on its class members
        /// </summary>
        /// <returns></returns>
        public AStarNode Clone() { return (AStarNode)MemberwiseClone(); }

        /// <summary>
        /// Display the cost, heuristic and the action associated to this node
        /// </summary>
        /// <returns></returns>
        public override string ToString() { return String.Format("[cost: {0} | heuristic {1}]: {2}", CostSoFar, HeuristicCost, Action); }
    }

    public class NezAStar
    {
        static NezAStarStorage _storage = new NezAStarStorage();

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

        public static Stack<GOAPAction> Plan(ActionPlanner planner, NezWorldState start, NezWorldState goal, List<AStarNode> selectedNodes = null)
        {
            _storage.Clear();
            AStarNode currentNode = Pool<AStarNode>.Obtain(); //starting node?

            currentNode.WorldState = start;
            currentNode.ParentWorldState = start;
            currentNode.CostSoFar = 0; //g
            currentNode.HeuristicCost = CalculateHeuristicCost(start, goal); //h
            currentNode.CostSoFarAndHeuristicCost = currentNode.CostSoFar + currentNode.HeuristicCost; //f
            currentNode.Depth = 1;

            _storage.AddToOpenList(currentNode);

            while (true)
            {
                //nothing left open - failed to find a path
                if (!_storage.HasOpenedNodes())
                {
                    _storage.Clear();
                    return null;
                }

                currentNode = _storage.RemoveCheapestOpenNode();

                _storage.AddToClosedList(currentNode);

                //all done we reached our goal
                if (goal.Equals(currentNode.WorldState))
                {
                    Stack<GOAPAction> plan = ReconstructPlan(currentNode, selectedNodes);
                    _storage.Clear();
                    return plan;
                }

                List<AStarNode> neighbors = planner.GetPossibleTransitions(currentNode.WorldState);

                for (int i = 0; i < neighbors.Count; i++)
                {
                    AStarNode currNeighbor = neighbors[i]; //current neighbor
                    AStarNode openedNeighbor = _storage.FindOpenNode(currNeighbor); //if the currNeighbor is in the list of opened nodes - return that as an open neighbor
                    AStarNode closedNeighbor = _storage.FindClosedNode(currNeighbor); //if the currNeighbor is in the list of closed nodes - return that as a closed neighbor
                    int cost = currentNode.CostSoFar + currNeighbor.CostSoFar; //Add the costs of the neighbor node and the current node to get the total cost

                    //if neighbor in open list and cost less than g(neighbor)
                    if (openedNeighbor != null && cost < openedNeighbor.CostSoFar)
                    {
                        //remove neighbor from open list, because new path is better
                        _storage.RemoveOpenNode(openedNeighbor);
                        openedNeighbor = null;
                    }

                    //if neighbor is in closed and cost less than g(neighbor)
                    if (closedNeighbor != null && cost < openedNeighbor.CostSoFar)
                    {
                        //remove neighbor from closed
                        _storage.RemoveClosedNode(closedNeighbor);
                        //closedNeighbor = null; - shouldnt this be here?
                    }

                    //if neighbor is not in either 
                    if (openedNeighbor == null && closedNeighbor == null)
                    {
                        AStarNode newNeighbor = Pool<AStarNode>.Obtain();
                        newNeighbor.WorldState = currentNode.WorldState;
                        newNeighbor.CostSoFar = cost;
                        newNeighbor.HeuristicCost = CalculateHeuristicCost(currentNode.WorldState, goal);
                        newNeighbor.CostSoFarAndHeuristicCost = newNeighbor.CostSoFar + newNeighbor.HeuristicCost;

                        newNeighbor.Action = currentNode.Action;
                        newNeighbor.ParentWorldState = currentNode.WorldState;
                        newNeighbor.ParentNode = currentNode;
                        newNeighbor.Depth = currentNode.Depth + 1;
                        _storage.AddToOpenList(newNeighbor);
                    }
                }

                //done with neighbors - return it back to pool
                ListPool<AStarNode>.Free(neighbors);
            }
        }

        /// <summary>
        /// internal function to reconstruct the plan by tracing from last node to initial node
        /// </summary>
        /// <returns></returns>
        static Stack<GOAPAction> ReconstructPlan(AStarNode goalNode, List<AStarNode> selectedNodes)
        {
            int totalActionsInPlan = goalNode.Depth - 1;
            Stack<GOAPAction> plan = new Stack<GOAPAction>(totalActionsInPlan);

            AStarNode currentNode = goalNode;
            for (int i = 0; i <= totalActionsInPlan - 1; i++)
            {
                //optionally add the node to the list if we have been passed one
                if (selectedNodes != null)
                    selectedNodes.Add(currentNode.Clone());

                plan.Push(currentNode.Action);
                currentNode = currentNode.ParentNode;
            }

            //our nodes went from the goal back to the start so reverse them
            if (selectedNodes != null)
                selectedNodes.Reverse();

            return plan;
        }

        /// <summary>
        /// Calculate heuristic estimate for remaining distance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        static int CalculateHeuristicCost(NezWorldState from, NezWorldState to)
        {
            long care = ~to.DontCare;
            long difference = (from.Values & care) ^ (to.Values & care);
            int distance = 0;

            for (int i = 0; i < ActionPlanner.MAX_CONDITIONS; ++i)
            {
                if ((difference & (1L << i)) != 0)
                {
                    distance++;
                }
            }

            return distance;
        }
    }
}