using System.Collections;
using UnityEngine;

namespace GOAP_Refactored
{
    public class Punch : Action<WorkerAgent>
    {
        public Punch(WorkerAgent controller) : base(controller, "Punch", 5)
        {
            PreConditions.Add("Position", new Vector3(0, 0.5f, 0), null); //If theres a different value than what we expect, we still want NO PLAN
            PostConditions.Add("Health", 0f, null); //Health -= 50 - introduce operators!
        }

        public override IEnumerator OnActionStart()
        {
            yield return new WaitForSeconds(2f);
        }
    }
}