using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Native
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class WorkerAgent : GOAPAgent
    {
        public Worker _worker;

        protected void Start()
        {
            _worker = Create(new Worker(this, _stringBuilder));
            Agent.Plan(true);

            Agent.OnPlanEnd += () => Agent.Plan(true);
        }
    }
}