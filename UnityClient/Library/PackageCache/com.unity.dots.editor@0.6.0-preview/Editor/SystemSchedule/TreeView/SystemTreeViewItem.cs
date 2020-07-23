using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Editor.Bridge;

namespace Unity.Entities.Editor
{
    class SystemTreeViewItem : ITreeViewItem, IPoolable
    {
        readonly List<ITreeViewItem> m_CachedChildren = new List<ITreeViewItem>();
        public IPlayerLoopNode Node;
        public PlayerLoopSystemGraph Graph;
        public World World;
        public bool ShowInactiveSystems;

        const string k_ComponentToken = "c:";
        const int k_ComponentTokenLength = 2;

        public ComponentSystemBase System => (Node as IComponentSystemNode)?.System;

        public bool HasChildren => Node.Children.Count > 0;

        public string GetSystemName(World world = null)
        {
            return null == world ? Node.WorldName : Node.Name;
        }

        public bool GetParentState()
        {
            return Node.EnabledInHierarchy;
        }

        public void SetPlayerLoopSystemState(bool state)
        {
            Node.Enabled = state;
        }

        public void SetSystemState(bool state)
        {
            Node.Enabled = state;
        }

        public string GetEntityMatches()
        {
            if (HasChildren) // Group system do not need entity matches.
                return string.Empty;

            if (null == System?.EntityQueries)
                return string.Empty;

            var matchedEntityCount = System.EntityQueries.Sum(query => query.CalculateEntityCount());

            return matchedEntityCount.ToString();
        }

        float GetAverageRunningTime(ComponentSystemBase system)
        {
            switch (system)
            {
                case ComponentSystemGroup systemGroup:
                {
                    if (systemGroup.Systems != null)
                    {
                        return systemGroup.Systems.Sum(GetAverageRunningTime);
                    }
                }
                break;
                case ComponentSystemBase systemBase:
                {
                    return Graph.RecordersBySystem.ContainsKey(systemBase)
                        ? Graph.RecordersBySystem[systemBase].ReadMilliseconds()
                        : 0.0f;
                }
            }

            return -1;
        }

        public string GetRunningTime()
        {
            var totalTime = 0.0f;

            if (Node is IPlayerLoopSystemData)
            {
                return string.Empty;
            }

            if (children.Count() != 0)
            {
                totalTime = Node.Enabled
                    ? Node.Children.OfType<IComponentSystemNode>().Sum(child => GetAverageRunningTime(child.System))
                    : 0.0f;
            }
            else
            {
                if (Node.IsRunning && Node is IComponentSystemNode data)
                {
                    totalTime = Node.Enabled ? GetAverageRunningTime(data.System) : 0.0f;
                }
                else
                {
                    return "-";
                }
            }

            return totalTime.ToString("f2");
        }

        public int id => Node.Hash;
        public ITreeViewItem parent { get; internal set; }

        public IEnumerable<ITreeViewItem> children
        {
            get
            {
                if (m_CachedChildren.Count == 0 && Node.Children.Count > 0)
                {
                    PopulateChildren();
                }
                return m_CachedChildren;
            }
        }

        bool ITreeViewItem.hasChildren => HasChildren;

        public void AddChild(ITreeViewItem child)
        {
            throw new NotImplementedException();
        }

        public void AddChildren(IList<ITreeViewItem> children)
        {
            throw new NotImplementedException();
        }

        public void RemoveChild(ITreeViewItem child)
        {
            throw new NotImplementedException();
        }

        public void PopulateChildren(string searchFilter = null)
        {
            m_CachedChildren.Clear();
            foreach (var child in Node.Children)
            {
                if (!child.ShowForWorld(World))
                    continue;

                if (!child.IsRunning && !ShowInactiveSystems)
                    continue;

                // Filter systems by system name or whether contains given components.
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    if (!FilterSystem(child, searchFilter))
                        continue;
                }

                var item = SystemSchedulePool.GetSystemTreeViewItem(Graph, child, this, World, ShowInactiveSystems);
                m_CachedChildren.Add(item);
            }
        }

        static bool FilterSystem(IPlayerLoopNode node, string searchFilter)
        {
            switch (node)
            {
                case ComponentSystemBaseNode baseNode:
                {
                    return FilterBaseSystem(baseNode, searchFilter);
                }

                case ComponentGroupNode groupNode:
                {
                    if (groupNode.Children.Any(child => FilterSystem(child, searchFilter)))
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        static bool FilterBaseSystem(ComponentSystemBaseNode node, string searchFilter)
        {
            if (null == node)
                return true;

            var systemName = node.Name;

            using (var stringList = SplitSearchString(searchFilter).ToPooledList())
            {
                foreach (var singleString in stringList.List)
                {
                    if (singleString.StartsWith(k_ComponentToken, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!EntityQueryUtility.ContainsThisComponentType(node.System, singleString.Substring(k_ComponentTokenLength)))
                            return false;
                    }
                    else
                    {
                        systemName = systemName.Replace(" ", string.Empty);
                        if (systemName.IndexOf(singleString, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;
                    }
                }
            }

            return true;
        }

        public static IEnumerable<string> SplitSearchString(string searchString)
        {
            searchString = searchString.Trim();

            var stringArray = searchString.Split(' ');
            foreach (var singleString in stringArray)
            {
                yield return singleString;
            }
        }

        public void Reset()
        {
            World = null;
            Graph = null;
            Node = null;
            parent = null;
            ShowInactiveSystems = false;
            m_CachedChildren.Clear();
        }

        public void ReturnToPool()
        {
            foreach (var child in m_CachedChildren.OfType<SystemTreeViewItem>())
            {
                child.ReturnToPool();
            }

            SystemSchedulePool.ReturnToPool(this);
        }
    }
}
