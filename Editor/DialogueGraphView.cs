using System;
using System.Collections.Generic;
using System.Linq;
using DialogTree.Editor.Nodes;
using DialogTree.Runtime.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogTree.Editor
{
    public sealed class DialogueGraphView : GraphView
    {
        private readonly Dictionary<string, CpuNodeView> _cpuNodes = new();
        private readonly Dictionary<string, UserNodeView> _userNodes = new();
        private StartNodeView _startNode;

        public DialogueGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var styleSheet = Resources.Load<StyleSheet>("Styles/DialogueGraph");
            if (styleSheet != null) styleSheets.Add(styleSheet);

            graphViewChanged = OnGraphViewChanged;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var localMousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);

            evt.menu.AppendAction("New CPU Node",
                _ => CreateCpuNode(new CpuNode
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Text = "New message",
                    Position = localMousePosition
                }));

            evt.menu.AppendAction("New User Node",
                _ => CreateUserNode(new UserNode
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Text = "New message",
                    Position = localMousePosition
                }));

            if (_startNode == null)
            {
                evt.menu.AppendAction("New Start Node",
                    _ => CreateStartNode(new StartNode
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Position = localMousePosition
                    }));
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(port =>
                port.direction != startPort.direction &&
                port.node != startPort.node
            ).ToList();
        }
        private CpuNodeView CreateCpuNode(CpuNode dataData)
        {
            var nodeView = new CpuNodeView(dataData);
            AddElement(nodeView);
            _cpuNodes[dataData.Id] = nodeView;
            return nodeView;
        }

        private UserNodeView CreateUserNode(UserNode dataData)
        {
            var nodeView = new UserNodeView(dataData);
            AddElement(nodeView);
            _userNodes[dataData.Id] = nodeView;
            return nodeView;
        }

        private StartNodeView CreateStartNode(StartNode dataData)
        {
            if (_startNode != null)
            {
                Debug.LogWarning("[DialogueTreeEditor] A start node already exists!");
                return _startNode;
            }

            _startNode = new StartNodeView(dataData);
            AddElement(_startNode);
            return _startNode;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    ApplyEdgeTarget(edge, connect: true);
                }
            }

            if (change.elementsToRemove == null)
                return change;

            foreach (var element in change.elementsToRemove)
            {
                switch (element)
                {
                    case Edge edge:
                        ApplyEdgeTarget(edge, connect: false);
                        break;
                    case CpuNodeView cpuNode:
                        _cpuNodes.Remove(cpuNode.Id);
                        break;
                    case UserNodeView userNode:
                        _userNodes.Remove(userNode.Id);
                        break;
                    case StartNodeView:
                        _startNode = null;
                        break;
                }
            }

            return change;
        }

        private static void ApplyEdgeTarget(Edge edge, bool connect)
        {
            var targetNodeId = GetNodeId(edge.input?.node);
            var value = connect ? targetNodeId : null;

            switch (edge.output?.node)
            {
                case StartNodeView start:
                    if (connect) start.SetTarget(value);
                    else start.ClearTarget();
                    break;

                case CpuNodeView cpu:
                    if (edge.output.userData is CpuOutputNode output)
                    {
                        cpu.SetOutputTarget(output.Id, value);
                    }
                    break;

                case UserNodeView user:
                    if (connect) user.SetTarget(value);
                    else user.ClearTarget();
                    break;
            }
        }

        private static string GetNodeId(Node node) => node switch
        {
            CpuNodeView c => c.Id,
            UserNodeView u => u.Id,
            StartNodeView s => s.Id,
            _ => null
        };

        public DialogGraph BuildGraphData(string graphId)
        {
            var graph = new DialogGraph { GraphId = graphId };

            if (_startNode != null) graph.Nodes.Add(_startNode.GetData());
            foreach (var node in _cpuNodes.Values) graph.Nodes.Add(node.GetData());
            foreach (var node in _userNodes.Values) graph.Nodes.Add(node.GetData());

            return graph;
        }

        public void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
            _cpuNodes.Clear();
            _userNodes.Clear();
            _startNode = null;
        }

        public void LoadGraph(DialogGraph graph)
        {
            ClearGraph();
            if (graph == null) return;

            foreach (var nodeData in graph.Nodes)
            {
                switch (nodeData)
                {
                    case StartNode start: CreateStartNode(start); break;
                    case CpuNode cpu: CreateCpuNode(cpu); break;
                    case UserNode user: CreateUserNode(user); break;
                }
            }

            foreach (var nodeData in graph.Nodes)
            {
                switch (nodeData)
                {
                    case StartNode start when !string.IsNullOrEmpty(start.NextNodeId):
                        ConnectPorts(_startNode.OutputPort, GetInputPort(start.NextNodeId));
                        break;

                    case CpuNode cpu:
                        foreach (var output in cpu.Outputs)
                        {
                            if (string.IsNullOrEmpty(output.NextNodeId)) continue;
                            if (!_cpuNodes[cpu.Id].OutputPorts.TryGetValue(output.Id, out var outPort)) continue;
                            ConnectPorts(outPort, GetInputPort(output.NextNodeId));
                        }
                        break;

                    case UserNode user when !string.IsNullOrEmpty(user.NextNodeId):
                        ConnectPorts(_userNodes[user.Id].OutputPort, GetInputPort(user.NextNodeId));
                        break;
                }
            }
        }

        private Port GetInputPort(string nodeId)
        {
            if (_cpuNodes.TryGetValue(nodeId, out var cpu))
                return cpu.InputPort;

            if (_userNodes.TryGetValue(nodeId, out var user))
                return user.InputPort;

            return null;
        }

        private void ConnectPorts(Port output, Port input)
        {
            if (output == null || input == null) return;
            var edge = output.ConnectTo(input);
            AddElement(edge);
        }
    }
}