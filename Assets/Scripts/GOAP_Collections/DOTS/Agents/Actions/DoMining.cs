using System.Collections;
using UnityEngine;

namespace GOAP_DOTS
{
    public class DoMining : Action<Worker>
    {
        private Animator _animator;
        private Transform _appleTree;

        private float _miningDuration;

        public DoMining(Worker context, Animator animator, float miningDuration, Transform appleTree) : base(context, "DoMining", 10)
        {
            _animator = animator;
            _miningDuration = miningDuration;
            _appleTree = appleTree;

            Preconditions.Add("AtMine", true);

            Postconditions.Add("AtMine", false);
            Postconditions.Add("IsMining", true);
        }

        public override IEnumerator OnActionStart()
        {
            yield return null;

            //_contextObject._controller.transform.rotation = Quaternion.LookRotation(Vector3.Scale((_appleTree.position - _contextObject._controller.transform.position).normalized, Vector3.right + Vector3.forward));
            _animator.CrossFade("Mining", 0.1f);
        }

        public override IEnumerator OnActionExecute()
        {
            float t = 0;

            while(t < _miningDuration)
            {
                t += Time.deltaTime;
                yield return null;
            }
        }

        public override IEnumerator OnActionEnd()
        {
            _animator.CrossFade("Gathering", 0.1f);
            _contextObject.ItemsInBasket++;
            yield return new WaitForSeconds(2.5f);
        }
    }
}