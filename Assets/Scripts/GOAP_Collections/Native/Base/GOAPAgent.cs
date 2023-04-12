using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;

namespace GOAP_Native
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

        public void RotateTowards(Vector3 target)
        {
           transform.rotation = Quaternion.LookRotation(Vector3.Scale((-target).normalized, Vector3.right + Vector3.forward));
        }
    }
}