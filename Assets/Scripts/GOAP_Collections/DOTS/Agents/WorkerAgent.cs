using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GOAP_DOTS
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class WorkerAgent : GOAPAgent
    {
        public Transform[] Targets = new Transform[3];

        private void Start()
        {
            Create(new Worker(this));
            Agent.Plan(_stringBuilder, true);

            Agent.OnPlanEnd += () => Agent.Plan(_stringBuilder, true);
        }
    }

    public class Worker : Agent
    {
        private WorkerAgent _workerAgent;
        private Animator _animator;
        private NavMeshAgent _agent;

        public int Stamina = 100;

        public bool HasAxe = false;
        public int ItemsInBasket = 0;
        public float AxeSharpness = 0;

        public GoTo goToContainer;
        public GoTo goToBlackSmith;
        public GoTo goToMine;

        public EmptyBasket doEmptyBasket;
        public DoMining doMining;

        public WorldState defaultWorldState;
        public WorldState doWorkGoal;

        public Worker(WorkerAgent controller) : base(controller)
        {
            _workerAgent = controller;

            _agent = controller.GetComponent<NavMeshAgent>();
            _animator = controller.GetComponentInChildren<Animator>();

            //-------------------------------Create States----------------------------------------
            defaultWorldState = new WorldState("Default", 1, Allocator.Persistent, true, 1);

            defaultWorldState.Add("AtContainer", false);
            defaultWorldState.Add("AtSmith", false);
            defaultWorldState.Add("AtMine", AtMine());

            defaultWorldState.Add("HasAxe", HasAxe);
            defaultWorldState.Add("IsBasketFull", IsBasketFull());
            defaultWorldState.Add("IsAxeSharp", IsAxeSharp());
            defaultWorldState.Add("IsMining", false);

            doWorkGoal = new WorldState("Mining", 1, Allocator.Persistent, true, 1 - RecoverPriority());
            doWorkGoal.Add("IsMining", true);

            //------------------------------Add States-------------------------------------------
            AddWorldState(defaultWorldState);

            AddGoal(doWorkGoal);

            //------------------------------Create Actions---------------------------------------
            goToMine = new GoTo("GoToMine", "AtMine", AtMine, controller.Targets[0].position, _agent, _animator, "Movement");
            goToMine.Preconditions.Add("IsBasketFull", false);

            goToContainer = new GoTo("GoToContainer", "AtContainer", AtContainer, controller.Targets[2].position, _agent, _animator, "LoadedMovement");

            doEmptyBasket = new EmptyBasket(this, 5f, _animator, controller.Targets[2]);
            doMining = new DoMining(this, _animator, 3f, controller.Targets[0]);

            //------------------------------Add Actions---------------------------------------
            AddAction(goToMine);
            AddAction(goToContainer);

            AddAction(doMining);
            AddAction(doEmptyBasket);

            BakeGoals();
            BakeActionconditions();
        }

        public override void UpdateWorldStatesAndGoals()
        {
            base.UpdateWorldStatesAndGoals();

            ValidateGoal(doWorkGoal.Name, true, 1 - RecoverPriority());

            defaultWorldState.SetStateValue("AtMine", AtMine());

            defaultWorldState.SetStateValue("IsBasketFull", IsBasketFull());
        }

        private bool AtMine() { return Vector3.Distance(_agent.transform.position, _workerAgent.Targets[0].position) < _agent.stoppingDistance + 2; }
        private bool AtContainer() { return Vector3.Distance(_agent.transform.position, _workerAgent.Targets[2].position) < _agent.stoppingDistance + 2; }

        private bool IsBasketFull() { return ItemsInBasket >= 1; }

        private bool IsAxeSharp() { return AxeSharpness > 60f; }

        private float RecoverPriority() { return Stamina == 0 ? 1 : 0; }

        public override void Dispose()
        {
            doWorkGoal.Dispose();
            defaultWorldState.Dispose();

            base.Dispose();
        }
    }
}