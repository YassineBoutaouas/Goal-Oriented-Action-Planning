using Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Native
{
    public class GoTo : ActionBase
    {
        private Worker _worker;

        private NavMeshAgent _agent;
        private Animator _animator;
        private Target _target;
        private TargetManager.TargetType _targetType;

        private NavMeshPath _currentPath;

        public GoTo(Worker worker, string actionName, TargetManager.TargetType type, NavMeshAgent agent, Animator animator) : base(actionName, 10)
        {
            _worker = worker;

            _animator = animator;

            _agent = agent;
            _currentPath = new NavMeshPath();

            _targetType = type;

            Effects.Add("CurrentTarget", (int)type, null);
        }

        public override bool Validate()
        {
            _target = TargetManager.Instance.TryGetTarget(_targetType);

            if (_target == null) return false;

            return _agent.CalculatePath(_target.Position, _currentPath);
        }

        public override int CalculateCost() { return (int)_currentPath.GetPathDistance(_agent.transform.position); }

        public override IEnumerator OnActionStart()
        {
            _animator.CrossFade("Movement", 0.05f);
            _animator.SetFloat("ForwardSpeed", 1);
            _agent.StopAgent();
            yield return null;
        }

        public override IEnumerator OnActionExecute()
        {
            _agent.SetAgentPath(_currentPath, 0.1f, 3f, 3f);

            while (Vector3.Distance(_target.Position, _agent.transform.position) > 1f)
                yield return null;
        }

        public override IEnumerator OnActionEnd()
        {
            _agent.StopAgent();
            _animator.SetFloat("ForwardSpeed", 0);
            yield return null;

            _worker._workerAgent.RotateTowards(_target.transform.forward);

            _target.Free();
            _worker.ReleaseTarget();
        }

        public override IEnumerator OnActionCancelled()
        {
            _agent.StopAgent();
            _animator.SetFloat("ForwardSpeed", 0);
            yield return null;

            _worker._workerAgent.RotateTowards(_target.transform.forward);

            _target.Free();
            _worker.ReleaseTarget();
        }
    }
}