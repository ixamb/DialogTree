using DialogTree.Editor.Utils;
using DialogTree.Runtime.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogTree.Editor.Nodes
{
    public sealed class UserNodeView : Node
    {
        private const int TitleMaxLength = 40;
        private static readonly Color HeaderColor = new(0.16f, 0.35f, 0.55f);

        public string Id { get; }
        public Port InputPort { get; }
        public Port OutputPort { get; }

        private readonly UserNode _dataData;

        public UserNodeView(UserNode dataData)
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

            var textField = new TextField("Text (User)") { multiline = true, value = _dataData.Text };
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.RegisterValueChangedCallback(evt =>
            {
                _dataData.Text = evt.newValue;
                title = GetDisplayTitle();
            });
            mainContainer.Insert(1, textField);

            var shortField = new TextField("Short") { value = _dataData.ShortText };
            shortField.tooltip = "Version displayed in the choices if the full text is too long. Leave blank for no short version.";
            shortField.RegisterValueChangedCallback(evt => _dataData.ShortText = evt.newValue);
            mainContainer.Insert(2, shortField);

            OutputPort = GraphViewUtils.CreatePort(this, "Out", Direction.Output);
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        private string GetDisplayTitle()
        {
            if (string.IsNullOrWhiteSpace(_dataData.Text)) return "User (empty)";
            var singleLine = _dataData.Text.Replace('\n', ' ').Replace('\r', ' ');
            return singleLine.Length > TitleMaxLength ? singleLine[..TitleMaxLength] + "…" : singleLine;
        }

        public void SetTarget(string targetNodeId) => _dataData.NextNodeId = targetNodeId;
        public void ClearTarget() => _dataData.NextNodeId = null;

        public UserNode GetData()
        {
            _dataData.Position = new Vector2(style.left.value.value, style.top.value.value);
            return _dataData;
        }
    }
}