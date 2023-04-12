using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Extensions;

namespace GOAP_DOTS
{
    /// <summary>
    /// Static class that provides methods to find a plan and to find a goal. 
    /// Utilizes the job system
    /// </summary>
    public static class AStar
    {
        public const int Max_Iterations = 10000;

        /// <summary>
        /// Method to find a plan
        /// </summary>
        public static void Plan(ActionPlanner planner, NativeList<int> pathActions)
        {
            //---Get-Goal---
            if (planner.ViableGoals.IsEmpty)
                return;

            JobHandle jobHandle;

            GetGoalJob getGoal = new GetGoalJob(planner);

            jobHandle = getGoal.Schedule();

            jobHandle.Complete();

            planner.CurrentGoal.Name = getGoal.FetchedGoal[0].Name;
            planner.CurrentGoal.Priority = getGoal.FetchedGoal[0].Priority;
            getGoal.Dispose();

            //---Get-Plan---
            if (getGoal.Goal.States.IsEmpty)
                return;

            FindPathJob findPathJob = new FindPathJob(planner, pathActions);

            jobHandle = findPathJob.Schedule();

            jobHandle.Complete();

            if (!pathActions.IsEmpty)
                return;

            Plan(planner, pathActions);
        }

        /// <summary>
        /// Retrieves the highest priority goal
        /// </summary>
        public static void GetGoal(ActionPlanner planner)
        {
            JobHandle jobHandle;

            GetGoalJob getGoal = new GetGoalJob(planner);

            jobHandle = getGoal.Schedule();

            jobHandle.Complete();

            planner.CurrentGoal.Name = getGoal.FetchedGoal[0].Name;
            planner.CurrentGoal.Priority = getGoal.FetchedGoal[0].Priority;

            getGoal.Dispose();
        }

        /// <summary>
        /// Represents a desired state of the world
        /// </summary>
        public struct Node : IEquatable<Node>, IComparable<Node>, IDisposable
        {
            public WorldState GoalState;

            public int CostSoFar;
            public int HeuristicCost;
            public int TotalCost;

            public int ActionIndex;

            public int ParentIndex;

            public int Depth;

            public Node(int actionIndex, int costSoFar, int heuristic, int totalCost, int parentIndex, int depth)
            {
                GoalState = new WorldState("", 1, Allocator.Temp, true);

                ActionIndex = actionIndex;

                CostSoFar = costSoFar;
                HeuristicCost = heuristic;
                TotalCost = totalCost;

                ParentIndex = parentIndex;
                Depth = depth;
            }

            public void Dispose()
            {
                CostSoFar = 0;
                HeuristicCost = 0;
                TotalCost = 0;

                ActionIndex = 0;
                ParentIndex = 0;
                Depth = 0;
            }

            public bool Equals(Node other) { return GoalState.Equals(other.GoalState); }

            public override string ToString() { return ActionIndex.ToString(); }

            public int CompareTo(Node other) { return other.TotalCost.CompareTo(TotalCost); }
        }

        /// <summary>
        /// Identifier class to easily find an associated node instance
        /// </summary>
        public struct PathNodeIdentifier : IComparable<PathNodeIdentifier>, IEquatable<PathNodeIdentifier>
        {
            public int PathNodesIndex;
            public int Cost;

            public PathNodeIdentifier(int pathIndex, int cost)
            {
                PathNodesIndex = pathIndex;
                Cost = cost;
            }

            public int CompareTo(PathNodeIdentifier other) { return other.Cost.CompareTo(Cost); }

            public bool Equals(PathNodeIdentifier other) { return other.PathNodesIndex == PathNodesIndex; }
        }

        /// <summary>
        /// Job that gets the highest priority goal
        /// </summary>
        [BurstCompile]
        private struct GetGoalJob : IJob, IDisposable
        {
            private NativePriorityQueue<ViableWorldState> _viableGoals;
            private NativeArray<(FixedString32Bytes, bool)> _allGoals;

            public WorldState Goal;
            private readonly int _goalsWidth;

            public NativeArray<ViableWorldState> FetchedGoal;

            public GetGoalJob(ActionPlanner planner)
            {
                Goal = planner.CurrentGoal;

                _viableGoals = planner.ViableGoals;

                FetchedGoal = new NativeArray<ViableWorldState>(1, Allocator.TempJob);

                _allGoals = planner.AllGoals;
                _goalsWidth = planner._goalsWidth;
            }

            [BurstCompile]
            public void Execute()
            {
                Goal.States.Clear();

                if (_viableGoals.Length == 0)
                    return;

                ViableWorldState viableGoal = _viableGoals.Dequeue();

                int flatIndex = 0.CalculateFlatIndex(viableGoal.ListIndex, _goalsWidth);

                for (int i = flatIndex; i < flatIndex + _goalsWidth; i++)
                {
                    if (!_allGoals[i].Item1.IsEmpty)
                        Goal.Add(_allGoals[i].Item1, _allGoals[i].Item2);
                }

                FetchedGoal[0] = viableGoal;
            }

            public void Dispose()
            {
                FetchedGoal.Dispose();
            }
        }

        /// <summary>
        /// Job to find a valid sequence of actions for a given world state and goal
        /// </summary>
        [BurstCompile]
        private struct FindPathJob : IJob, IDisposable
        {
            #region Members
            private NativeList<ViableAction> _viableActions;

            private readonly ProfilerMarker _marker;

            public NativeList<int> PathActions;

            private WorldState _worldState;
            private WorldState _goal;

            private NativeArray<(FixedString32Bytes, bool)> _allPreconditions;
            private NativeArray<(FixedString32Bytes, bool)> _allPostconditions;

            private readonly int _preconditionsWidth;
            private readonly int _postconditionsWidth;
            #endregion

            public FindPathJob(ActionPlanner planner, NativeList<int> pathActions)
            {
                _marker = new ProfilerMarker("Plan");

                _viableActions = planner.ViableActions;

                _worldState = planner.CurrentWorldState;
                _goal = planner.CurrentGoal;

                PathActions = pathActions;

                //-------------------------------------------------------------------------------------
                _preconditionsWidth = planner._preconditionsWidth;
                _postconditionsWidth = planner._postconditionsWidth;

                _allPreconditions = planner.AllPreconditions;
                _allPostconditions = planner.AllPostconditions;
            }

            [BurstCompile]
            public void Execute()
            {
                _marker.Begin();

                /// goal is already satisfied
                if (_goal.IsContained(_worldState)) return;

                ///List of openNodes contains identifiers for the actual nodes created
                ///List pathNodes are nodes that are newly created
                NativeList<Node> pathNodes = new NativeList<Node>(Allocator.Temp);
                NativePriorityQueue<PathNodeIdentifier> openNodes = new NativePriorityQueue<PathNodeIdentifier>(1, Allocator.Temp);

                GetEntryNodes(pathNodes, openNodes);

                ///We found no actions that could satisfy the goal to begin with, so we stop the search
                if (openNodes.IsEmpty)
                {
                    openNodes.Dispose();
                    pathNodes.Dispose();

                    _marker.End();

                    return;
                }

                Node currentNode;
                NativeHashSet<int> closedNodes = new NativeHashSet<int>(1, Allocator.Temp);

                int counter = 0;
                do
                {
                    ///Get the cheapest node out of openNodes
                    int currentNodeIndex = openNodes.Dequeue().PathNodesIndex;

                    currentNode = pathNodes[currentNodeIndex];
                    closedNodes.Add(currentNodeIndex);

                    ///We check if our goal is reached by checking if the preconditions are empty, since they are ruled out one by one if they are satisfied by the postconditions or the worldstate
                    if (currentNode.GoalState.States.IsEmpty)
                    {
                        ReconstructPlan(currentNode, pathNodes);
                        break;
                    }

                    ///Find actions that satisfy the current node action
                    for (int i = 0; i < _viableActions.Length; i++)
                    {
                        ViableAction viableAction = _viableActions[i];

                        ///Check if the postconditions of an action are contained in the preconditions of the currentnode - the currentnode.conditionstate
                        if (!ConditionsContainedInWorldState(currentNode.GoalState, _allPostconditions, viableAction.Index, _postconditionsWidth)) continue;

                        ///Create a neighbor node, applying the postconditions of the action to the currentnode.targetstate and adding the preconditions of the action to neighbor.conditionState
                        Node neighbor = new Node(viableAction.Index, viableAction.Cost + currentNode.CostSoFar, 0, 0, -1, 0);

                        ///Adds new preconditions if they are valid!
                        InheritPreconditions(neighbor, currentNode.GoalState, viableAction.Index);

                        ///We need to get the index of the currentNeighbor node from the pathNodes position in the array
                        pathNodes.Contains(neighbor, out int currentNeighborIndex);

                        ///Already searched the node!
                        if (closedNodes.Contains(currentNeighborIndex)) continue;

                        int combinedCost = currentNode.CostSoFar + neighbor.CostSoFar; //Combined cost of neighbor and parent
                        int neighborHeuristic = CalculateHeuristicCost(neighbor.GoalState);

                        int totalNeighborcost = combinedCost + neighborHeuristic;

                        int openNodeIndex = -1;
                        for (int j = 0; j < openNodes.Length; j++)
                        {
                            if (openNodes[j].PathNodesIndex != currentNeighborIndex) continue;

                            openNodeIndex = j;
                            break;
                        }

                        if (openNodeIndex == -1 || currentNode.CostSoFar + neighborHeuristic < neighbor.CostSoFar) // not in OPEN OR path is shorter
                        {
                            neighbor.CostSoFar = combinedCost;
                            neighbor.HeuristicCost = neighborHeuristic;
                            neighbor.TotalCost = totalNeighborcost;
                            neighbor.ParentIndex = currentNodeIndex;
                            neighbor.Depth = currentNode.Depth + 1;

                            if (openNodeIndex == -1)
                            {
                                pathNodes.Add(neighbor);
                                openNodes.Enqueue(new PathNodeIdentifier(pathNodes.Length - 1, neighbor.TotalCost));
                                continue;
                            }

                            pathNodes[currentNeighborIndex] = neighbor;
                            openNodes.RemoveAt(openNodeIndex);
                            openNodes.Enqueue(new PathNodeIdentifier(currentNeighborIndex, neighbor.TotalCost));
                        }
                    }

                } while (openNodes.Length > 0 && ++counter < Max_Iterations);

                openNodes.Dispose();
                closedNodes.Dispose();
                pathNodes.Dispose();

                currentNode.Dispose();

                _marker.End();
            }

            public void GetEntryNodes(NativeList<Node> pathNodes, NativePriorityQueue<PathNodeIdentifier> openNodes)
            {
                ///Fetch all of the actions that can satisfy the goal by checking if the goal is contained in the postconditions of an action!
                ///if so we create a new node, passing the worldstate as the state of the node
                ///and applying the actions postconditions to node.state
                for (int i = 0; i < _viableActions.Length; i++)
                {
                    ViableAction viableAction = _viableActions[i];

                    if (!WorldStateContainedInConditions(_goal, _allPostconditions, viableAction.Index, _postconditionsWidth)) continue;

                    Node goalNode = new Node(viableAction.Index, viableAction.Cost, 0, viableAction.Cost, pathNodes.Length + 1, 1);

                    AccumulatePreconditions(goalNode.GoalState, _worldState, viableAction.Index);

                    ///We dont need the whole collection of worldstate variables:
                    ///we only care about the postconditions that are applied -> they can be set as targetstate of the goal node since we know that they will be the outcome of the action
                    ///the preconditions that are added represent the preconditions at that given node
                    ///
                    ///For the entry nodes we add the goal as the node.Targetstate since we already checked if the goal is contained in the actions postconditions
                    ///We further add all of the other postconditions to that node
                    ///
                    ///We can now assume that this is the new worldstate since the node.targetstate contains the postconditions of the action
                    ///The node.conditionstates represent the variables that still have to be satisfied by further actions - 
                    ///In order to set them correctly: If a targetstate sets a value that is different from the precondition - it has to be added
                    ///Otherwise the precondition should check if the worldstate satisfies it
                    pathNodes.Add(goalNode);
                    openNodes.Enqueue(new PathNodeIdentifier(pathNodes.Length - 1, goalNode.TotalCost));
                }
            }

            /// <summary>
            /// Adds the preconditions of a given action to the precondition state of a given node
            /// </summary>
            private void AccumulatePreconditions(WorldState goalState, WorldState originState, int listIndex)
            {
                ///If precondition is contained in postconditions - skip
                ///If precondition is contained in worldstate - skip
                ///Add precondition to goal state of node

                int flatIndex = 0.CalculateFlatIndex(listIndex, _preconditionsWidth);
                for (int x = flatIndex; x < flatIndex + _preconditionsWidth; x++)
                {
                    (FixedString32Bytes, bool) precondition = _allPreconditions[x];

                    ///Check if the key is contained in targetStates
                    ///If not we check outside if it is already satisfied because if it is we do not care about the precondition
                    ///otherwise: The precondition gets added and the next action has to satisfy that precondition
                    ///If the precondition is contained in the targetstate we know that we have already satisfied it and we dont need to add it

                    if (IsConditionSatisfied(precondition.Item1, precondition.Item2, _allPostconditions, listIndex, _postconditionsWidth, out int indexResult))
                        continue;

                    if (indexResult == -1 && ContainsKeyValue(originState, precondition.Item1, precondition.Item2)) //if the key is not present in the postconditions then we check for outside - if it is satisfied we skip it
                        continue;

                    if (precondition.Item1.IsEmpty)
                        continue;

                    if (!goalState.States.TryAdd(precondition.Item1, precondition.Item2))
                        goalState.States[precondition.Item1] = precondition.Item2;
                }
            }

            /// <summary>
            /// Inherits the preconditions from a given parentstate
            /// </summary>
            private void InheritPreconditions(Node node, WorldState parentState, int listIndex)
            {
                NativeArray<FixedString32Bytes> keys = parentState.States.GetKeyArray(Allocator.Temp);

                ///Add all of the preconditions of the parent node if they are valid
                foreach (FixedString32Bytes key in keys)
                {
                    ///Check if the key is contained in postcondition state of the node - if so it is already satisfied and we dont need further actions to satisfy it!
                    if (IsConditionSatisfied(key, parentState.States[key], _allPostconditions, listIndex, _postconditionsWidth, out int _))
                        continue;

                    if (!node.GoalState.States.TryAdd(key, parentState.States[key]))
                        node.GoalState.States[key] = parentState.States[key];
                }

                keys.Dispose();

                AccumulatePreconditions(node.GoalState, _worldState, listIndex);
            }

            #region Containment methods
            /// <summary>
            /// Checks if the world state is contained in a given list of conditions 
            /// </summary>
            private bool WorldStateContainedInConditions(WorldState state, NativeArray<(FixedString32Bytes, bool)> conditionsList, int listIndex, int listWidth)
            {
                NativeArray<FixedString32Bytes> keys = state.States.GetKeyArray(Allocator.Temp);

                foreach (FixedString32Bytes key in keys)
                {
                    if (!CheckConditionsContainsKey(key, conditionsList, listIndex, listWidth, out int indexResult)) return false;

                    if (!conditionsList[indexResult].Item2.Equals(state.States[key])) return false;
                }

                keys.Dispose();

                return true;
            }

            /// <summary>
            /// Checks if the conditions of an action are contained in a given world state
            /// </summary>
            private bool ConditionsContainedInWorldState(WorldState state, NativeArray<(FixedString32Bytes, bool)> conditionsList, int listIndex, int listWidth)
            {
                int flatIndex = 0.CalculateFlatIndex(listIndex, listWidth);
                for (int i = flatIndex; i < flatIndex + listWidth; i++)
                {
                    (FixedString32Bytes, bool) tuple = conditionsList[i];

                    if (tuple.Item1.IsEmpty) continue;

                    if (!state.States.TryGetValue(tuple.Item1, out bool value)) return false;

                    if (value.Equals(tuple.Item2)) return true;
                }

                return false;
            }

            private bool IsConditionSatisfied(FixedString32Bytes key, bool value, NativeArray<(FixedString32Bytes, bool)> collection, int listIndex, int listWidth, out int indexResult)
            {
                if (CheckConditionsContainsKey(key, collection, listIndex, listWidth, out indexResult))
                    if (value.Equals(collection[indexResult].Item2))
                        return true;

                return false;
            }

            private bool ContainsKeyValue(WorldState state, FixedString32Bytes key, bool value)
            {
                if (key.IsEmpty) return false;

                if (!state.States.TryGetValue(key, out bool mappedValue)) return false;

                if (!value.Equals(mappedValue)) return false;

                return true;
            }

            /// <summary>
            /// Checks if a tuple, whose key equals a given key, exists in a given array
            /// </summary>
            public bool CheckConditionsContainsKey(FixedString32Bytes key, NativeArray<(FixedString32Bytes, bool)> collection, int listIndex, int listwidth, out int indexResult)
            {
                indexResult = -1;

                if (key.IsEmpty) return false;

                int i = 0.CalculateFlatIndex(listIndex, listwidth);
                for (int x = i; x < i + listwidth; x++)
                {
                    if (collection[x].Item1 == key)
                    {
                        indexResult = x;
                        return true;
                    }
                }

                return false;
            }
            #endregion

            /// <summary>
            /// Calculates the heuristic cost of two given world states
            /// </summary>
            private int CalculateHeuristicCost(WorldState to) { return to.States.Count; }

            /// <summary>
            /// Constructs a plan if a path has been found
            /// </summary>
            private void ReconstructPlan(Node goalNode, NativeList<Node> pathNodes)
            {
                Node currentNode = goalNode;
                int totalActionsInPlan = goalNode.Depth;

                PathActions.Clear();

                for (int i = 0; i < totalActionsInPlan; i++)
                {
                    PathActions.Add(currentNode.ActionIndex);

                    if (currentNode.ParentIndex > pathNodes.Length - 1) break;

                    currentNode = pathNodes[currentNode.ParentIndex];
                }

                currentNode.Dispose();
            }

            public void Dispose() { }
        }
    }
}