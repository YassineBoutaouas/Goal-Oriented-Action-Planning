using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behavior_Tree
{
    public class WaitNode : ActionNode
    {
        public float Duration = 1;
        private float startTime = 1;

        protected override void OnStart()
        {
            startTime = Time.time;
        }

        protected override void OnStop()
        {

        }

        protected override State OnUpdate()
        {
            if (Time.time - startTime > Duration)
                return State.Success;

            return State.Running;
        }
    }
}