using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GOAP_Native_Debugging
{
    [CreateAssetMenu(menuName = "AI/GOAP/Goal", fileName = "New Goal")]
    public class SerializedGoal : ScriptableObject
    {
        public List<GoalItem> goals = new List<GoalItem>();

        public SerializedGoal()
        {
            Debug.Log("Create");
        }
    }

    [System.Serializable]
    public class GoalItem
    {
        public string Key;
        public object Value;
    }

    public class BoolItem : GoalItem
    {
        public bool Bool;

        public BoolItem(bool b)
        {
            Bool = b;
        }
    }

    public class IntItem : GoalItem
    {
        public int Integer;

        public IntItem(int i)
        {
            Integer = i;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SerializedGoal), true)]
    public class SerializedGoalEditor : Editor
    {
        private SerializedGoal goal;

        private void OnEnable()
        {
            goal = target as SerializedGoal;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Debug stuff"))
            {
                for (int i = 0; i < goal.goals.Count; i++)
                {
                    Debug.Log(goal.goals[i].Key);
                    Debug.Log(goal.goals[i].Value.ToString());
                }
            }
        }
    }
#endif
}