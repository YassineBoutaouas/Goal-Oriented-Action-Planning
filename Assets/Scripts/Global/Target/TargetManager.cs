using System;
using System.Collections.Generic;
using UnityEngine;
using static TargetManager;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public List<TargetCollection> Targets = new List<TargetCollection>();

    public enum TargetType { Mine, Crate, Workbench, Bed, None }

    public void Awake() { Instance = this; }

    public Target TryGetTarget(TargetType type)
    {
        foreach(Target t in Targets[(int)type].Targets)
        {
            if (t.Occupy()) return t;
        }

        return null;
    }

    public void FreeTarget(Target target)
    {
        target.Free();
    }

    [Serializable]
    public class TargetCollection
    {
        public TargetType Tag;

        public List<Target> Targets;

        public TargetCollection(TargetType tag)
        {
            Tag = tag;

            Targets = new List<Target>();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TargetManager))]
public class TargetManagerEditor : Editor
{
    private TargetManager _targetManager;

    private void OnEnable()
    {
        _targetManager = target as TargetManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        if(GUILayout.Button("Fetch targets"))
        {
            _targetManager.Targets.Clear();

            for (int i = 0; i < 4; i++)
            {
                _targetManager.Targets.Add(new TargetCollection((TargetType)i));
            }

            Target[] targets = FindObjectsOfType<Target>();

            for (int i = 0; i < targets.Length; i++)
            {
                _targetManager.Targets[(int)targets[i].Type].Targets.Add(targets[i]);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif