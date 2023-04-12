using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behavior_Tree
{
    public class RepeatNode : DecoratorNode
    {
        protected override void OnStart()
        {
            
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnUpdate()
        {
            Child.Update();
            
            //Break condition -> how often, while success etc.
            return State.Running;
        }
    }
}