using UnityEngine;

namespace Behavior_Tree
{
    public abstract class Node : ScriptableObject
    {
        public enum State { Running, Failed, Success }

        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;
        [HideInInspector] public string GUID;
        [HideInInspector] public Vector2 Position;

        [HideInInspector] public Blackboard blackboard;

        [TextArea] public string Description;

        public State Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running)
            {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual Node Clone()
        {
            return Instantiate(this);
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();
    }
}