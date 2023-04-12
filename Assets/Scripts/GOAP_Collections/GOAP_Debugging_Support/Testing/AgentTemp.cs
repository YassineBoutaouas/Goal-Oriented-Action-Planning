using GOAP_Native_Debugging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AgentTemp : MonoBehaviour
{
    public Blackboard blackboard;
}

#if UNITY_EDITOR
[CustomEditor(typeof(AgentTemp), true)]
public class AgentTempEditor : Editor
{
    //private AgentTemp _agent;

    //public void OnEnable()
    //{
    //    _agent = target as AgentTemp;
    //}

    //public override void OnInspectorGUI()
    //{
    //    base.OnInspectorGUI();
    //    if(GUILayout.Button("Add entries"))
    //    {
    //        //_agent.blackboard.CreateEntry("Position", typeof(Vector3));
    //        //_agent.blackboard.CreateEntry("HasHealth", typeof(bool));

    //        AssetDatabase.SaveAssets();
    //    }
    //}
}
#endif