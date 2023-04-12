using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Refactored
{
    //public class Worker : Agent<WorkerAgent>
    //{
    //    public int AppleAmount = 0;
    //    public bool IsAxeSharp;
    //    public bool isEating;
//
    //    public float PlayerHealth;
    //    public float OwnHealth = 100;
//
    //    public bool DoesHaveBed;
//
    //    public Vector3 ShootingPos;
    //    public Vector3 Bed;
    //    private Vector3 _currentTarget;
//
    //    public Worker(WorkerAgent controller, Transform appleTree) : base(controller)
    //    {
    //        AnimationHandler animationHandler = controller.GetComponentInChildren<AnimationHandler>();
//
    //        ShootingPos = appleTree.position;
//
    //        ActionBase goTo = new GoTo(controller, GetCurrentTarget, controller.GetComponent<NavMeshAgent>(), "GoTo", 6, 0.5f, 6, animationHandler, _currentTarget);
//
    //        ActionBase shooting = new Shooting(controller, animationHandler, (Vector3)GetCurrentTarget());
    //        ActionBase load = new Load(controller, animationHandler, 4f, this, ShootingPos);
    //        ActionBase getCover = new GetCover(controller, animationHandler);
    //        ActionBase scream = new Scream(controller, animationHandler);
//
    //        ActionBase runAway = new RunAway(controller, animationHandler, controller.GetComponent<NavMeshAgent>());
    //        ActionBase heal = new Heal(controller, animationHandler, this);
//
    //        //----FLEEING----
//
    //        AddAction(goTo);
//
    //        AddAction(shooting);
    //        AddAction(load);
    //        AddAction(getCover);
    //        AddAction(scream);
//
    //        AddAction(runAway);
    //        AddAction(heal);
//
    //        WorldState worldState = CreateWorldState("Default", WorldState.True, WorldState.DefaultPriority);
    //        worldState.Add("Position", GetPosition(), GetPosition);
//
    //        worldState.Add("GunLoaded", false, null);
    //        worldState.Add("HasCover", false, null);
    //        worldState.Add("Screamed", false, null);
    //        worldState.Add("Health", PlayerHealth, null);
//
    //        worldState.Add("RunAway", false, null);
    //        worldState.Add("OwnHealth", OwnHealth, GetHealth);
//
    //        WorldState goal = CreateGoalState("Attacking", WorldState.True, AttackPriority);
    //        goal.Add("Health", 0, null);
//
    //        WorldState recoverGoal = CreateGoalState("Recover", WorldState.True, HealPriority);
    //        recoverGoal.Add("OwnHealth", 100, GetHealth);
//
    //        Plan();
//
    //        OnPlanEnd += () => Plan();
    //    }
//
    //    private object GetCurrentTarget()
    //    {
    //        _currentTarget = ShootingPos;
//
    //        return _currentTarget;
    //    }
//
    //    private float AttackPriority()
    //    {
    //        return OwnHealth < 50 ? 0 : 1;
    //    }
//
    //    private float HealPriority()
    //    {
    //        return 1 - AttackPriority();
    //    }
//
    //    private object GetPosition()
    //    {
    //        return _controllerObject.transform.position;
    //    }
//
    //    private object GetHealth()
    //    {
    //        return OwnHealth;
    //    }
    //}
}