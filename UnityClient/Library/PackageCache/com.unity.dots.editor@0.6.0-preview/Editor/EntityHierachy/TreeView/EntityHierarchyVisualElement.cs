using System;
using UnityEngine.UIElements;

namespace Unity.Entities.Editor
{
    class EntityHierarchyVisualElement : BindableElement, IBinding, IPoolable
    {
        public EntityHierarchyTreeView Owner { get; set; }

        readonly VisualElement m_Icon;
        readonly Label m_NameLabel;
        readonly VisualElement m_SystemButton;

        EntityHierarchyTreeViewItem m_Source;
        uint m_NodeVersion;

        public EntityHierarchyVisualElement()
        {
            binding = this;

            Resources.Templates.EntityHierarchyItem.Clone(this);
            AddToClassList(UssClasses.DotsEditorCommon.CommonResources);
            AddToClassList(UssClasses.Resources.EntityHierarchy);

            m_Icon = this.Q<VisualElement>(className: UssClasses.EntityHierarchyWindow.Item.Icon);
            m_NameLabel = this.Q<Label>(className: UssClasses.EntityHierarchyWindow.Item.NameLabel);
            m_SystemButton = this.Q<VisualElement>(className: UssClasses.EntityHierarchyWindow.Item.SystemButton);
        }

        public void SetSource(EntityHierarchyTreeViewItem source)
        {
            m_Source = source;
            Update();
        }

        void IBinding.PreUpdate() {}

        public void Update()
        {
            var nodeVersion = m_Source.Strategy.GetNodeVersion(m_Source.NodeId);
            if (m_NodeVersion == nodeVersion)
                return;

            m_NodeVersion = nodeVersion;
            ClearDynamicClasses();

            var nodeId = m_Source.NodeId;
            switch (nodeId.Kind)
            {
                case NodeKind.Entity:
                {
                    RenderEntityNode(m_Source.Strategy.GetNodeName(nodeId));
                    break;
                }
                case NodeKind.Scene:
                {
                    RenderSceneNode(nodeId);
                    break;
                }
                case NodeKind.Root:
                case NodeKind.None:
                {
                    RenderInvalidNode(nodeId);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        void IBinding.Release() {}

        void IPoolable.Reset()
        {
            Owner = null;
            m_Source = null;
        }

        void IPoolable.ReturnToPool() => EntityHierarchyPool.ReturnVisualElement(this);

        void RenderEntityNode(string label)
        {
            m_NameLabel.text = label;

            m_Icon.AddToClassList(UssClasses.EntityHierarchyWindow.Item.IconEntity);
            m_SystemButton.AddToClassList(UssClasses.EntityHierarchyWindow.Item.SystemButtonEntity);
        }

        void RenderSceneNode(EntityHierarchyNodeId nodeId)
        {
            m_NameLabel.AddToClassList(UssClasses.EntityHierarchyWindow.Item.NameScene);

            // TODO: Update once we have an official way to get scene names.
            m_NameLabel.text = $"Scene ({nodeId.ToString()})";

            m_Icon.AddToClassList(UssClasses.EntityHierarchyWindow.Item.IconScene);
        }

        void RenderInvalidNode(EntityHierarchyNodeId nodeId)
        {
            m_NameLabel.text = $"<UNKNOWN> ({nodeId.ToString()})";
        }

        void ClearDynamicClasses()
        {
            m_NameLabel.RemoveFromClassList(UssClasses.EntityHierarchyWindow.Item.NameScene);

            m_Icon.RemoveFromClassList(UssClasses.EntityHierarchyWindow.Item.IconScene);
            m_Icon.RemoveFromClassList(UssClasses.EntityHierarchyWindow.Item.IconEntity);

            m_SystemButton.RemoveFromClassList(UssClasses.EntityHierarchyWindow.Item.SystemButtonEntity);
        }
    }
}
