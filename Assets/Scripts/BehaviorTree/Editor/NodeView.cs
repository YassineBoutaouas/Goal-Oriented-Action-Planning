#if UNITY_EDITOR
using Behavior_Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> OnNodeSelected;
    public Behavior_Tree.Node node;
    public Port InputPort;
    public Port OutputPort;

    public NodeView(Behavior_Tree.Node node) : base("Assets/Scripts/BehaviorTree/Editor/NodeView.uxml")
    {
        this.node = node;
        this.title = node.name;
        this.viewDataKey = node.GUID;
        style.left = node.Position.x;
        style.top = node.Position.y;

        CreateInputPorts();
        CreateOutputPorts();
        SetupClasses();

        Label descriptionLabel = this.Q<Label>("description");
        descriptionLabel.bindingPath = "Description";
        descriptionLabel.Bind(new SerializedObject(node));
    }

    private void SetupClasses()
    {
        if (node is RootNode)
            AddToClassList("root");
        else if (node is CompositeNode)
            AddToClassList("composite");
        else if (node is DecoratorNode)
            AddToClassList("decorator");
        else
            AddToClassList("action");
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Undo.RecordObject(node, "Change Position");
        node.Position.x = newPos.xMin;
        node.Position.y = newPos.yMin;
        EditorUtility.SetDirty(node);
    }

    private void CreateInputPorts()
    {
        if (node is RootNode == false)
            InputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));

        if (InputPort != null)
        {
            InputPort.portName = "";
            InputPort.style.flexDirection = FlexDirection.Column;
            inputContainer.Add(InputPort);
        }
    }

    private void CreateOutputPorts()
    {
        if (node is CompositeNode)
        {
            OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }
        else if (node is DecoratorNode || node is RootNode)
        {
            OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
        }

        if (OutputPort != null)
        {
            OutputPort.portName = "";
            OutputPort.style.flexDirection = FlexDirection.ColumnReverse;
            outputContainer.Add(OutputPort);
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();

        OnNodeSelected?.Invoke(this);
    }

    public void SortChildren()
    {
        CompositeNode composite = node as CompositeNode;

        if (composite != null)
            composite.Children.Sort(SortByHorizontalPosition);
    }

    private int SortByHorizontalPosition(Behavior_Tree.Node left, Behavior_Tree.Node right)
    {
        return left.Position.x < right.Position.x ? -1 : 1;
    }

    public void UpdateState()
    {
        RemoveFromClassList("running");
        RemoveFromClassList("failure");
        RemoveFromClassList("success");

        if (!Application.isPlaying) return;

        switch (node.state)
        {
            case Behavior_Tree.Node.State.Running:
                if(node.started) AddToClassList("running");
                break;
            case Behavior_Tree.Node.State.Failed:
                AddToClassList("failure");
                break;
            case Behavior_Tree.Node.State.Success:
                AddToClassList("success");
                break;
            default:
                break;
        }
    }
}
#endif