using UnityEngine;
using UnityEngine.AI;

namespace GOAP_Refactored
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class WorkerAgent : MonoBehaviour
    {
        ////public Transform Bed;
        //public Transform AppleTree;
//
        //private Worker _worker;
        //private InputManager _inputManager;
//
        //protected void Start()
        //{
        //    _worker = new Worker(this, AppleTree);
//
        //    SetInputListeners();
        //}
//
        //protected void SetInputListeners()
        //{
        //    _inputManager = InputManager.GetInstance();
        //    _inputManager.inputActions.InGame.Jump.performed += SetTired;
        //}
//
        //protected void SetTired(InputAction.CallbackContext ctx)
        //{
        //    _worker.OwnHealth = 0;
        //    _worker.ValidatePlan();
        //}
    }
}