using System;
using Unity.Properties.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class InspectorVisitor<T> : PropertyVisitor, IInspectorVisitor
    {
        public bool EnableRootCustomInspectors = true;
        
        public T Target { get; }
        public InspectorVisitorContext VisitorContext { get; }

        readonly PropertyPath m_Path = new PropertyPath();

        public InspectorVisitor(PropertyElement bindingElement, T target)
        {
            VisitorContext = new InspectorVisitorContext(bindingElement);
            Target = target;
            AddAdapter(new NullAdapter<T>(this));
            AddAdapter(new BuiltInAdapter<T>(this));
        }

        public void PushPathPart(PropertyPath.Part path)
        {
            m_Path.PushPart(path);
        }

        public void PopPathPart()
        {
            m_Path.Pop();
        }

        public void ClearPath()
        {
            m_Path.Clear();
        }

        public void RestorePath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PushPathPart(path[i]);
            }
        }

        public void AddToPath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PushPathPart(path[i]);
            }
        }

        public void RemoveFromPath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PopPathPart();
            }
        }

        public void AddToPath(IProperty property)
        {
            if (property is IListElementProperty listElementProperty)
            {
                PushPathPart(new PropertyPath.Part(listElementProperty.Index));
            }
            else if (property is IDictionaryElementProperty dictionaryElementProperty)
            {
                PushPathPart(new PropertyPath.Part((object)dictionaryElementProperty.ObjectKey));
            }
            else
            {
                PushPathPart(new PropertyPath.Part(property.Name));
            }
        }

        public void RemoveFromPath(IProperty property)
        {
            PopPathPart();
        }

        public PropertyPath GetCurrentPath()
        {
            var path = new PropertyPath();
            path.PushPath(m_Path);
            return path;
        }

        protected override bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property,
            ref TContainer container, ref TValue value)
        {
            var shouldShow = true;
            if (null != VisitorContext.Root.m_AttributeFilter && !(property is IPropertyWrapper))
            {
                shouldShow = VisitorContext.Root.m_AttributeFilter(property.GetAttributes());
            }
            return !shouldShow || !IsFieldTypeSupported<TContainer, TValue>() || !ShouldShowField(property);
        }

        protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property,
            ref TContainer container, ref TValue value)
        {
            if (!RuntimeTypeInfoCache.IsContainerType(value.GetType()))
            {
                return;
            }

            var isWrapper = typeof(PropertyWrapper<TValue>).IsInstanceOfType(container);
            if (!isWrapper)
            {
                AddToPath(property);
            }

            try
            {
                var path = GetCurrentPath();
                using (var references = VisitorContext.MakeVisitedReferencesScope(this, ref value, path))
                {
                    if (references.VisitedOnCurrentBranch)
                    {
                        VisitorContext.Parent.Add(new CircularReferenceElement<TValue>(VisitorContext.Root, property, value, path, references.GetReferencePath()));
                        return;
                    }

                    var inspector = default(IInspector);
                    if (!path.Empty || EnableRootCustomInspectors)
                    {
                        var visitor = new CustomInspectorVisitor<TValue>();
                        visitor.PropertyPath = path;
                        visitor.Root = VisitorContext.Root;
                        visitor.Property = property;
                        PropertyContainer.Visit(ref value, visitor);
                        inspector = visitor.Inspector;
                    }

                    var old = EnableRootCustomInspectors;
                    EnableRootCustomInspectors = true;
                    if (null != inspector)
                    {
                        var customInspector = new CustomInspectorElement(path, inspector, VisitorContext.Root);
                        VisitorContext.Parent.contentContainer.Add(customInspector);
                    }
                    else
                    {
                        RecurseProperty(ref value, property, path);
                    }

                    EnableRootCustomInspectors = old;
                }
            }
            finally
            {
                if (!isWrapper)
                {
                    RemoveFromPath(property);
                }
            }
        }

        public void RecurseProperty<TValue>(ref TValue value, IProperty property, PropertyPath path)
        {
            if (path.PartsCount > 0)
            {
                var foldout = GuiFactory.Foldout<TValue>(property, path, VisitorContext);
                using (VisitorContext.MakeParentScope(foldout))
                {
                    property.Visit(this, ref value);
                }
            }
            else
            {
                property.Visit(this, ref value);
            }
        }

        protected override void VisitList<TContainer, TList, TElement>(Property<TContainer, TList> property,
            ref TContainer container, ref TList value)
        {
            AddToPath(property);
            try
            {
                var path = GetCurrentPath();
                using (var references = VisitorContext.MakeVisitedReferencesScope(this, ref value, path))
                {
                    if (references.VisitedOnCurrentBranch)
                    {
                        VisitorContext.Parent.Add(new CircularReferenceElement<TList>(VisitorContext.Root, property, value, path, references.GetReferencePath()));
                        return;
                    }

                    var element = GuiFactory.Foldout<TList, TElement>(property, path, VisitorContext);
                    VisitorContext.Parent.contentContainer.Add(element);
                    using (VisitorContext.MakeParentScope(element))
                    {
                        element.Reload(property);
                    }
                }
            }
            finally
            {
                RemoveFromPath(property);
            }
        }

        protected override void VisitDictionary<TContainer, TDictionary, TKey, TValue>(
            Property<TContainer, TDictionary> property, ref TContainer container,
            ref TDictionary value)
        {
            AddToPath(property);
            try
            {
                var path = GetCurrentPath();
                using (var references = VisitorContext.MakeVisitedReferencesScope(this, ref value, path))
                {
                    if (references.VisitedOnCurrentBranch)
                    {
                        VisitorContext.Parent.Add(new CircularReferenceElement<TDictionary>(VisitorContext.Root, property, value, path, references.GetReferencePath()));
                        return;
                    }

                    var element =
                        GuiFactory.Foldout<TDictionary, TKey, TValue>(property, path, VisitorContext);
                    VisitorContext.Parent.contentContainer.Add(element);
                    using (VisitorContext.MakeParentScope(element))
                    {
                        element.Reload(property);
                    }
                }
            }
            finally
            {
                RemoveFromPath(property);
            }
        }

        protected override void VisitSet<TContainer, TSet, TValue>(Property<TContainer, TSet> property,
            ref TContainer container, ref TSet value)
        {
            AddToPath(property);
            try
            {
                var path = GetCurrentPath();
                using (var references = VisitorContext.MakeVisitedReferencesScope(this, ref value, path))
                {
                    if (references.VisitedOnCurrentBranch)
                    {
                        VisitorContext.Parent.Add(new CircularReferenceElement<TSet>(VisitorContext.Root, property, value, path, references.GetReferencePath()));
                        return;
                    }

                    var element =
                        GuiFactory.SetFoldout<TContainer, TSet, TValue>(property, path, VisitorContext);
                    VisitorContext.Parent.contentContainer.Add(element);
                    using (VisitorContext.MakeParentScope(element))
                    {
                        element.Reload(property);
                    }
                }
            }
            finally
            {
                RemoveFromPath(property);
            }
        }

        static bool IsFieldTypeSupported<TContainer, TValue>()
        {
            if (typeof(TValue) == typeof(object) && typeof(TContainer) != typeof(PropertyWrapper<TValue>))
                return false;

            if (Nullable.GetUnderlyingType(typeof(TValue)) != null)
                return false;

            return true;
        }

        static bool ShouldShowField<TContainer, TValue>(Property<TContainer, TValue> property)
        {
            return !property.HasAttribute<HideInInspector>();
        }
    }
}