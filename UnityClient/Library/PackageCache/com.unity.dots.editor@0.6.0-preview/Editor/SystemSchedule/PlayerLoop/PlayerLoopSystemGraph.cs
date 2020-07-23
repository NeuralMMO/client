using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.LowLevel;
using UnityEngine.Profiling;

namespace Unity.Entities.Editor
{
    using SystemWrapper = ScriptBehaviourUpdateOrder.DummyDelegateWrapper;

    class PlayerLoopSystemGraph
    {
        static DateTime s_LastValidate;
        static readonly TimeSpan s_OneSecond = TimeSpan.FromSeconds(1);

        // We only support one graph at a time for now, so this is encapsulated here.
        public static PlayerLoopSystemGraph Current { get; private set; } = new PlayerLoopSystemGraph();
        public static event Action OnGraphChanged;

        static PlayerLoopSystemGraph()
        {
            ParsePlayerLoopSystem(PlayerLoop.GetCurrentPlayerLoop(), Current);
            s_LastValidate = DateTime.Now;
            EditorApplication.update += ValidateCurrentGraph;
        }

        static void ValidateCurrentGraph()
        {
            var now = DateTime.Now;
            if (now - s_LastValidate < s_OneSecond)
            {
                return;
            }
            s_LastValidate = now;

            var graph = new PlayerLoopSystemGraph();
            ParsePlayerLoopSystem(PlayerLoop.GetCurrentPlayerLoop(), graph);
            if (!DidChange(Current, graph))
            {
                graph.Reset();
                return;
            }

            Current.Reset();
            Current = graph;
            OnGraphChanged?.Invoke();
        }

        static bool DidChange(PlayerLoopSystemGraph lhs, PlayerLoopSystemGraph rhs)
        {
            if (lhs.Roots.Count != rhs.Roots.Count)
                return true;

            for (var i = 0; i < lhs.Roots.Count; ++i)
            {
                if (DidChange(lhs.Roots[i], rhs.Roots[i]))
                    return true;
            }

            return false;
        }

        static bool DidChange(IPlayerLoopNode lhs, IPlayerLoopNode rhs)
        {
            if (lhs.Parent?.Hash != rhs.Parent?.Hash)
                return true;

            if (lhs.Children.Count != rhs.Children.Count)
                return true;

            if (lhs.Hash != rhs.Hash)
                return true;

            for (var i = 0; i < lhs.Children.Count; ++i)
            {
                if (DidChange(lhs.Children[i], rhs.Children[i]))
                    return true;
            }

            return false;
        }

        public readonly List<IPlayerLoopNode> Roots = new List<IPlayerLoopNode>();

        public readonly Dictionary<ComponentSystemBase, AverageRecorder> RecordersBySystem = new Dictionary<ComponentSystemBase, AverageRecorder>();

        public readonly List<ComponentSystemBase> AllSystems = new List<ComponentSystemBase>();

        public void Reset()
        {
            foreach (var root in Roots)
            {
                root.ReturnToPool();
            }
            Roots.Clear();
            RecordersBySystem.Clear();
            AllSystems.Clear();
        }

        // Parse through the player loop system to get all system list and their parent-children relationship,
        // which will be used to build the treeview.
        public static void ParsePlayerLoopSystem(PlayerLoopSystem rootPlayerLoopSystem, PlayerLoopSystemGraph graph)
        {
            graph.Reset();
            Parse(rootPlayerLoopSystem, graph);
        }

        static void Parse(PlayerLoopSystem playerLoopSystem, PlayerLoopSystemGraph graph, IPlayerLoopNode parent = null)
        {
            // The integration of `ComponentSystemBase` into the player loop is done through a wrapper type.
            // If the target of the player loop system is the wrapper type, we will parse this as a `ComponentSystemBase`.
            if (null != playerLoopSystem.updateDelegate && playerLoopSystem.updateDelegate.Target is SystemWrapper wrapper)
            {
                Parse(wrapper.System, graph, parent);
                return;
            }

            // Add the player loop system to the graph if it is not the root one.
            if (null != playerLoopSystem.type)
            {
                var playerLoopSystemNode = Pool<PlayerLoopSystemNode>.GetPooled();
                playerLoopSystemNode.Value = playerLoopSystem;
                var node = playerLoopSystemNode;
                AddToGraph(graph, node, parent);
                parent = node;
            }

            if (null == playerLoopSystem.subSystemList)
                return;

            foreach (var subSystem in playerLoopSystem.subSystemList)
            {
                Parse(subSystem, graph, parent);
            }
        }

        static void Parse(ComponentSystemBase system, PlayerLoopSystemGraph graph, IPlayerLoopNode parent = null)
        {
            IPlayerLoopNode node;

            graph.AllSystems.Add(system);

            switch (system)
            {
                case ComponentSystemGroup group:
                    var groupNode = Pool<ComponentGroupNode>.GetPooled();
                    groupNode.Value = group;
                    node = groupNode;
                    foreach (var s in group.Systems)
                    {
                        Parse(s, graph, node);
                    }
                    break;

                default:
                    var systemNode = Pool<ComponentSystemBaseNode>.GetPooled();
                    systemNode.Value = system;
                    node = systemNode;

                    var recorder = Recorder.Get($"{system.World?.Name ?? "none"} {system.GetType().FullName}");
                    if (!graph.RecordersBySystem.ContainsKey(system))
                    {
                        graph.RecordersBySystem.Add(system, new AverageRecorder(recorder));
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("System added twice: " + system);
                    }

                    recorder.enabled = true;
                    break;
            }

            AddToGraph(graph, node, parent);
        }

        static void AddToGraph(PlayerLoopSystemGraph graph, IPlayerLoopNode node, IPlayerLoopNode parent = null)
        {
            if (null == parent)
            {
                graph.Roots.Add(node);
            }
            else
            {
                node.Parent = parent;
                parent.Children.Add(node);
            }
        }
    }
}
