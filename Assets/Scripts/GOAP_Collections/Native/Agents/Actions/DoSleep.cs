using System.Collections;
using UnityEngine;

namespace GOAP_Native
{
    public class DoSleep : Action<Worker>
    {
        private Animator _animator;
        private float _duration;

        public DoSleep(Worker contextObject, Animator animator, float duration) : base(contextObject, nameof(DoSleep), 1)
        {
            _animator = animator;
            _duration = duration;

            Preconditions.Add("CurrentTarget", (int)TargetManager.TargetType.Bed, null);

            Effects.Add("Stamina", 100, null);
        }

        public override IEnumerator OnActionStart()
        {
            yield return null;

            _animator.CrossFade("GoSleep", 0.1f);
        }

        public override IEnumerator OnActionEnd()
        {
            yield return new WaitForSeconds(_duration);

            _contextObject.Stamina = 100;
            _animator.CrossFade("WakeUp", 0.1f);
        }
    }
}