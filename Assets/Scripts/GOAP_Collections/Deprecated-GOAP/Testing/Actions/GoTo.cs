using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Refactored
{
    //public class GoTo : Action<WorkerAgent>
    //{
    //    private Func<object> GetTarget;
//
    //    private float _speed;
    //    private float _stoppingDistance;
    //    private float _acceleration;
//
    //    private AnimationHandler _animationHandler;
    //    private NavMeshPath _path;
    //    private NavMeshAgent _agent;
//
    //    public GoTo(WorkerAgent context, Func<object> getTarget, NavMeshAgent agent, string name, float speed, float stoppingDistance, float acceleration, AnimationHandler animationHandler, Vector3 target) : base(context, name, 100)
    //    {
    //        //PostCondition CAN be dynamic, we dont know where we go!
    //        GetTarget = getTarget;
    //        PostConditions.Add("Position", target, GetTarget);
//
    //        _speed = speed;
    //        _stoppingDistance = stoppingDistance;
    //        _acceleration = acceleration;
//
    //        _path = new NavMeshPath();
    //        _agent = agent;
    //        _animationHandler = animationHandler;
    //    }
//
    //    public override bool Validate()
    //    {
    //        return _agent.CalculatePath((Vector3)GetTarget(), _path);
    //    }
//
    //    public override int CalculateCost()
    //    {
    //        return 10; /*(int)_path.GetPathDistance(_agent.transform.position)*/
    //    }
//
    //    public override IEnumerator OnActionStart()
    //    {
    //        _agent.SetAgentPath(_path, 0, _speed, _acceleration);
//
    //        _animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["Movement"].HashID, 0.1f);
//
    //        _animationHandler.Animator.SetFloat(_animationHandler.AnimatorTransitions["ForwardSpeed"], 1f);
//
    //        yield return null;
    //    }
//
    //    public override IEnumerator OnActionExecute()
    //    {
    //        while (Vector3.Distance(_contextObject.transform.position, (Vector3)GetTarget()) > _stoppingDistance)
    //        {
    //            yield return null;
    //            Debug.Log("Still walking");
    //        }
//
    //        _contextObject.transform.position = (Vector3)GetTarget();
    //    }
//
    //    public override IEnumerator OnActionEnd()
    //    {
    //        yield return null;
    //        _animationHandler.Animator.SetFloat(_animationHandler.AnimatorTransitions["ForwardSpeed"], 0);
    //        _agent.StopAgent();
    //    }
//
    //    public override IEnumerator OnActionCancelled()
    //    {
    //        _agent.StopAgent();
//
    //        _animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["WakeUp"].HashID, 0.1f);
//
    //        yield return new WaitForSeconds(_animationHandler.AnimationStates["WakeUp"].Duration);
    //    }
    //}
}