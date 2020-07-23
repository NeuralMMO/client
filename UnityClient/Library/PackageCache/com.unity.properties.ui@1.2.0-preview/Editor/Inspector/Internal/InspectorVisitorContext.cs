using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.Properties.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class InspectorVisitorContext
    {
        internal struct ParentScope : IDisposable
        {
            readonly InspectorVisitorContext m_Context;
            readonly VisualElement m_Parent;
            
            public ParentScope(InspectorVisitorContext context, VisualElement parent)
            {
                m_Context = context;
                m_Parent = parent;
                m_Context.PushParent(m_Parent);
            }
            
            public void Dispose()
            {
                m_Context.PopParent(m_Parent);
            }
        }
        
        internal struct PropertyPathScope : IDisposable
        {
            readonly IInspectorVisitor m_Visitor;
            readonly PropertyPath m_Path;
            
            public PropertyPathScope(IInspectorVisitor visitor, PropertyPath path)
            {
                m_Visitor = visitor;
                m_Path = visitor.GetCurrentPath();
                m_Visitor.ClearPath();
                m_Visitor.AddToPath(path);
            }
            
            public void Dispose()
            {
                m_Visitor.ClearPath();
                m_Visitor.RestorePath(m_Path);
            }
        }
        
        internal struct VisitedReferencesScope<TValue> : IDisposable
        {
            readonly IInspectorVisitor m_Visitor;
            readonly object m_Object;
            readonly bool m_ReferenceType;
            public readonly bool VisitedOnCurrentBranch;

            public PropertyPath GetReferencePath()
            {
                return m_Visitor.VisitorContext.m_References.GetPath(m_Object);
            }
            
            public VisitedReferencesScope(IInspectorVisitor visitor, ref TValue value, PropertyPath path)
            {
                m_Visitor = visitor;
                m_ReferenceType = !RuntimeTypeInfoCache<TValue>.IsValueType;
                
                if (m_ReferenceType)
                {
                    if (null == value)
                    {
                        m_Object = null;
                        VisitedOnCurrentBranch = false;
                        return;
                    }

                    m_ReferenceType = !value.GetType().IsValueType;
                }

                if (m_ReferenceType)
                {
                    m_Object = value;
                    VisitedOnCurrentBranch = !m_Visitor.VisitorContext.PushReference(value, path);
                }
                else
                {
                    m_Object = null;
                    VisitedOnCurrentBranch = false;
                }
            }
            
            public void Dispose()
            {
                if (m_ReferenceType)
                {
                    m_Visitor.VisitorContext.PopReference(m_Object);
                }
            }
        }
        
        readonly Stack<VisualElement> m_ParentStack;
        readonly InspectedReferences m_References;
        public readonly PropertyElement Root;
        
        internal InspectorVisitorContext(PropertyElement root)
        {
            m_ParentStack = new Stack<VisualElement>();
            m_References = new InspectedReferences();
            Root = root;
        }

        public PropertyPathScope MakePropertyPathScope(IInspectorVisitor visitor, PropertyPath path)
        {
            return new PropertyPathScope(visitor, path);
        }
        
        public ParentScope MakeParentScope(VisualElement parent)
        {
            return new ParentScope(this, parent);
        }

        public VisitedReferencesScope<TValue> MakeVisitedReferencesScope<TValue>(IInspectorVisitor visitor, ref TValue value, PropertyPath path)
        {
            return new VisitedReferencesScope<TValue>(visitor, ref value, path);
        }
        
        void PushParent(VisualElement parent)
        {
            m_ParentStack.Push(parent);
        }

        void PopParent(VisualElement parent)
        {
            if (m_ParentStack.Peek() == parent)
            {
                m_ParentStack.Pop();
            }
            else
            {
                Debug.LogError($"{nameof(InspectorVisitorContext)}.{nameof(MakeParentScope)} was not properly disposed for parent: {parent?.name}");
            }
        }

        public VisualElement Parent
        {
            get
            {
                if (m_ParentStack.Count > 0)
                {
                    return m_ParentStack.Peek();
                }
                throw new InvalidOperationException($"A parent element must be set.");
            }
        }

        bool PushReference(object obj, PropertyPath path)
             => m_References.PushReference(obj, path);

        void PopReference(object obj)
            => m_References.PopReference(obj);
    }
}