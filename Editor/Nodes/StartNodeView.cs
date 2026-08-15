using DialogTree.Editor.Utils;
using DialogTree.Runtime.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogTree.Editor.Nodes
{
    public sealed class StartNodeView : Node
    {
        public string Id { get; }
        public Port OutputPort { get; }

        private readonly StartNode _dataData;

        public StartNodeView(StartNode dataData)
        {
            _dataData = dataData;
            Id = dataData.Id;

            title = "Start";
            viewDataKey = dataData.Id;
            style.left = dataData.Position.x;
            style.top = dataData.Position.y;

            capabilities &= ~Capabilities.Deletable;

            OutputPort = GraphViewUtils.CreatePort(this, "Out", Direction.Output);
            outputContainer.Add(OutputPort);

            RefreshExpandedState();
            RefreshPorts();
        }

        public sealed override string title
        {
            get { return base.title; }
            set { base.title = value; }
        }

        public void SetTarget(string targetNodeId)
        {
            _dataData.NextNodeId = targetNodeId;
        }

        public void ClearTarget()
        {
            _dataData.NextNodeId = null;
        }

        public StartNode GetData()
        {
            _dataData.Position = new Vector2(style.left.value.value, style.top.value.value);
            return _dataData;
        }
    }
}
