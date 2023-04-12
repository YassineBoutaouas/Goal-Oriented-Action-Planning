using System.Collections;
using UnityEngine;

namespace GOAP_Refactored
{
    //public class Shooting : Action<WorkerAgent>
    //{
    //     private AnimationHandler _animationHandler;

    //     public Shooting(WorkerAgent context, AnimationHandler animationHandler, Vector3 target) : base(context, nameof(Shooting), 10)
    //     {
    //         PreConditions.Add("Position", target, null);
    //         PreConditions.Add("GunLoaded", true, null);

    //         PostConditions.Add("Health", 0, null);

    //         _animationHandler = animationHandler;
    //     }

    //     public override IEnumerator OnActionStart()
    //     {
    //         //_animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["Eat"].HashID, 0.1f);

    //         yield return new WaitForSeconds(1);
    //     }
    // }

    // public class Load : Action<WorkerAgent>
    // {
    //     private AnimationHandler _animationHandler;
    //     private float _duration;

    //     private Worker _worker;

    //     public Load(WorkerAgent context, AnimationHandler animationHandler, float duration, Worker worker, Vector3 appleTreePos) : base(context, nameof(Load), 10)
    //     {
    //         PreConditions.Add("HasCover", true, null);

    //         PostConditions.Add("GunLoaded", true, null);

    //         _animationHandler = animationHandler;
    //         _duration = duration;
    //         _worker = worker;
    //     }

    //     public override IEnumerator OnActionStart()
    //     {
    //         //_animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["MiningLoop"].HashID, 0.1f);

    //         yield return new WaitForSeconds(1);
    //     }
    // }

    // public class GetCover : Action<WorkerAgent>
    // {
    //     private AnimationHandler _animationHandler;

    //     public GetCover(WorkerAgent context, AnimationHandler animationHandler) : base(context, nameof(GetCover), 20)
    //     {
    //         PreConditions.Add("Screamed", true, null);

    //         PostConditions.Add("HasCover", true, null);

    //         _animationHandler = animationHandler;
    //     }

    //     public override IEnumerator OnActionStart()
    //     {
    //         //_animationHandler.Animator.CrossFade(_animationHandler.AnimationStates["Smithing"].HashID, 0.1f);

    //         yield return new WaitForSeconds(1);
    //     }
    // }

    // public class Scream : Action<WorkerAgent>
    // {
    //     public Scream(WorkerAgent context, AnimationHandler animationHandler) : base(context, nameof(Scream), 20)
    //     {
    //         PostConditions.Add("Screamed", true, null);
    //     }

    //     public override IEnumerator OnActionStart()
    //     {
    //         yield return new WaitForSeconds(1);
    //     }
    // }
}