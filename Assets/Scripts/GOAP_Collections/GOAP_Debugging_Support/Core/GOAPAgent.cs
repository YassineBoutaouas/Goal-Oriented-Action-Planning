using System;
using UnityEngine;

namespace GOAP_Native_Debugging
{
    public class GOAPAgent : MonoBehaviour
    {
        public event Action OnAgentRegistered;
        public void ReleaseAgentRegistered() { OnAgentRegistered = null; }
        public Agent Agent { private set; get; }

        public T Create<T>(T agent) where T : Agent
        {
            Agent = agent;
            OnAgentRegistered?.Invoke();

            return agent;
        }
    }
}