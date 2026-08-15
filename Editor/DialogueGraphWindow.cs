using System;
using System.Collections.Generic;
using DialogTree.Editor.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogTree.Editor
{
    public sealed class DialogueGraphWindow : EditorWindow
    {
        private readonly List<DialogueGraphTab> _tabs = new();
        private DialogueGraphTab _activeTab;

        private VisualElement _tabBar;
        private VisualElement _graphContainer;
        private TextField _titleField;
        private Label _statusLabel;

        [MenuItem("Window/Dialogue Tree Editor")]
        public static void Open()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("Dialogue Tree Editor");
            window.minSize = new Vector2(800, 500);
        }

        private void OnEnable()
        {
            BuildToolbar();

            _tabBar = new VisualElement
            {
                name = "tab-bar",
                style = { flexDirection = FlexDirection.Row, marginTop = 2, marginBottom = 2 }
            };
            rootVisualElement.Add(_tabBar);

            _graphContainer = new VisualElement { style = { flexGrow = 1 } };
            rootVisualElement.Add(_graphContainer);

            CreateNewTab();
        }

        private void OnDisable()
        {
            rootVisualElement.Clear();
            _tabs.Clear();
            _activeTab = null;
        }

        private void BuildToolbar()
        {
            var toolbar = new Toolbar();

            toolbar.Add(new ToolbarButton(OnNewGraph) { text = "New" });
            toolbar.Add(new ToolbarButton(OnLoadGraph) { text = "Open…" });
            toolbar.Add(new ToolbarButton(OnSaveGraph) { text = "Save" });
            toolbar.Add(new ToolbarButton(OnSaveGraphAs) { text = "Save as…" });

            _titleField = new TextField("Title") { style = { marginLeft = 12, width = 220 } };
            _titleField.RegisterValueChangedCallback(evt =>
            {
                if (_activeTab == null) return;
                _activeTab.Title = evt.newValue;
                RebuildTabBar();
            });
            toolbar.Add(_titleField);

            _statusLabel = new Label("No file loaded")
            {
                style = { unityTextAlign = TextAnchor.MiddleLeft, marginLeft = 12 }
            };
            toolbar.Add(_statusLabel);

            rootVisualElement.Add(toolbar);
        }

        private void CreateNewTab()
        {
            var tab = new DialogueGraphTab { GraphId = Guid.NewGuid().ToString("N") };

            tab.View.style.flexGrow = 1;
            tab.View.StretchToParentSize();

            _tabs.Add(tab);
            _graphContainer.Add(tab.View);

            SetActiveTab(tab);
            RebuildTabBar();
        }

        private void SetActiveTab(DialogueGraphTab tab)
        {
            if (_activeTab != null)
                _activeTab.View.style.display = DisplayStyle.None;

            _activeTab = tab;
            _activeTab.View.style.display = DisplayStyle.Flex;

            _titleField.SetValueWithoutNotify(_activeTab.Title);
            UpdateStatusLabel();
        }

        private void CloseTab(DialogueGraphTab tab)
        {
            if (tab.IsDirty && !EditorUtility.DisplayDialog("Close tab",
                    $"« {tab.Title} » has unsaved changes. Close anyway?", "Yes", "Cancel"))
            {
                return;
            }

            _graphContainer.Remove(tab.View);
            _tabs.Remove(tab);

            if (_tabs.Count == 0)
            {
                CreateNewTab();
            }
            else if (_activeTab == tab)
            {
                SetActiveTab(_tabs[0]);
            }

            RebuildTabBar();
        }

        private void RebuildTabBar()
        {
            _tabBar.Clear();

            foreach (var tab in _tabs)
            {
                var tabElement = new VisualElement { style = { flexDirection = FlexDirection.Row, marginRight = 4 } };

                var selectButton = new Button(() => SetActiveTab(tab)) { text = tab.DisplayLabel };
                if (tab == _activeTab)
                {
                    selectButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                }

                var closeButton = new Button(() => CloseTab(tab)) { text = "✕" };

                tabElement.Add(selectButton);
                tabElement.Add(closeButton);
                _tabBar.Add(tabElement);
            }

            var addButton = new Button(CreateNewTab) { text = "+" };
            _tabBar.Add(addButton);
        }

        private void OnNewGraph()
        {
            CreateNewTab();
        }

        private void OnLoadGraph()
        {
            var path = EditorUtility.OpenFilePanel("Open a dialogue graph", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;

            var graph = DialogueGraphLoader.Load(path);
            if (graph == null) return;

            CreateNewTab();
            _activeTab.View.LoadGraph(graph);
            _activeTab.FilePath = path;
            _activeTab.GraphId = graph.GraphId;
            _activeTab.Title = string.IsNullOrEmpty(graph.Title) ? "Untitled" : graph.Title;

            _titleField.SetValueWithoutNotify(_activeTab.Title);
            RebuildTabBar();
            UpdateStatusLabel();
        }

        private void OnSaveGraph()
        {
            if (_activeTab == null) return;

            if (string.IsNullOrEmpty(_activeTab.FilePath))
            {
                OnSaveGraphAs();
                return;
            }

            SaveToPath(_activeTab.FilePath);
        }

        private void OnSaveGraphAs()
        {
            var path = EditorUtility.SaveFilePanel("Save the dialogue graph",
                Application.dataPath, "dialogue_graph", "json");
            if (string.IsNullOrEmpty(path)) return;

            SaveToPath(path);
        }

        private void SaveToPath(string path)
        {
            var graphData = _activeTab.View.BuildGraphData(_activeTab.GraphId);
            graphData.Title = _activeTab.Title;

            DialogueGraphSaver.Save(graphData, path);

            _activeTab.FilePath = path;
            _activeTab.IsDirty = false;

            RebuildTabBar();
            UpdateStatusLabel();

            if (path.StartsWith(Application.dataPath))
            {
                AssetDatabase.Refresh();
            }
        }

        private void UpdateStatusLabel()
        {
            _statusLabel.text = string.IsNullOrEmpty(_activeTab.FilePath)
                ? "No file loaded"
                : $"File: {_activeTab.FilePath}";
        }
    }
}