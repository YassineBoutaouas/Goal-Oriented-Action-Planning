using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GOAP_Native_Debugging
{
    public class GoalConfiguration : ScriptableObject
    {
        public List<Blackboard> Blackboards = new List<Blackboard>();
        public List<SerializedGoal> Goals = new List<SerializedGoal>();

        public List<WorldState> GeneratedGoals = new List<WorldState>(); 
    }
}