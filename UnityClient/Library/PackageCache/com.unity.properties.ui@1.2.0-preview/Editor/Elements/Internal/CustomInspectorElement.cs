using System;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class CustomInspectorElement : VisualElement, IBindable, IBinding
    {
        readonly PropertyPath m_BasePath;
        readonly PropertyElement m_Root;
        readonly VisualElement m_Content;
        readonly IInspector m_Inspector;
        readonly PropertyPath m_RelativePath = new PropertyPath();
        readonly PropertyPath m_AbsolutePath = new PropertyPath();

        public IBinding binding { get; set; }
        
        public string bindingPath { get; set; }
        bool HasInspector { get; }

        public IInspector Inspector => m_Inspector;
        
        public CustomInspectorElement(PropertyPath basePath, IInspector inspector, PropertyElement root)
        {
            m_Root = root;
            binding = this;
            m_BasePath = basePath;
            name = inspector.Type.Name;
            m_Inspector = inspector;
            m_Inspector.Parent = this;
            m_Content = m_Inspector.Build();
            if (null == m_Content)
            {
                return;
            }

            if (this != m_Content)
            {
                Add(m_Content);
                RefreshBindings();
            }
            
            HasInspector = true;
        }
        
        void IBinding.PreUpdate()
        {
            // Nothing to do.
        }

        void IBinding.Update()
        {
            if (!HasInspector)
            {
                return;
            }
            if (!m_Root.IsPathValid(m_BasePath))
            {
                return;
            }
            m_Inspector.Update();
        }

        void IBinding.Release()
        {
            // Nothing to do.
        }
        
        void RefreshBindings()
        {
            RegisterBinding(contentContainer);
        }

        void RegisterBinding(VisualElement content, bool foundRoot = false)
        {
            if (content is CustomInspectorElement && content != this)
            {
                return;
            }

            if (content is PropertyElement)
            {
                return;
            }
            
            var bindable = false;
            if (content is IBindable b && !string.IsNullOrEmpty(b.bindingPath))
            {
                bindable = true;

                if (!foundRoot)
                {
                    foundRoot = true;
                    switch (m_Inspector.Part.Type)
                    {
                        case PropertyPath.PartType.Name:
                            m_RelativePath.PushName(b.bindingPath);
                            break;
                        case PropertyPath.PartType.Index:
                            m_RelativePath.PushIndex(int.Parse(b.bindingPath.Substring(1, b.bindingPath.Length -2)));
                            break;
                        case PropertyPath.PartType.Key:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    m_RelativePath.PushName(b.bindingPath);
                }
                
                if (m_Inspector.IsPathValid(m_RelativePath))
                {
                    var path = new PropertyPath();
                    path.PushPath(m_Inspector.PropertyPath);
                    if (path.PartsCount > 0)
                    {
                        path.Pop();
                    }

                    path.PushPath(m_RelativePath);
                    m_Inspector.RegisterBindings(path, content);
                }
                
                if (content == m_Content)
                {
                    switch (m_Inspector.Part.Type)
                    {
                        case PropertyPath.PartType.Name:
                            m_AbsolutePath.PushName(b.bindingPath);
                            break;
                        case PropertyPath.PartType.Index:
                            m_RelativePath.PushIndex(int.Parse(b.bindingPath.Substring(1, b.bindingPath.Length -2)));
                            break;
                        case PropertyPath.PartType.Key:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    m_AbsolutePath.PushName(b.bindingPath);
                }
                
                if (m_Inspector.IsPathValid(m_AbsolutePath) && m_RelativePath.ToString() != b.bindingPath)
                {
                    var path = new PropertyPath();
                    path.PushPath(m_Inspector.PropertyPath);
                    path.Pop();
                    path.PushPath(m_AbsolutePath);
                    m_Inspector.RegisterBindings(path, content);
                }
                m_AbsolutePath.Clear();
            }
            foreach (var child in content.Children())
            {
                RegisterBinding(child.contentContainer, foundRoot);
            }
            if (bindable)
            {
                m_RelativePath.Pop();
            }
        }
    }
}