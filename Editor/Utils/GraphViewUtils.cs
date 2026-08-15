using UnityEditor.Experimental.GraphView;

namespace DialogTree.Editor.Utils
{
    public static class GraphViewUtils
    {
        public static Port CreatePort(Node node, string portName, Direction direction,
            Port.Capacity capacity = Port.Capacity.Single)
        {
            var port = node.InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }
    }
}
