using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Native
{
    public class Worker : Agent
    {
        public WorkerAgent _workerAgent;

        private Animator _animator;
        private NavMeshAgent _agent;

        private int _currentTarget;

        public int Stamina = 100;

        public int ItemsCarried = 0;
        public float AxeSharpness = 100;

        public Worker(GOAPAgent controller, StringBuilder strBuilder) : base(controller)
        {
            _workerAgent = (WorkerAgent)controller;
            _agent = controller.GetComponent<NavMeshAgent>();
            _animator = controller.GetComponentInChildren<Animator>();
            _currentTarget = (int)TargetManager.TargetType.None;

            //------------------------------Add States-------------------------------------------
            WorldState defaultWorldState = CreateWorldState("Default", WorldState.True, () => 1);

            defaultWorldState.Add("IsMining", false, null);
            defaultWorldState.Add("CurrentTarget", (int)TargetManager.TargetType.None, TargetType);
            defaultWorldState.Add("Stamina", Stamina, GetStamina);
            defaultWorldState.Add("ItemsCarried", ItemsCarried, GetItemsCarried);
            defaultWorldState.Add("AxeSharpness", AxeSharpness, GetAxeSharpness);

            WorldState doWorkGoal = CreateGoalState("DoWork", WorldState.True, WorkingPriority);
            doWorkGoal.Add("IsMining", true, null);

            WorldState doSharpenAxeGoal = CreateGoalState("DoSharpenAxe", WorldState.True, AxeSharpeningPriority);
            doSharpenAxeGoal.Add("AxeSharpness", 100, null);

            WorldState doSleepGoal = CreateGoalState("DoSleep", WorldState.True, SleepPriority);
            doSleepGoal.Add("Stamina", 100, null);

            ///------------------------------Create Actions--------------------------------------
            GoTo goToMine = new GoTo(this, "GoToMine", TargetManager.TargetType.Mine, _agent, _animator);
            goToMine.Preconditions.Add("ItemsCarried", 0, null);

            GoTo goToCrate = new GoTo(this, "GoToCrate", TargetManager.TargetType.Crate, _agent, _animator);
            goToCrate.Preconditions.Add("ItemsCarried", 10, null);

            GoTo goToWorkBench = new GoTo(this, "GoToWorkBench", TargetManager.TargetType.Workbench, _agent, _animator);
            goToWorkBench.Preconditions.Add("ItemsCarried", 0, null);

            GoTo goToBed = new GoTo(this, "GoToBed", TargetManager.TargetType.Bed, _agent, _animator);
            goToBed.Preconditions.Add("ItemsCarried", 0, null);

            EmptyBasket doEmptyBasket = new EmptyBasket(this, 4f, _animator);
            DoMining doMining = new DoMining(this, _animator, 5f);

            DoSleep doSleep = new DoSleep(this, _animator, 10f);
            DoSharpenAxe doSharpenAxe = new DoSharpenAxe(this, _animator, 4f);

            ///------------------------------Add all actions-------------------------------------
            AddAction(goToMine);
            AddAction(goToCrate);
            AddAction(goToWorkBench);
            AddAction(goToBed);

            AddAction(doEmptyBasket);
            AddAction(doMining);

            AddAction(doSharpenAxe);
            AddAction(doSleep);
        }

        private float WorkingPriority() { return 1 - AxeSharpeningPriority(); }

        private float AxeSharpeningPriority() { return AxeSharpness < 40 ? 1 : 0; }

        private float SleepPriority() { return Stamina < 40 ? 100 : 0; }

        public void ReleaseTarget() { _currentTarget = (int)TargetManager.TargetType.None; }

        private int TargetType() { return _currentTarget; }

        private int GetStamina() { return Stamina; }

        private int GetItemsCarried() { return ItemsCarried; }

        private float GetAxeSharpness() { return AxeSharpness; }
    }
}