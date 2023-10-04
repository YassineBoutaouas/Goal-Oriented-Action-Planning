using System.Collections;
using UnityEngine;

namespace GOAP_DOTS
{
    public class EmptyBasket : Action<Worker>
    {
        private Animator _animator;
        private float _duration;

        public EmptyBasket(Worker controller, float duration, Animator animator, Transform basketTransform) : base(controller, "EmptyBasket", 5)
        {
            _animator = animator;
            _duration = duration;

            Preconditions.Add("AtContainer", true);

            Postconditions.Add("IsBasketFull", false);
        }

        public override IEnumerator OnActionStart()
        {
            yield return null;

            _animator.CrossFade("Deloading", 0.1f);
        }

        public override IEnumerator OnActionEnd()
        {
            yield return new WaitForSeconds(_duration);

            _contextObject.ItemsInBasket = 0;
            _animator.CrossFade("DeloadEnd", 0.1f);
        }
    }
}