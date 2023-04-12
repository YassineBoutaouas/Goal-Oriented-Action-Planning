using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP_Native
{
    public class DoSharpenAxe : Action<Worker>
    {
        private Animator _animator;
        private float _duration;

        public DoSharpenAxe(Worker contextObject, Animator animator, float duration) : base(contextObject, nameof(DoSharpenAxe), 10)
        {
            _animator = animator;
            _duration = duration;

            Preconditions.Add("CurrentTarget", (int)TargetManager.TargetType.Workbench, null);

            Effects.Add("AxeSharpness", 100, null);
        }

        public override IEnumerator OnActionStart()
        {
            yield return null;

            _animator.CrossFade("Smithing", 0.1f);
        }

        public override IEnumerator OnActionEnd()
        {
            yield return new WaitForSeconds(_duration);

            _contextObject.AxeSharpness = 100;
            _animator.CrossFade("SmithingEnd", 0.1f);
        }
    }
}