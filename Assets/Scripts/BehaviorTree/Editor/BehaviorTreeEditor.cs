#if UNITY_EDITOR
using Behavior_Tree;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using System;

public class BehaviorTreeEditor : EditorWindow
{
    private BehaviorTreeView _treeView;
    private InspectorView _inspectorView;
    private IMGUIContainer _blackBoardView;
    private ObjectField _treeField;

    private SerializedObject _treeObject;
    private SerializedProperty _blackboardProp;

    [MenuItem("Window/AI/BehaviorTree")]
    public static void OpenWindow()
    {
        BehaviorTreeEditor wnd = GetWindow<BehaviorTreeEditor>();
        wnd.titleContent = new GUIContent("BehaviorTree");
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (Selection.activeObject is BehaviorTree)
        {
            OpenWindow();
            return true;
        }
        return false;
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/BehaviorTree/Editor/BehaviorTree.uxml");
        visualTree.CloneTree(root);

        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/BehaviorTree/Editor/BehaviorTree.uss");

        root.styleSheets.Add(styleSheet);

        _treeView = root.Q<BehaviorTreeView>();
        _inspectorView = root.Q<InspectorView>();

        _blackBoardView = root.Q<IMGUIContainer>();
        _blackBoardView.onGUIHandler = () =>
        {
            _treeObject.Update();

            if (_blackboardProp != null)
                EditorGUILayout.PropertyField(_blackboardProp);

            _treeObject.ApplyModifiedProperties();
        };

        _treeField = root.Q<ObjectField>();

        _treeView.OnNodeSelected = OnNodeSelectionChange;

        OnSelectionChange();
    }

    private void OnSelectionChange()
    {
        BehaviorTree tree = Selection.activeObject as BehaviorTree;

        if (!tree && (Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent(out BehaviorTreeRunner treeRunner)))
        {
            tree = treeRunner.tree;
        }

        _treeField.value = tree;

        if (!tree) return;

        if (Application.isPlaying)
        {
            _treeView.PopulateView(tree);
            return;
        }

        if (AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
            _treeView.PopulateView(tree);

        _treeObject = new SerializedObject(tree);
        _blackboardProp = _treeObject.FindProperty("blackboard");
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.EnteredEditMode:
                OnSelectionChange();
                break;
            case PlayModeStateChange.ExitingEditMode:

                break;
            case PlayModeStateChange.EnteredPlayMode:
                OnSelectionChange();
                break;
            case PlayModeStateChange.ExitingPlayMode:

                break;
        }
    }

    private void OnNodeSelectionChange(NodeView nodeView)
    {
        _inspectorView.UpdateSelection(nodeView.node);
    }

    private void OnInspectorUpdate()
    {
        _treeView.UpdateNodeStates();
    }
}
#endif