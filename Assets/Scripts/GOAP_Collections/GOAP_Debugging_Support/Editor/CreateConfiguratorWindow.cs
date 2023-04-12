#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GOAP_Native_Debugging
{
    public class CreateConfiguratorWindow : EditorWindow
    {
        private const string _createConfiguratorPath = "Assets/Scripts/GOAP_Collections/GOAP_Debugging_Support/Editor/UI/CreateGoalConfiguratorEditor.uxml";

        private TextField _objectName;

        private Label _pathLabel;
        private Button _changePath;

        private Button _saveAsset;
        private Button _cancel;

        private string _selectedPath;

        private System.Action<GoalConfiguration> OnGoalConfigChanged;

        public static void OpenWindow(System.Action<GoalConfiguration> onGoalConfigCreated)
        {
            CreateConfiguratorWindow wnd = GetWindow<CreateConfiguratorWindow>();
            wnd.titleContent = new GUIContent("Create new configurator");

            wnd.position = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 4, Screen.height / 4);
            wnd.ShowPopup();

            wnd.OnGoalConfigChanged = onGoalConfigCreated;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(_createConfiguratorPath);
            visualTree.CloneTree(root);

            _objectName = root.Q<TextField>();

            _pathLabel = root.Q<Label>("path-label");
            _changePath = root.Q<Button>("path-button");

            _saveAsset = root.Q<Button>("save-button");
            _cancel = root.Q<Button>("cancel-button");

            _changePath.clicked += SelectPath;
            _saveAsset.clicked += SaveNewConfig;
            _cancel.clicked += Close;
        }

        private void SelectPath()
        {
            string absolutePath = EditorUtility.OpenFolderPanel("Select folder", "Assets/Scripts", "");
            if (absolutePath == null || absolutePath == "") return;

            _selectedPath = absolutePath.Substring(absolutePath.IndexOf("Assets/"));
            _pathLabel.text = _selectedPath;
        }

        private void SaveNewConfig()
        {
            if (_selectedPath == null)
            {
                EditorUtility.DisplayDialog("Path empty error", "Please select a valid path", "Ok");
                return;
            }

            GoalConfiguration obj = ScriptableObject.CreateInstance<GoalConfiguration>();

            AssetDatabase.CreateAsset(obj, _selectedPath + $"/{_objectName.text}.asset");
            AssetDatabase.SaveAssets();

            OnGoalConfigChanged(obj);

            Close();
        }

        private void OnDisable()
        {
            _changePath.clicked -= SelectPath;
            _saveAsset.clicked -= SaveNewConfig;
            _cancel.clicked -= Close;
        }
    }
}
#endif