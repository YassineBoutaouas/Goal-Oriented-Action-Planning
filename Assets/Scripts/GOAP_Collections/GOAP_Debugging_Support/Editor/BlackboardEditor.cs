#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GOAP_Native_Debugging
{
    [CustomEditor(typeof(Blackboard))]
    public class BlackboardEditor : Editor
    {
        private Blackboard _blackBoard;
        private const string path = "Assets/Scripts/GOAP_Collections/GOAP_Debugging_Support/Editor/UI/BlackboardEditor.uxml";
        private const string entryObjectpath = "Assets/Scripts/GOAP_Collections/GOAP_Debugging_Support/Editor/UI/BlackboardEntry.uxml";

        private VisualElement _messageContainer;
        private HelpBox _helpBox;

        private VisualElement _entriesContainer;
        private List<BlackboardEntry> _blackboardEntries;
        private Button _addButton;
        private Button _clearButton;

        private bool _hasDuplicates;
        private List<BlackboardEntry> _entryKeys;

        private void OnEnable() { _blackBoard = target as Blackboard; _entryKeys = new List<BlackboardEntry>(); }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new VisualElement();

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            tree.CloneTree(inspector);

            _entriesContainer = inspector.Q<ScrollView>("entries");

            _blackboardEntries = new List<BlackboardEntry>();

            _messageContainer = inspector.Q<VisualElement>("warning-message");
            _helpBox = new HelpBox("The collection of keys contains duplicates! Please remove before proceeding", HelpBoxMessageType.Error);

            PopulateEntries();

            _addButton = inspector.Q<Button>("add-button");
            _clearButton = inspector.Q<Button>("clear-button");

            _addButton.clicked += AddEntry;
            _clearButton.clicked += ClearEntries;

            return inspector;
        }

        public void PopulateEntries()
        {
            serializedObject.Update();

            for (int i = 0; i < _blackBoard.Entries.Count; i++)
            {
                Blackboard.Item entryObject = _blackBoard.Entries[i];
                BlackboardEntry entryElement = new BlackboardEntry(_blackBoard, entryObject, entryObjectpath, serializedObject, i);

                entryElement.RegisterCallbacks(entryObject.Key, entryObject.Value);
                entryElement.RemoveButton.clicked += () => RemoveEntry(entryElement);
                entryElement.OnKeyChanged += CheckForDuplicates;

                _entriesContainer.Add(entryElement);
                _blackboardEntries.Add(entryElement);
            }

            CheckForDuplicates();

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public void AddEntry()
        {
            serializedObject.Update();

            //Undo.RecordObject(_blackBoard, "Add Entry");
            //Undo.RecordObject(this, "Add Entry");

            Blackboard.Item entryObject = _blackBoard.CreateEntry($"Entry[{_blackBoard.Entries.Count}]", Blackboard.ValueType.Bool);

            BlackboardEntry entryElement = new BlackboardEntry(_blackBoard, entryObject, entryObjectpath, serializedObject, _blackBoard.Entries.Count - 1);

            entryElement.RegisterCallbacks($"Entry[{_blackBoard.Entries.Count - 1}]", Blackboard.ValueType.Bool);
            entryElement.RemoveButton.clicked += () => RemoveEntry(entryElement);
            entryElement.OnKeyChanged += CheckForDuplicates;

            _entriesContainer.Add(entryElement);
            _blackboardEntries.Add(entryElement);

            CheckForDuplicates();

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public void RemoveEntry(BlackboardEntry entryElement)
        {
            serializedObject.Update();

            //Undo.RecordObject(_blackBoard, "Remove Entry");

            entryElement.UnregisterCallbacks();
            _blackBoard.RemoveEntry(entryElement.EntryObject);

            _entriesContainer.Remove(entryElement);
            _blackboardEntries.Remove(entryElement);

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public void ClearEntries()
        {
            serializedObject.Update();

            //Undo.RecordObject(_blackBoard, "Cleared Entries");

            for (int i = 0; i < _blackboardEntries.Count; i++)
            {
                _blackboardEntries[i].UnregisterCallbacks();
                _entriesContainer.Remove(_blackboardEntries[i]);
            }

            _blackBoard.ClearEntries();
            _blackboardEntries.Clear();

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public void CheckForDuplicates()
        {
            _entryKeys.Clear();

            _hasDuplicates = false;

            foreach (Blackboard.Item item in _blackBoard.Entries)
            {
                if (_blackBoard.Entries.Count(i => i.Key.Equals(item.Key)) > 1)
                {
                    _hasDuplicates = true;
                    break;
                }
            }

            _messageContainer.Clear();
            Debug.ClearDeveloperConsole();

            if (_hasDuplicates)
            {
                _messageContainer.Add(_helpBox);
                Debug.LogError("Blackboard Duplicates detected. Please remove duplicates before proceeding");
            }
        }

        public void OnDisable()
        {
            if (_blackBoard == null) return;

            _addButton.clicked -= AddEntry;
            _clearButton.clicked -= ClearEntries;
        }
    }

    public class BlackboardEntry : VisualElement
    {
        private Blackboard _blackBoard;

        public Blackboard.Item EntryObject;
        private SerializedObject _serializedObject;

        public Button RemoveButton;
        public Action OnKeyChanged;

        public BlackboardEntry(Blackboard blackBoard, Blackboard.Item entryIndex, string entryObjectpath, SerializedObject serializedObj, int index)
        {
            _blackBoard = blackBoard;
            _serializedObject = serializedObj;
            EntryObject = entryIndex;

            VisualTreeAsset tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(entryObjectpath);
            tree.CloneTree(this);

            this.Q<Label>("entry-label").text = "[Blackboard Entry]";

            if (index % 2 != 0)
                AddToClassList("dark-element");

            RemoveButton = this.Q<Button>("delete-button");
        }

        public void RegisterCallbacks(string initialValue, Blackboard.ValueType initialType)
        {
            TextField textField = this.Q<TextField>("entry-key");
            textField.value = initialValue;

            textField.RegisterCallback<ChangeEvent<string>>(UpdateEntryKeyCallback);


            EnumField enumField = this.Q<EnumField>("type-enum");
            enumField.value = initialType;

            enumField.Q<EnumField>("type-enum").RegisterCallback<ChangeEvent<Enum>>(UpdateEntryValueCallback);
        }

        private void UpdateEntryKeyCallback(ChangeEvent<string> strEvent)
        {
            _serializedObject.Update();
            //Undo.RecordObject(_blackBoard, "Changed Key");

            EntryObject.Key = strEvent.newValue;
            OnKeyChanged?.Invoke();

            _serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private void UpdateEntryValueCallback(ChangeEvent<Enum> enumEvent)
        {
            _serializedObject.Update();
            //Undo.RecordObject(_blackBoard, "Changed Value");

            EntryObject.Value = (Blackboard.ValueType)enumEvent.newValue;
            _serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        public void UnregisterCallbacks()
        {
            this.Q<TextField>("entry-key").UnregisterCallback<ChangeEvent<string>>(UpdateEntryKeyCallback);

            this.Q<EnumField>("type-enum").Q<EnumField>("type-enum").UnregisterCallback<ChangeEvent<Enum>>(UpdateEntryValueCallback);

            OnKeyChanged = null;
        }
    }
}
#endif