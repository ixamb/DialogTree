using System;
using System.Collections.Generic;
using DialogTree.Editor.Utils;
using DialogTree.Runtime.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogTree.Editor.Nodes
{
    public sealed class CpuNodeView : Node
    {
        private const int TitleMaxLength = 40;
        private static readonly Color HeaderColor = new(0.55f, 0.35f, 0.10f);

        public string Id { get; }
        public Port InputPort { get; }
        public Dictionary<string, Port> OutputPorts { get; } = new();

        public event Action<CpuNodeView> OutputsChanged;

        private readonly CpuNode _dataData;
        private readonly VisualElement _outputsContainer;

        public CpuNodeView(CpuNode dataData)
        {
            _dataData = dataData;
            Id = dataData.Id;

            title = GetDisplayTitle();
            viewDataKey = dataData.Id;
            style.left = dataData.Position.x;
            style.top = dataData.Position.y;
            titleContainer.style.backgroundColor = HeaderColor;

            InputPort = GraphViewUtils.CreatePort(this, "In", Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(InputPort);

            var textField = new TextField("Text (CPU)")
            {
                multiline = true, value = _dataData.Text,
                style = { whiteSpace = WhiteSpace.Normal }
            };
            textField.RegisterValueChangedCallback(evt =>
            {
                _dataData.Text = evt.newValue;
                title = GetDisplayTitle();
            });
            mainContainer.Insert(1, textField);

            _outputsContainer = new VisualElement { name = "outputs-container" };
            outputContainer.Add(_outputsContainer);
            RefreshOutputPorts();

            var addOutputButton = new Button(AddOutput) { text = "+ Output" };
            outputContainer.Add(addOutputButton);

            RefreshExpandedState();
            RefreshPorts();
        }

        public sealed override string title
        {
            get => base.title;
            set => base.title = value;
        }

        private string GetDisplayTitle()
        {
            if (string.IsNullOrWhiteSpace(_dataData.Text)) return "CPU (empty)";
            var singleLine = _dataData.Text.Replace('\n', ' ').Replace('\r', ' ');
            return singleLine.Length > TitleMaxLength ? singleLine[..TitleMaxLength] + "…" : singleLine;
        }

        private void AddOutput()
        {
            _dataData.Outputs.Add(new CpuOutputNode { Id = Guid.NewGuid().ToString("N") });
            RefreshOutputPorts();
            OutputsChanged?.Invoke(this);
        }

        private void RemoveOutput(CpuOutputNode output)
        {
            _dataData.Outputs.Remove(output);
            RefreshOutputPorts();
            OutputsChanged?.Invoke(this);
        }

        private void RefreshOutputPorts()
        {
            _outputsContainer.Clear();
            OutputPorts.Clear();

            foreach (var output in _dataData.Outputs)
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                var removeButton = new Button(() => RemoveOutput(output)) { text = "✕" };
                var port = GraphViewUtils.CreatePort(this, string.Empty, Direction.Output);
                port.userData = output;

                row.Add(removeButton);
                row.Add(port);

                _outputsContainer.Add(row);
                OutputPorts[output.Id] = port;
            }

            RefreshPorts();
            RefreshExpandedState();
        }

        public void SetOutputTarget(string outputId, string targetNodeId)
        {
            var output = _dataData.Outputs.Find(o => o.Id == outputId);
            if (output != null) output.NextNodeId = targetNodeId;
        }

        public CpuNode GetData()
        {
            _dataData.Position = new Vector2(style.left.value.value, style.top.value.value);
            return _dataData;
        }
    }
}