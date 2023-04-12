#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GOAP_Native.Editor
{
    [CustomEditor(typeof(GOAPAgent), true)]
    public class GOAPAgentEditor : UnityEditor.Editor
    {
        //private GOAPAgent _agent;

        //private VisualElement _baseInspector;
        //private VisualElement _goapInspector;

        //#region Current Goal and WorldState
        //private VisualElement _currentWorldStateContainer;
        //private VisualElement _currentGoalContainer;

        //private WorldStateElement _currentWorldState;
        //private WorldStateElement _currentGoal;
        //#endregion

        //private Label _inspectorLabel;

        //#region Foldout containers
        //private Foldout _actionsContainer;
        //private List<ActionElement> _actions = new List<ActionElement>();

        //private Foldout _worldStatesContainer;
        //private List<WorldStateElement> _worldStates = new List<WorldStateElement>();

        //private Foldout _goalsContainer;
        //private List<WorldStateElement> _goals = new List<WorldStateElement>();
        //#endregion

        //private VisualElement _currentPlan;
        //private TextElement _actionPlan;

        //public override VisualElement CreateInspectorGUI()
        //{
        //    _baseInspector = new VisualElement();
        //    InspectorElement.FillDefaultInspector(_baseInspector, serializedObject, this);

        //    CreateFoldouts();

        //    ///------------------Add elements-------------------------------------------------------------

        //    CreateCurrentWorldStatesContainer();

        //    _goapInspector.Add(_actionsContainer);

        //    _goapInspector.Add(_worldStatesContainer);
        //    _goapInspector.Add(_goalsContainer);
            
        //    _currentPlan = new VisualElement();
        //    Label label = new Label("Current Plan:");
        //    label.style.fontSize = 15;
        //    _currentPlan.Add(label);
        //    _actionPlan = new TextElement();
        //    _currentPlan.Add(_actionPlan);

        //    _goapInspector.Add(_currentPlan);

        //    _goapInspector.style.backgroundColor = Color.black * 0.2f;

        //    _baseInspector.Add(_goapInspector);

        //    return _baseInspector;
        //}

        //private void OnEnable()
        //{
        //    _agent = (GOAPAgent)target;

        //    _agent.OnAgentRegistered += Repaint;
        //    _agent.OnAgentRegistered += SubscribeChanges;
        //    _agent.OnAgentRegistered += FetchActions;
        //}

        //private void SubscribeChanges()
        //{
        //    _agent.Agent.OnEditorCallback += RefreshCurrentWorldStates;
        //    _agent.Agent.OnEditorCallback += RefreshActions;
        //    _agent.Agent.OnEditorCallback += RefreshPlan;

        //    _agent.Agent.OnEditorCallback += Repaint;
        //}

        //private void CreateFoldouts()
        //{
        //    _goapInspector = new VisualElement { name = "GOAP-Inspector" };

        //    _inspectorLabel = new Label { text = "GOAP Inspector" };
        //    _inspectorLabel.style.fontSize = 15;

        //    _goapInspector.Add(_inspectorLabel);

        //    _actionsContainer = new Foldout { text = "All Actions" };

        //    _worldStatesContainer = new Foldout { text = "All World States", value = false };
        //    _goalsContainer = new Foldout { text = "All Goals", value = false };
        //}

        //private void RefreshPlan()
        //{
        //    _actionPlan.Clear();

        //    for(int i = 0; i < _agent.Agent.Planner.CurrentActionPlan.Count; i++)
        //    {
        //        string name = _agent.Agent.Planner.AllActions[_agent.Agent.Planner.CurrentActionPlan[i]].Name;
        //        TextElement textElement = new TextElement{text = name };
        //        _actionPlan.Add(textElement);
        //    }
        //}

        //#region WorldState/Goal
        //private void RefreshCurrentWorldStates()
        //{
        //    _currentWorldState.DrawStates(_agent.Agent.Planner.CurrentWorldState);
        //    _currentGoal.DrawStates(_agent.Agent.Planner.CurrentGoal);
        //}

        //private void CreateCurrentWorldStatesContainer()
        //{
        //    _currentGoalContainer = new VisualElement();
        //    _currentWorldStateContainer = new VisualElement();

        //    _goapInspector.Add(_currentWorldStateContainer);
        //    _goapInspector.Add(_currentGoalContainer);

        //    ///---------------------------------------------------------------------------
        //    _currentWorldState = new WorldStateElement("Current World State:", false);
        //    _currentWorldState.style.backgroundColor = new Color(0.2f, 0f, 0.2f, 0.1f);

        //    _currentGoal = new WorldStateElement("Current Goal:");
        //    _currentGoal.style.backgroundColor = new Color(0, 0.2f, 0f, 0.1f);

        //    _currentWorldStateContainer.Add(_currentWorldState);
        //    _currentGoalContainer.Add(_currentGoal);
        //}
        //#endregion

        //private void RefreshWorldStatesAndGoals()
        //{
        //    _worldStatesContainer.Clear();
        //    _worldStates.Clear();

        //    for (int i = 0; i < _agent.Agent.Planner._worldStates.Count; i++)
        //    {
        //        WorldState state = _agent.Agent.Planner._worldStates.ElementAt(i).Value;

        //        WorldStateElement element = new WorldStateElement(state.Name, true);
        //        element.DrawStates(state);

        //        _worldStatesContainer.Add(element);
        //    }

        //    _goalsContainer.Clear();
        //    _goals.Clear();

        //    for (int i = 0; i < _agent.Agent.Planner._goalStates.Count; i++)
        //    {
        //        WorldState state = _agent.Agent.Planner._goalStates.ElementAt(i).Value;

        //        WorldStateElement element = new WorldStateElement(state.Name, true);
        //        element.DrawStates(state);

        //        _goalsContainer.Add(element);
        //    }
        //}

        //private void FetchActions()
        //{
        //    _actionsContainer.Clear();
        //    _actions.Clear();

        //    for (int i = 0; i < _agent.Agent.Planner.AllActions.Count; i++)
        //    {
        //        IAction action = _agent.Agent.Planner.AllActions[i];

        //        ActionElement element = new ActionElement(action);

        //        _actionsContainer.Add(element);
        //    }
        //}

        //private void RefreshActions()
        //{
        //    int actionCount = _agent.Agent.Planner.AllActions.Count;
        //    _actionsContainer.Clear();

        //    for (int i = 0; i < actionCount; i++)
        //    {
        //        if (i > _actions.Count - 1) 
        //        { 
        //            ActionElement element = new ActionElement(_agent.Agent.Planner.AllActions[i]);

        //            _actions.Add(element);

        //            _actionsContainer.Add(element);

        //            continue;
        //        }

        //        _actions[i].DrawAction(_agent.Agent.Planner.AllActions[i]);
        //        _actionsContainer.Add(_actions[i]);
        //    }

        //    while (_actions.Count > actionCount)
        //    {
        //        _actions.RemoveAt(_actions.Count - 1);
        //    }
        //}

        //private void OnDisable()
        //{
        //    if (_agent.Agent != null) _agent.Agent.ReleaseEditorEvents();
        //    _agent.ReleaseAgentRegistered();
        //}
    }
}
#endif