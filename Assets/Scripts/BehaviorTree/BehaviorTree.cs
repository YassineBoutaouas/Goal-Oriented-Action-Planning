using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Behavior_Tree
{
    [CreateAssetMenu(menuName = "AI/Behavior Tree", fileName = "New Behavior Tree")]
    public class BehaviorTree : ScriptableObject
    {
        public Node RootNode;
        public Node.State TreeState = Node.State.Running;
        public List<Node> Nodes = new List<Node>();
        public Blackboard blackboard = new Blackboard();

        public Node.State Update()
        {

            if (RootNode.state == Node.State.Running)
                TreeState = RootNode.Update();

            //reset root node
            return TreeState;
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            Node node = CreateInstance(type) as Node;
            node.name = type.Name;
            node.GUID = GUID.Generate().ToString();

            Undo.RecordObject(this, "Added tree node");
            Nodes.Add(node);

            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(node, this);

            Undo.RegisterCreatedObjectUndo(node, "Added tree node");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Deleted tree node");

            Nodes.Remove(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        public void AddChild(Node parent, Node child)
        {
            Undo.RecordObject(parent, "Add child");

            RootNode root = parent as RootNode;
            if (root)
                root.Child = child;

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator)
                decorator.Child = child;

            CompositeNode composite = parent as CompositeNode;
            if (composite)
                composite.Children.Add(child);

            EditorUtility.SetDirty(parent);
        }

        public void RemoveChild(Node parent, Node child)
        {
            Undo.RecordObject(parent, "Add child");

            RootNode root = parent as RootNode;
            if (root)
                root.Child = null;

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator)
                decorator.Child = null;

            CompositeNode composite = parent as CompositeNode;
            if (composite)
                composite.Children.Remove(child);

            EditorUtility.SetDirty(parent);
        }

        public List<Node> GetChildren(Node parent)
        {
            RootNode root = parent as RootNode;
            if (root && root.Child != null)
            {
                List<Node> children = new List<Node> { root.Child };
                return children;
            }

            DecoratorNode decorator = parent as DecoratorNode;
            if (decorator && decorator.Child != null)
            {
                List<Node> children = new List<Node> { decorator.Child };
                return children;
            }

            CompositeNode composite = parent as CompositeNode;
            if (composite)
                return composite.Children;

            return null;
        }
#endif

        public void Traverse(Node node, System.Action<Node> visitor)
        {
            if (node)
            {
                visitor.Invoke(node);
                GetChildren(node)?.ForEach((n) => Traverse(n, visitor));
            }
        }

        public BehaviorTree Clone()
        {
            BehaviorTree clone = Instantiate(this);
            clone.RootNode = clone.RootNode.Clone();
            clone.Nodes = new List<Node>();
            Traverse(clone.RootNode, (n) => { clone.Nodes.Add(n); });

            return clone;
        }
    }
}