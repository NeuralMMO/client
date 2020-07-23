using System;
using Unity.Properties.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class BindingTarget<TTarget> : IBindingTarget<TTarget>
    {
        TTarget m_Target;
        readonly InspectorVisitor<TTarget> m_Visitor;
        readonly BindingVisitor m_BindingVisitor;

        public TTarget Target
        {
            get => m_Target;
            set => m_Target = value;
        }
        public PropertyElement Root { get; }
        public IInspectorVisitor Visitor { get; }
        public Type TargetType { get; }
        
        public BindingTarget(PropertyElement root, TTarget target)
        {
            Root = root;
            m_Target = target;
            
            TargetType = m_Target.GetType();
            m_Visitor = new InspectorVisitor<TTarget>(root, target);
            Visitor = m_Visitor;
            m_BindingVisitor = new BindingVisitor();
        }

        public bool TryGetTarget<T>(out T target)
        {
            if (m_Target is T t)
            {
                target = t;
                return true;
            }

            target = default;
            return false;
        }

        public bool IsPathValid(PropertyPath path)
        {
            if (path.PartsCount == 0)
            {
                return true;
            }
            return PropertyContainer.IsPathValid(ref m_Target, path);
        }

        public void RegisterBindings(PropertyPath path, VisualElement element)
        {
            m_BindingVisitor.Reset();
            m_BindingVisitor.Path = path;
            m_BindingVisitor.Root = Root;
            m_BindingVisitor.Element = element;
            PropertyContainer.Visit(ref m_Target, m_BindingVisitor);
        }

        public void VisitAtPath(PropertyPath path, VisualElement parent)
        {
            var contextPath = new PropertyPath();
            contextPath.PushPath(path);
            contextPath.Pop();

            using (m_Visitor.VisitorContext.MakePropertyPathScope(m_Visitor, contextPath))
            using (m_Visitor.VisitorContext.MakeParentScope(parent))
                PropertyContainer.Visit(ref m_Target, m_Visitor, path);
        }

        public void SetAtPath<TValue>(PropertyPath path, TValue value)
        {
            PropertyContainer.SetValue(ref m_Target, path, value);
        }

        public bool TrySetAtPath<TValue>(PropertyPath path, TValue value)
        {
            return PropertyContainer.TrySetValue(ref m_Target, path, value);
        }

        public TValue GetAtPath<TValue>(PropertyPath path)
        {
            return PropertyContainer.GetValue<TTarget, TValue>(ref m_Target, path);
        }

        public bool TryGetAtPath<TValueType>(PropertyPath path, out TValueType value)
        {
            return PropertyContainer.TryGetValue(ref m_Target, path, out value);
        }

        public bool TryGetProperty(PropertyPath path, out IProperty property)
        {
            return PropertyContainer.TryGetProperty(ref m_Target, path, out property);
        }

        public void GenerateHierarchy()
        {
            Root.Clear();
            using (m_Visitor.VisitorContext.MakeParentScope(Root))
            {

                var wrapper = new PropertyWrapper<TTarget>(Target);
                PropertyContainer.Visit(ref wrapper, m_Visitor);
            }
        }
        
        public void Release()
        {
            Root.Clear();
        }
    }
}