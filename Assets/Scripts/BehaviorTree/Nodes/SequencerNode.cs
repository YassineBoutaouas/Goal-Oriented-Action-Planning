using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behavior_Tree
{
    public class SequencerNode : CompositeNode
    {
        private int currentChild;

        protected override void OnStart()
        {
            currentChild = 0;
        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            Node child = Children[currentChild];

            switch (child.Update())
            {
                case State.Running:
                    return State.Running;
                case State.Failed:
                    return State.Failed;
                case State.Success:
                    currentChild++;
                    break;
            }

            return currentChild == Children.Count ? State.Success : State.Running;
        }
    }
}