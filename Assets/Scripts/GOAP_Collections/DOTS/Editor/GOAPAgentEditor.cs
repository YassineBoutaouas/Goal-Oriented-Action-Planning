#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GOAP_DOTS.Editor
{
    [CustomEditor(typeof(GOAPAgent), true)]
    public class GOAPAgentEditor : UnityEditor.Editor
    {
        private GOAPAgent _agent;

        private VisualElement _baseInspector;
        private VisualElement _goapInspector;

        #region Current Goal and WorldState
        private VisualElement _currentWorldStateContainer;
        private VisualElement _currentGoalContainer;

        private WorldStateElement _currentWorldState;
        private WorldStateElement _currentGoal;
        #endregion

        private Label _inspectorLabel;

        #region Foldout containers
        private Foldout _actionsContainer;
        private Foldout _viableActionsContainer;

        private Foldout _worldStatesContainer;
        private Foldout _goalsContainer;
        #endregion

        public override VisualElement CreateInspectorGUI()
        {
            _baseInspector = new VisualElement();
            InspectorElement.FillDefaultInspector(_baseInspector, serializedObject, this);

            CreateFoldouts();

            ///------------------Add elements-------------------------------------------------------------

            CreateCurrentWorldStatesContainer();

            _goapInspector.Add(_actionsContainer);
            _goapInspector.Add(_viableActionsContainer);

            _goapInspector.Add(_worldStatesContainer);
            _goapInspector.Add(_goalsContainer);

            _baseInspector.Add(_goapInspector);

            return _baseInspector;
        }

        private void OnEnable()
        {
            _agent = (GOAPAgent)target;

            _agent.OnAgentRegistered += Repaint;
            _agent.OnAgentRegistered += SubscribeChanges;
        }

        private void SubscribeChanges()
        {
            _agent.Agent.OnEditorCallback += RefreshCurrentWorldStates;

            _agent.Agent.OnEditorCallback += Repaint;
        }

        private void CreateFoldouts()
        {
            _goapInspector = new VisualElement { name = "GOAP-Inspector" };

            _inspectorLabel = new Label { text = "GOAP Inspector" };

            _goapInspector.Add(_inspectorLabel);

            _actionsContainer = new Foldout { text = "All Actions" };
            _viableActionsContainer = new Foldout { text = "Viable Actions" };

            _worldStatesContainer = new Foldout { text = "All World States" };
            _goalsContainer = new Foldout { text = "All Goals" };
        }

        private void RefreshCurrentWorldStates()
        {
            _currentWorldState.DrawStates(_agent.Agent._planner.CurrentWorldState);
            _currentGoal.DrawStates(_agent.Agent._planner.CurrentGoal);
        }

        private void CreateCurrentWorldStatesContainer()
        {
            _currentGoalContainer = new VisualElement();
            _currentWorldStateContainer = new VisualElement();

            _currentWorldStateContainer.Add(new Label { text = "Current World State: " });
            _currentGoalContainer.Add(new Label { text = "Current Goal: " });

            _goapInspector.Add(_currentWorldStateContainer);
            _goapInspector.Add(_currentGoalContainer);

            ///---------------------------------------------------------------------------
            _currentWorldState = new WorldStateElement(false);
            _currentGoal = new WorldStateElement();

            _currentWorldStateContainer.Add(_currentWorldState);
            _currentGoalContainer.Add(_currentGoal);
        }

        private void OnDisable()
        {
            if (_agent.Agent != null) _agent.Agent.ReleaseEditorEvents();
            _agent.ReleaseAgentRegistered();
        }
    }
}
#endif