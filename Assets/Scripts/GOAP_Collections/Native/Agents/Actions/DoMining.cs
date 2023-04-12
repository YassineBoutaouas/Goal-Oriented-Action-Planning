using System.Collections;
using UnityEngine;

namespace GOAP_Native
{
    public class DoMining : Action<Worker>
    {
        private Animator _animator;

        private float _miningDuration;

        public DoMining(Worker context, Animator animator, float miningDuration) : base(context, "DoMining", 10)
        {
            _animator = animator;
            _miningDuration = miningDuration;

            Preconditions.Add("CurrentTarget", (int)TargetManager.TargetType.Mine, null);

            Effects.Add("IsMining", true, null);
            Effects.Add("ItemsCarried", 10, null);

            Effects.Add("CurrentTarget", (int)TargetManager.TargetType.None, null);
        }

        public override IEnumerator OnActionStart()
        {
            yield return null;

            _animator.CrossFade("Mining", 0.1f);
        }

        public override IEnumerator OnActionExecute()
        {
            float t = 0;

            while (t < _miningDuration)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        public override IEnumerator OnActionEnd()
        {
            _animator.CrossFade("Gathering", 0.1f);
            _contextObject.ItemsCarried = 10;
            _contextObject.AxeSharpness -= 40;
            _contextObject.Stamina -= 20;
            yield return new WaitForSeconds(2.5f);
        }
    }
}