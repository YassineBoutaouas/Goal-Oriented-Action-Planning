using System;
using System.Text;
using UnityEngine;

namespace GOAP_DOTS
{
    public class GOAPAgent : MonoBehaviour
    {
        public event Action OnAgentRegistered;
        public void ReleaseAgentRegistered() { OnAgentRegistered = null; }
        public Agent Agent { private set; get; }
        protected StringBuilder _stringBuilder = new StringBuilder();

        public T Create<T>(T agent) where T : Agent
        {
            Agent = agent;
            OnAgentRegistered?.Invoke();

            return agent;
        }

        protected virtual void OnDisable()
        {
            Agent.Dispose();
        }
    }
}