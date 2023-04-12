#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

namespace GOAP_Native_Debugging
{
    public class GoalsConfiguratorWindow : EditorWindow
    {
        #region Static and constant members
        [MenuItem("Window/AI/GOAP/Goal Configurator")]
        public static void OpenWindow()
        {
            GoalsConfiguratorWindow wnd = GetWindow<GoalsConfiguratorWindow>();
            wnd.titleContent = new GUIContent("Goal Configurator");
        }

        private const string _configuratorPath = "Assets/Scripts/GOAP_Collections/GOAP_Debugging_Support/Editor/UI/GoalStatesEditor.uxml";
        private const string _configuratorStyle = "Assets/Scripts/GOAP_Collections/GOAP_Debugging_Support/Editor/UI/GoalsStatesEditor.uss";
        #endregion

        #region Editor views
        private VisualElement _inspectorView;
        private ScrollView _blackboardContainer;
        private ScrollView _goalsContainer;
        #endregion

        #region Configuration elements and objects
        private ToolbarMenu _configuratorsMenu;
        private ToolbarButton _createNewConfiguration;

        private Func<DropdownMenuAction, DropdownMenuAction.Status> OnMenuSelected;

        private Label _currentGoalConfigurationField;
        private GoalConfiguration _currentGoalConfiguration;
        #endregion

        #region Blackboard objects
        private Button _addBlackBoardButton;

        private BlackboardSearchProvider _blackboardSearchProvider;
        private ContextualMenuManipulator _blackboardContextMenuManipulator;
        #endregion

        public void CreateGUI()
        {
            #region Base elements
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_configuratorPath);
            visualTree.CloneTree(root);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(_configuratorStyle);
            root.styleSheets.Add(styleSheet);
            
            _inspectorView = root.Q<VisualElement>("inspector-container");
            _blackboardContainer = root.Q<ScrollView>("blackboards-items");
            _goalsContainer = root.Q<ScrollView>("goals-items");
            #endregion

            #region Blackboard creation references
            _addBlackBoardButton = root.Q<Button>("add-blackboard");
            _blackboardSearchProvider = ScriptableObject.CreateInstance<BlackboardSearchProvider>();
            #endregion

            #region Configuration selection and toolbar objects
            _configuratorsMenu = root.Q<ToolbarMenu>();
            _createNewConfiguration = root.Q<ToolbarButton>("create-button");
            _currentGoalConfigurationField = root.Q<Label>("current-configuration");

            OnMenuSelected = SelectMenu;
            RefreshConfiguratorsMenu();
            #endregion

            #region Context menu objects
            _blackboardContextMenuManipulator = new ContextualMenuManipulator(CreateBlackboardContext);
            #endregion

            SubscribeEvents();
        }

        private void PopulateWindow()
        {
            _blackboardContainer.Clear();
            _goalsContainer.Clear();

            ReleaseEvents();
            SubscribeEvents();

            Debug.Log("Populate window");
        }

        #region Context menus
        private void CreateBlackboardContext(ContextualMenuPopulateEvent menuBuilder)
        {
            menuBuilder.menu.AppendAction("Add Blackboard", OpenBlackboardSearchWindow, DropdownMenuAction.Status.Normal);
            menuBuilder.menu.AppendAction("Remove Blackboard", RemoveBlackboard, DropdownMenuAction.Status.Normal);
        }
        #endregion

        #region Blackboard objects
        private void OpenBlackboardSearchWindow() { SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Mouse.current.position.ReadValue())), _blackboardSearchProvider); }

        private void OpenBlackboardSearchWindow(DropdownMenuAction menuAction) { OpenBlackboardSearchWindow(); }

        private void AddBlackboard(Blackboard entry)
        {
            if (_currentGoalConfiguration.Blackboards.Contains(entry)) return;

            AddBlackboardEntry(entry);
        }

        private void AddBlackboardEntry(Blackboard entry)
        {
            ListItemElement blackboardObject = new ListItemElement(entry.name, "blackboard-object", "blackboard-object", "blackboard-object-label");
            _blackboardContainer.Add(blackboardObject);

            _currentGoalConfiguration.Blackboards.Add(entry);
        }

        private void RemoveBlackboard(DropdownMenuAction menuAction)
        {
            int index = _blackboardContainer.IndexOf((VisualElement)_blackboardContextMenuManipulator.target.focusController.focusedElement);

            _blackboardContainer.RemoveAt(index);
            _currentGoalConfiguration.Blackboards.RemoveAt(index);
        }
        #endregion

        #region Goal configuration menu
        private void CreateGoalConfiguration() { CreateConfiguratorWindow.OpenWindow(SelectConfiguration); }

        private void RefreshConfiguratorsMenu()
        {
            _configuratorsMenu.menu.ClearItems();

            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(GoalConfiguration).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                string[] entryTitle = path.Split('/');

                _configuratorsMenu.menu.AppendAction(entryTitle[^1], SelectConfiguration, OnMenuSelected, AssetDatabase.LoadAssetAtPath(path, typeof(GoalConfiguration)));
            }
        }

        private DropdownMenuAction.Status SelectMenu(DropdownMenuAction arg) { return DropdownMenuAction.Status.Normal; }

        private void SelectConfiguration(DropdownMenuAction menuAction)
        {
            SelectConfiguration(menuAction.userData as GoalConfiguration);
        }

        private void SelectConfiguration(GoalConfiguration config)
        {
            _currentGoalConfiguration = config;
            _currentGoalConfigurationField.text = _currentGoalConfiguration ? _currentGoalConfiguration.name : "No Configuration Selected";

            PopulateWindow();
        }
        #endregion

        private void OnDisable() { ReleaseEvents(); }

        #region Event methods
        private void SubscribeEvents()
        {
            _blackboardSearchProvider.OnSelectedBlackboard += AddBlackboard;
            _addBlackBoardButton.clicked += OpenBlackboardSearchWindow;

            EditorApplication.projectChanged += RefreshConfiguratorsMenu;
            _createNewConfiguration.clicked += CreateGoalConfiguration;

            _blackboardContainer.AddManipulator(_blackboardContextMenuManipulator);
        }

        private void ReleaseEvents()
        {
            _blackboardSearchProvider.OnSelectedBlackboard -= AddBlackboard;
            _addBlackBoardButton.clicked -= OpenBlackboardSearchWindow;

            EditorApplication.projectChanged -= RefreshConfiguratorsMenu;
            _createNewConfiguration.clicked -= CreateGoalConfiguration;

            _blackboardContainer.RemoveManipulator(_blackboardContextMenuManipulator);
        }
        #endregion
    }

    public class ListItemElement : VisualElement
    {
        public ListItemElement(string labelName, string objectIdentifier, string objectClassStyle, string labelClassStyle)
        {
            name = objectIdentifier;
            Label blackboardLabel = new Label(labelName);

            Add(blackboardLabel);
            focusable = true;

            blackboardLabel.AddToClassList(labelClassStyle);
            AddToClassList(objectClassStyle);
        }
    }

    public class BlackboardSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public System.Action<Blackboard> OnSelectedBlackboard;
        private List<SearchTreeEntry> _blackboardSearchTree;

        public BlackboardSearchProvider()
        {
            _blackboardSearchTree = new List<SearchTreeEntry>();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            _blackboardSearchTree.Clear();

            _blackboardSearchTree.Add(new SearchTreeGroupEntry(new GUIContent("Blackboards"), 0));

            foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(Blackboard).Name))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                Blackboard blackboard = (Blackboard)AssetDatabase.LoadAssetAtPath(path, typeof(Blackboard));

                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(blackboard.name));
                entry.level = 1;
                entry.userData = blackboard;
                _blackboardSearchTree.Add(entry);
            }

            return _blackboardSearchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            OnSelectedBlackboard?.Invoke((Blackboard)SearchTreeEntry.userData);
            return true;
        }
    }
}
#endif