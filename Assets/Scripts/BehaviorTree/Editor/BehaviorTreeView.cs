#if UNITY_EDITOR
using Behavior_Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class BehaviorTreeView : GraphTreeView
{
    public new class UxmlFactory : UxmlFactory<BehaviorTreeView, UxmlTraits> { }

    private BehaviorTree _behaviorTree;

    public Action<NodeView> OnNodeSelected;

    public override void OnUndoRedo()
    {
        base.OnUndoRedo();
        PopulateView(_behaviorTree);
        AssetDatabase.SaveAssets();
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //base.BuildContextualMenu(evt);
        {
            var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"Actions/[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }

        {
            var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"Composites/[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }

        {
            var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
            foreach (var type in types)
            {
                evt.menu.AppendAction($"Decorators/[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
            }
        }
    }

    private NodeView FindNodeView(Behavior_Tree.Node node)
    {
        return GetNodeByGuid(node.GUID) as NodeView;
    }

    internal void PopulateView(BehaviorTree tree)
    {
        _behaviorTree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (tree.RootNode == null)
        {
            tree.RootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        _behaviorTree.Nodes.ForEach(n => CreateNodeView(n));

        _behaviorTree.Nodes.ForEach(n =>
        {
            NodeView parent = FindNodeView(n);

            var children = _behaviorTree.GetChildren(n);

            if (children != null)
            {
                children.ForEach(c =>
                {
                    NodeView child = FindNodeView(c);

                    Edge edge = parent.OutputPort.ConnectTo(child.InputPort);
                    AddElement(edge);
                });
            }
        });
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        graphViewChange.elementsToRemove?.ForEach(elem =>
            {
                NodeView nodeView = elem as NodeView;
                if (nodeView != null)
                {
                    _behaviorTree.DeleteNode(nodeView.node);
                }

                Edge edge = elem as Edge;
                if (edge != null)
                {
                    NodeView parent = edge.output.node as NodeView;
                    NodeView child = edge.input.node as NodeView;

                    _behaviorTree.RemoveChild(parent.node, child.node);
                }
            });

        graphViewChange.edgesToCreate?.ForEach(edge =>
            {
                NodeView parent = edge.output.node as NodeView;
                NodeView child = edge.input.node as NodeView;

                _behaviorTree.AddChild(parent.node, child.node);
            });

        if(graphViewChange.movedElements != null)
        {
            nodes.ForEach((n) =>
            {
                NodeView view = n as NodeView;
                view.SortChildren();
            });
        }

        return graphViewChange;
    }

    private void CreateNode(Type type)
    {
        Behavior_Tree.Node node = _behaviorTree.CreateNode(type);
        CreateNodeView(node);
    }

    private void CreateNodeView(Behavior_Tree.Node node)
    {
        NodeView nodeView = new NodeView(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(node => 
        { 
            NodeView view = node as NodeView;
            view.UpdateState();
        });
    }
}
#endif