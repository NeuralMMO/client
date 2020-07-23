using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.QuickSearch
{
    class ExpressionGraph : GraphView, IDisposable
    {
        private SearchExpression m_Expression;

        private bool m_Disposed = false; // To detect redundant calls

        public event Action graphChanged;
        public event Action<SearchExpressionNode> nodeChanged;
        public event Action<IList<ISelectable>> selectionChanged;

        public ExpressionGraph(SearchExpression expression)
        {
            focusable = true;

            m_Expression = expression;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            ReloadExpression();

            graphViewChanged += OnGraphViewChanged;
        }

        ~ExpressionGraph()
        {
            Dispose(false);
        }

        private void NotifySelectionChanged()
        {
            EditorApplication.delayCall -= DelayNotifySelectionChanged;
            EditorApplication.delayCall += DelayNotifySelectionChanged;
        }

        private void DelayNotifySelectionChanged()
        {
            EditorApplication.delayCall -= DelayNotifySelectionChanged;
            selectionChanged?.Invoke(selection);
        }

        private void NotifyGraphChanged()
        {
            EditorApplication.update -= DelayNotifyGraphChanged;
            EditorApplication.update += DelayNotifyGraphChanged;
        }

        private void DelayNotifyGraphChanged()
        {
            EditorApplication.update -= DelayNotifyGraphChanged;
            graphChanged?.Invoke();
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            NotifySelectionChanged();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            NotifySelectionChanged();
        }

        public override EventPropagation DeleteSelection()
        {
            NotifySelectionChanged();
            return base.DeleteSelection();
        }

        public void Reload()
        {
            var edgesToRemove = this.Query<Edge>().ToList();
            foreach (var edge in edgesToRemove)
                RemoveElement(edge);

            var nodesToRemove = this.Query<Node>().ToList();
            foreach (var node in nodesToRemove)
                RemoveElement(node);

            ReloadExpression();
        }

        private bool TryGetNode(SearchExpressionNode ex, out Node node)
        {
            node = this.Query<Node>(ex.id).First();
            return node != null;
        }

        public void UpdateNode(SearchExpressionNode ex)
        {
            if (!TryGetNode(ex, out var node))
                return;

            node.title = FormatTitle(ex);

            if (ex.color != Color.clear)
                node.titleContainer.style.backgroundColor = ex.color;

            // Update result port
            if (ex.type == ExpressionType.Value ||
                ex.type == ExpressionType.Provider)
            {
                var outputPort = FindPort(node, "output");
                if (outputPort != null)
                    outputPort.portName = Convert.ToString(ex.value);
            }
            else if (ex.type == ExpressionType.Union)
            {
                UpdateUnionVariables(node);
            }
            else if (ex.type == ExpressionType.Select)
            {
                var outputPort = FindPort(node, "output");
                if (outputPort != null)
                    outputPort.portName = GetSelectNodeOutputPortName(ex);
            }
            else if (ex.type == ExpressionType.Map)
            {
                if (ex.TryGetVariableSource(ExpressionKeyName.X, out var xSource))
                {
                    if (!ex.TryGetProperty(ExpressionKeyName.GroupBy, out string groupBy) || string.IsNullOrEmpty(groupBy))
                        ex.SetProperty(ExpressionKeyName.GroupBy, GetSelectNodeOutputPortName(xSource).ToLowerInvariant());
                }

                if (ex.TryGetVariableSource(ExpressionKeyName.Y, out var ySource) && xSource == ySource)
                    ex.SetProperty(nameof(Mapping), (int)Mapping.Table);
            }

            NotifyGraphChanged();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (var t in Enum.GetValues(typeof(ExpressionType)))
            {
                var type = (ExpressionType)t;
                if (type == ExpressionType.Undefined || type == ExpressionType.Results)
                    continue;
                evt.menu.AppendAction($"Create {type}", menuAction =>
                {
                    var n = AddNode(type);
                    var localPos = VisualElementExtensions.ChangeCoordinatesTo(this, contentViewContainer, menuAction.eventInfo.localMousePosition);
                    n.SetPosition(new Rect(localPos, n.GetPosition().size));
                });
            }
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
        }

        private void UpdateUnionVariables(Node node)
        {
            if (!(node.userData is SearchExpressionNode ex))
                return;

            int indexName = 1;
            int connectedVars = 0;

            if (ex.variables != null)
            {
                foreach (var v in ex.variables)
                    if (v.source != null)
                        connectedVars++;
            }

            var ports = node.Query<Port>().Where(p => p.portType ==  typeof(ExpressionVariable)).ToList();
            foreach (var p in ports)
            {
                p.portName = indexName.ToString();
                indexName++;
            }

            if (connectedVars == ports.Count)
                AddInputPort(node, $"var-{ex.id}-{indexName}", indexName.ToString(), typeof(ExpressionVariable));

            node.RefreshPorts();
        }

        internal void AddNodeVariable(SearchExpressionNode ex, string varName)
        {
            if (!TryGetNode(ex, out var node))
                return;

            AddInputPort(node, $"var-{ex.id}-{varName}", varName, typeof(ExpressionVariable));
            node.RefreshPorts();
            NotifyGraphChanged();
        }

        internal bool RemoveNodeVariable(SearchExpressionNode ex, string varName)
        {
            if (!TryGetNode(ex, out var node))
                return false;

            var port = FindPort(node, "var", varName);
            if (port == null)
                return false;
            foreach (var c in port.connections.ToList())
                RemoveElement(c);
            RemoveElement(port);

            node.RefreshPorts();
            NotifyGraphChanged();

            return true;
        }

        private Port FindPort(Node node, string type, string name = null)
        {
            var portQuery = $"{type}-{node.name}";
            if (!String.IsNullOrEmpty(name))
                portQuery += $"-{name}";
            return node.Query<Port>(portQuery).First();
        }

        internal bool RenameNodeVariable(SearchExpressionNode ex, string oldVariableName, string newVariableName)
        {
            if (!TryGetNode(ex, out var node))
                return false;

            var port = FindPort(node, "var", oldVariableName);
            if (port == null)
                return false;

            port.name = $"var-{ex.id}-{newVariableName}";
            port.portName = newVariableName;
            node.RefreshPorts();
            NotifyGraphChanged();

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
             GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    graphViewChanged -= OnGraphViewChanged;
                }

                m_Disposed = true;
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
        {
            if (changes.elementsToRemove != null)
            {
                foreach (var rm in changes.elementsToRemove)
                {
                    if (rm is Edge edge)
                    {
                        if (edge.input?.node?.userData is SearchExpressionNode node)
                        {
                            if (typeof(ExpressionSource).IsAssignableFrom(edge.input.portType))
                                node.source = null;
                            else if (node.variables != null && typeof(ExpressionVariable).IsAssignableFrom(edge.input.portType))
                                node.SetVariableSource(edge.input.GetVarName(node), null);

                            nodeChanged?.Invoke(node);
                            NotifyGraphChanged();
                        }
                    }
                    else if (rm is Node graphNode && graphNode.userData is SearchExpressionNode ex)
                    {
                        if (ex.type == ExpressionType.Results)
                        {
                            Debug.LogWarning("You cannot remove final results node.");
                            changes.elementsToRemove.Remove(rm);
                            return changes;
                        }

                        m_Expression.RemoveNode(ex.id);
                        NotifyGraphChanged();
                    }
                }
            }

            if (changes.edgesToCreate != null)
            {
                foreach (var edge in changes.edgesToCreate)
                {
                    if (edge.input?.node?.userData is SearchExpressionNode nodeIn &&
                        edge.output?.node?.userData is SearchExpressionNode nodeOut)
                    {
                        if (typeof(ExpressionSource).IsAssignableFrom(edge.input.portType))
                            nodeIn.source = nodeOut;
                        else if (typeof(ExpressionVariable).IsAssignableFrom(edge.input.portType))
                            nodeIn.SetVariableSource(edge.input.GetVarName(nodeIn), nodeOut);
                        nodeChanged?.Invoke(nodeIn);
                        NotifyGraphChanged();
                    }
                }
            }

            return changes;
        }

        private void ReloadExpression()
        {
            foreach (var ex in m_Expression.nodes)
                AddNode(ex);
            AddConnections();

            EditorApplication.delayCall += () => EditorApplication.delayCall += () => FrameAll();
        }

        private void AddConnections()
        {
            foreach (var ex in m_Expression.nodes)
                AddConnections(ex);
        }

        private void AddConnection(Port outputPort, Port sourcePort, bool reconnect = false)
        {
            if (!reconnect || !outputPort.connected)
                AddElement(outputPort.ConnectTo(sourcePort));
            else
            {
                var edge = outputPort.connections.FirstOrDefault(e => e.input == null);
                if (edge != null)
                    sourcePort.Connect(edge);
            }
        }

        private string FormatTitle(SearchExpressionNode ex)
        {
            switch (ex.type)
            {
                case ExpressionType.Expression:
                    return ex.value != null ? System.IO.Path.GetFileNameWithoutExtension(Convert.ToString(ex.value)) : ex.name ?? ex.type.ToString();

                case ExpressionType.Value:
                    return ex.name ?? ex.type.ToString();

                case ExpressionType.Search:
                    if (String.IsNullOrEmpty(ex.name))
                        return Convert.ToString(ex.value);
                    return ex.name;

                case ExpressionType.Select:
                    return $"{ex.type} {ex.GetProperty("type", Convert.ToString(ex.value))}";
            }

            return ex.type.ToString();
        }

        private void OnNodeGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.target is Node nodeElement && nodeElement.userData is SearchExpressionNode node)
            {
                if (!evt.newRect.position.Equals(node.position))
                    node.position = evt.newRect.position;
            }
        }

        public Node AddNode(ExpressionType type)
        {
            var node = m_Expression.AddNode(type);
            var center = -contentViewContainer.transform.position;
            node.position = new Vector2(
                UnityEngine.Random.Range(center.x + 100f, center.x + contentRect.size.x - 100f),
                UnityEngine.Random.Range(center.y + 100f, center.y + contentRect.size.y - 100f));
            var graphNode = AddNode(node);
            ClearSelection();
            AddToSelection(graphNode);
            return graphNode;
        }

        private Node AddNode(SearchExpressionNode ex)
        {
            var node = new Node()
            {
                title = FormatTitle(ex),
                name = ex.id,
                expanded = true,
                userData = ex,
                tooltip = Convert.ToString(ex.value),
            };

            if (ex.color != Color.clear)
                node.titleContainer.style.backgroundColor = ex.color;

            node.SetPosition(new Rect(ex.position, Vector2.zero));
            node.RegisterCallback<GeometryChangedEvent>(OnNodeGeometryChanged);

            AddPorts(node, ex);
            AddElement(node);
            return node;
        }

        private static string GetSelectNodeOutputPortName(SearchExpressionNode ex)
        {
            var propertyName = ex.GetProperty("field", "Results");
            if (propertyName.StartsWith("m_", StringComparison.Ordinal))
                propertyName = propertyName.Substring(2);
            return propertyName;
        }

        private void AddPorts(Node node, SearchExpressionNode ex)
        {
            switch (ex.type)
            {
                case ExpressionType.Search:
                    AddInputPort(node, $"source-{ex.id}", "Source", typeof(ExpressionSource));
                    AddVariablePorts(node, ex);
                    AddOutputPort(node, $"output-{ex.id}", "Results", typeof(ExpressionSet));
                    break;

                case ExpressionType.Map:
                    AddInputPort(node, $"var-{ex.id}-{ExpressionKeyName.X}", ExpressionKeyName.X, typeof(ExpressionVariable));
                    AddInputPort(node, $"var-{ex.id}-{ExpressionKeyName.Y}", ExpressionKeyName.Y, typeof(ExpressionVariable));
                    AddOutputPort(node, $"output-{ex.id}", "Results", typeof(ExpressionSet));
                    break;

                case ExpressionType.Expression:
                    AddOutputPort(node, $"output-{ex.id}", "Results", typeof(ExpressionSet));
                    break;

                case ExpressionType.Select:
                    AddInputPort(node, $"source-{ex.id}", "Source", typeof(ExpressionResults));
                    AddOutputPort(node, $"output-{ex.id}", GetSelectNodeOutputPortName(ex), typeof(ExpressionSet));
                    break;

                case ExpressionType.Results:
                    AddInputPort(node, $"source-{ex.id}", "Source", typeof(ExpressionResults));
                    break;
                case ExpressionType.Provider:
                    AddOutputPort(node, $"output-{ex.id}", Convert.ToString(ex.value), typeof(ExpressionProvider));
                    break;

                case ExpressionType.Value:
                    AddOutputPort(node, $"output-{ex.id}", Convert.ToString(ex.value), typeof(ExpressionSet));
                    break;

                case ExpressionType.Union:
                    AddVariablePorts(node, ex);
                    UpdateUnionVariables(node);
                    AddOutputPort(node, $"output-{ex.id}", "Results", typeof(ExpressionSet));
                    break;

                case ExpressionType.Intersect:
                case ExpressionType.Except:
                    AddInputPort(node, $"source-{ex.id}", "Source", typeof(ExpressionResults));
                    AddInputPort(node, $"var-{ex.id}-With", "With", typeof(ExpressionVariable));
                    AddOutputPort(node, $"output-{ex.id}", "Results", typeof(ExpressionSet));
                    break;

                default:
                    throw new NotSupportedException($"Expression {ex.type} {ex.id} has no support ports");
            }

            node.RefreshPorts();
        }

        private void AddConnections(SearchExpressionNode ex)
        {
            switch (ex.type)
            {
                case ExpressionType.Search:
                case ExpressionType.Select:
                case ExpressionType.Union:
                case ExpressionType.Intersect:
                case ExpressionType.Except:
                case ExpressionType.Results:
                case ExpressionType.Expression:
                case ExpressionType.Map:
                {
                    if (ex.source != null)
                    {
                        var sourcePort = this.Query<Port>($"source-{ex.id}").First();
                        var outputPort = this.Query<Port>($"output-{ex.source.id}").First();
                        if (sourcePort != null && outputPort != null)
                            AddConnection(outputPort, sourcePort);
                    }

                    if (ex.variables != null)
                    {
                        foreach (var v in ex.variables)
                        {
                            if (v.source == null)
                                continue;

                            var sourcePort = this.Query<Port>($"var-{ex.id}-{v.name}").First();
                            var outputPort = this.Query<Port>($"output-{v.source.id}").First();
                            if (sourcePort != null && outputPort != null)
                                AddConnection(outputPort, sourcePort);
                        }
                    }

                    break;
                }

                case ExpressionType.Value:
                case ExpressionType.Provider:
                    // No source to connect for these nodes.
                    break;

                default:
                    throw new NotSupportedException($"Do not know how to connect ports for {ex.type} {ex.id}");
            }
        }

        private void AddVariablePorts(Node node, SearchExpressionNode ex)
        {
            if (ex.variables == null)
                return;
            foreach (var v in ex.variables)
                AddInputPort(node, $"var-{ex.id}-{v.name}", v.name, typeof(ExpressionVariable));
        }

        private void AddInputPort(Node node, string id, string name, Type type)
        {
            AddPort(node, id, name, Direction.Input, Port.Capacity.Single, type);
        }

        private void AddOutputPort(Node node, string id, string name, Type type)
        {
            AddPort(node, id, name, Direction.Output, Port.Capacity.Multi, type);
        }

        private void AddPort(Node node, string id, string name, Direction direction, Port.Capacity capacity, Type type)
        {
            var port = node.InstantiatePort(Orientation.Horizontal, direction, capacity, type);
            port.name = id;
            port.portName = name;
            var children = node.inputContainer.Children().ToList();
            var insertAt = children.FindLastIndex(v => v is Port p && p.direction == direction && type == p.portType);
            if (insertAt == -1)
                node.inputContainer.Add(port);
            else
                node.inputContainer.Insert(insertAt+1, port);
        }
    }
}