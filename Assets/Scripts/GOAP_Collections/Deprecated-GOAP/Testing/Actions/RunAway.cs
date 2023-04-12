using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Refactored
{
    //ic class RunAway : Action<WorkerAgent>
    //
    //private AnimationHandler _animationHandler;
    //private NavMeshAgent _agent;

    //public RunAway(WorkerAgent context, AnimationHandler animationHandler, NavMeshAgent agent) : base(context, nameof(RunAway), 10)
    //{
    //    PostConditions.Add("RunAway", true, null);
    //    _animationHandler = animationHandler;
    //    _agent = agent;
    //}

    //public override IEnumerator OnActionStart()
    //{
    //    _animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["Stretching"].HashID, 0.1f);

    //    yield return new WaitForSeconds(_animationHandler.AnimationStates["Stretching"].Duration);
    //}
    //

    //ic class Heal : Action<WorkerAgent>
    //
    //private AnimationHandler _animationHandler;
    //private Worker _worker;

    //public Heal(WorkerAgent context, AnimationHandler animationHandler, Worker worker) : base(context, nameof(Heal), 20)
    //{
    //    PreConditions.Add("HasCover", true, null);
    //    PreConditions.Add("RunAway", true, null);

    //    PostConditions.Add("OwnHealth", 100, null);

    //    _animationHandler = animationHandler;
    //    _worker = worker;
    //}

    //public override IEnumerator OnActionStart()
    //{
    //    //_animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["GoSleep"].HashID, 0.1f);

    //    yield return new WaitForSeconds(2f); //_animationHandler.AnimationStates["GoSleep"].Duration
    //}

        //public override IEnumerator OnActionExecute()
        //{
        //    _animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["Sleeping"].HashID, 0.1f);

        //    while(_worker.PlayerHealth > 0)
        //    {
        //        _worker.PlayerHealth -= Time.deltaTime;
        //        yield return null;
        //    }
        //}

        //public override IEnumerator OnActionEnd()
        //{
        //    _animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["WakeUp"].HashID, 0.1f);
        //    _worker.PlayerHealth = 0;

        //    yield return new WaitForSeconds(_animationHandler.AnimationStates["WakeUp"].Duration);
        //}
    //}
}