using System;

namespace GOAP_Nez_Deprecated
{
    public class NezAStarStorage
    {
        //The maximum amount of nodes that can be stored
        private const int MAX_NODES = 128;

        //Store open and closed nodes in seperate arrays to be able to go throught them
        private AStarNode[] _openNodes = new AStarNode[MAX_NODES];
        private AStarNode[] _closedNodes = new AStarNode[MAX_NODES];

        int _numOpenedNodes; //number of open nodes
        int _numClosedNodes; //number of processed/closed nodes
                             //int n { get { return _openNodes.Length; } }

        int _lastFoundOpened; //index of last node that is found to be opened
        int _lastFoundClosed; //index of last node that is found to be processed/closed

        internal NezAStarStorage() { }

        public void Clear()
        {
            for (int i = 0; i < _numOpenedNodes; i++)
            {
                Pool<AStarNode>.Free(_openNodes[i]);
                _openNodes[i] = null;
            }

            for (int i = 0; i < _numClosedNodes; i++)
            {
                Pool<AStarNode>.Free(_closedNodes[i]);
                _closedNodes[i] = null;
            }

            _numOpenedNodes = _numClosedNodes = 0;
            _lastFoundClosed = _lastFoundOpened = 0;
        }

        public AStarNode FindOpenNode(AStarNode node) //something is wrong - method just checks if the node is within the list of opened nodes and return it
        {
            for (int i = 0; i < _numOpenedNodes; i++)
            {
                //long care = ~node.WorldState.DontCare;
                if (node.Equals(_openNodes[i])) //(node.WorldState.Values & care) == (_openNodes[i].WorldState.Values & care))
                {
                    _lastFoundOpened = i; //author made mistake
                    return _openNodes[i]; //author made mistake
                }
            }

            return null;
        }

        public AStarNode FindClosedNode(AStarNode node)
        {
            for (int i = 0; i < _numClosedNodes; i++)
            {
                if (node.Equals(_closedNodes[i]))
                {
                    _lastFoundClosed = i;
                    return _closedNodes[i];
                }
            }

            return null;
        }

        public bool HasOpenedNodes() { return _numOpenedNodes > 0; }
        public bool HasClosedNodes() { return _numClosedNodes > 0; }

        public void RemoveOpenNode(AStarNode node) //for what??
        {
            if (HasOpenedNodes()) // _numOpenedNodes > 0
            {
                _openNodes[_lastFoundOpened] = _openNodes[_numOpenedNodes - 1];
                _numOpenedNodes--;
            }
            //_numOpened--; no need, if there are none value would go below 0!
        }

        public void RemoveClosedNode(AStarNode node)
        {
            if (HasOpenedNodes()) //_numClosedNodes > 0
            {
                _closedNodes[_lastFoundClosed] = _closedNodes[_numClosedNodes - 1];
                _numClosedNodes--;
            }
            //numClosedNodes--; no need, if there are none value would go below 0!
        }

        public bool IsOpen(AStarNode node) { return Array.IndexOf(_openNodes, node) > -1; }

        public bool IsClosed(AStarNode node) { return Array.IndexOf(_closedNodes, node) > -1; }

        public void AddToOpenList(AStarNode node) { _openNodes[_numOpenedNodes++] = node; }

        public void AddToClosedList(AStarNode node) { _closedNodes[_numClosedNodes++] = node; }

        public AStarNode RemoveCheapestOpenNode()
        {
            int lowestVal = int.MaxValue;

            _lastFoundOpened = -1;

            for (int i = 0; i < _numOpenedNodes; i++)
            {
                if (_openNodes[i].CostSoFarAndHeuristicCost < lowestVal)
                {
                    lowestVal = _openNodes[i].CostSoFarAndHeuristicCost;
                    _lastFoundOpened = i;
                }
            }

            AStarNode cheapestOpenNode = _openNodes[_lastFoundOpened];
            RemoveOpenNode(cheapestOpenNode);

            return cheapestOpenNode;
        }
    }
}