using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using Extensions;

namespace GOAP_DOTS
{
    public class GoTo : ActionBase
    {
        private NavMeshAgent _agent;
        private Animator _animator;

        private string _animationTag;

        private NavMeshPath _currentPath;

        private Func<bool> _atTargetMethod;
        private Vector3 _targetPosition;
        private FixedString32Bytes _conditionTarget;

        public GoTo(string actionName, FixedString32Bytes targetKey, Func<bool> targetMethod, Vector3 target, NavMeshAgent agent, Animator animator, string animationTag, params (FixedString32Bytes, bool)[] preconditions) : base(actionName, 10)
        {
            _animator = animator;
            _animationTag = animationTag;

            _agent = agent;
            _currentPath = new NavMeshPath();
            _targetPosition = target;

            _atTargetMethod = targetMethod;
            _conditionTarget = targetKey;

            for (int i = 0; i < preconditions.Length; i++)
                Preconditions.Add(preconditions[i].Item1, preconditions[i].Item2);

            Postconditions.Add(_conditionTarget, true);
        }

        public override bool Validate() { return _agent.CalculatePath(_targetPosition, _currentPath); }

        public override int CalculateCost() { return (int)_currentPath.GetPathDistance(_agent.transform.position); }

        public override IEnumerator OnActionStart()
        {
            _animator.CrossFade(_animationTag, 0.05f);
            _animator.SetFloat("ForwardSpeed", 1);
            _agent.StopAgent();
            yield return null;
        }

        public override IEnumerator OnActionExecute()
        {
            _agent.SetAgentPath(_currentPath, 0.1f, 6f, 6f);

            while(_atTargetMethod() == false)
                yield return null;
        }

        public override IEnumerator OnActionEnd()
        {
            _agent.StopAgent();
            _animator.SetFloat("ForwardSpeed", 0);
            yield return null;
        }

        public override IEnumerator OnActionCancelled()
        {
            _agent.StopAgent();
            _animator.SetFloat("ForwardSpeed", 0);
            yield return null;
        }

        public override void UpdatePostconditions() { }
    }
}