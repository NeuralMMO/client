using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Properties.UI.Internal;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Makes an element that can generate a UI hierarchy for a given target.
    /// </summary>
    public sealed class PropertyElement : BindableElement
    {
        /// <summary>
        ///   <para>Instantiates a PropertyElement using the data read from a UXML file.</para>
        /// </summary>
        class PropertyElementFactory : UnityEngine.UIElements.UxmlFactory<PropertyElement, PropertyElementTraits>
        {
        }

        /// <summary>
        ///   <para>Defines UxmlTraits for the PropertyElement.</para>
        /// </summary>
        class PropertyElementTraits : VisualElement.UxmlTraits
        {
        }
        
        /// <summary>
        /// Handler to react to changes.
        /// </summary>
        /// <param name="element">The element under which the change is detected.</param>
        /// <param name="path">The property path of the change</param>
        /// <remarks>
        /// The property path can be empty for the root object or when the path to the change is not provided.
        /// </remarks>
        public delegate void ChangeHandler(PropertyElement element, PropertyPath path);

        /// <summary>
        /// Handler to filter visitation based on a field's attribute.
        /// </summary>
        /// <param name="attributes">The attributes of the field.</param>
        /// <returns><see langword="true"/>if the field should be shown; <see langword="false"/> otherwise.</returns>
        public delegate bool AttributeFilterHandler(IEnumerable<Attribute> attributes);
        
        /// <summary>
        /// Register to this event to be notified when a change is detected.
        /// </summary>
        public event ChangeHandler OnChanged = delegate { };

        internal AttributeFilterHandler m_AttributeFilter;
        
        IBindingTarget m_BindingTarget;
        
        /// <summary>
        /// Constructs an instance of <see cref="PropertyElement"/>.
        /// </summary>
        public PropertyElement()
        {
            Resources.Templates.Common.AddStyles(this);
            AddToClassList(UssClasses.Variables);
        }

        /// <summary>
        /// Tries to get the target of the <see cref="PropertyElement"/> as an instance of type <see cref="T"/>.
        /// </summary>
        /// <param name="target">The target instance.</param>
        /// <typeparam name="T">The type of the target.</typeparam>
        /// <returns><see langword="true"/> if target was of the correct type; <see langword="false"/> otherwise.</returns>
        public bool TryGetTarget<T>(out T target)
        {
            if (null == m_BindingTarget)
            {
                target = default;
                return false;
            }
            
            if (m_BindingTarget is IBindingTarget<T> typedTarget)
            {
                target = typedTarget.Target;
                return true;
            }

            return m_BindingTarget.TryGetTarget(out target);
        }

        /// <summary>
        /// Gets the target of the <see cref="PropertyElement"/> as <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the target.</typeparam>
        /// <returns>The instance of the target.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no target has been set.</exception>
        /// <exception cref="InvalidCastException">Thrown when the target is not of type <see cref="T"/>.</exception>
        public T GetTarget<T>()
        {
            if (TryGetTarget(out T target))
            {
                return target;
            }

            if (null == m_BindingTarget)
            {
                throw new InvalidOperationException(
                    $"{nameof(PropertyElement)}: Could not get value of type `{typeof(T).Name}`, as no target has been set.");
            }
            
            throw new InvalidCastException($"{nameof(PropertyElement)}: Could not get value of type `{typeof(T).Name}`. Actual value is of type`{m_BindingTarget.TargetType.Name}`");
        }

        /// <summary>
        /// Sets the current target.
        /// </summary>
        /// <remarks>
        /// This will clear current hierarchy and regenerate a new one.
        /// </remarks>
        /// <param name="target">The target to set.</param>
        /// <typeparam name="T">The type of the target.</typeparam>
        public void SetTarget<T>(T target)
        {
            if (TryGetTarget(out T current) && current.Equals(target))
            {
                // Nothing to do..
                return;
            }

            if (null != m_BindingTarget)
            {
                var type = typeof(T);
                if (typeof(T).IsClass && null != target)
                {
                    type = target.GetType();
                }

                if (type == m_BindingTarget.TargetType)
                {
                    (m_BindingTarget as BindingTarget<T>).Target = target;
                    return;
                }
            }

            m_BindingTarget?.Release();

            if (typeof(T).IsClass && null == target)
            {
                m_BindingTarget = null;
                return;
            }
            
            m_BindingTarget = new BindingTarget<T>(this, target);
            name = target.GetType().Name;
            m_BindingTarget.GenerateHierarchy();
        }

        /// <summary>
        /// Clears the current target and removes all child elements from this element's contentContainer. 
        /// </summary>
        public void ClearTarget()
        {
            m_BindingTarget?.Release();
            m_BindingTarget = null;
        }

        /// <summary>
        /// Clears the current children and re-generates them.
        /// </summary>
        public void ForceReload()
        {
            m_BindingTarget?.Release();
            m_BindingTarget?.GenerateHierarchy();
        }
        
        /// <summary>
        /// Allows to filter the hierarchy generation based on the field's attributes.  
        /// </summary>
        /// <param name="filter">The filter method to apply.</param>
        public void SetAttributeFilter(AttributeFilterHandler filter)
        {
            if (m_AttributeFilter == filter)
                return;
            m_AttributeFilter = filter;
            ForceReload();
        }

        /// <summary>
        /// Sets the value of type <see cref="TValue"/> at the given path.
        /// </summary>
        /// <param name="path">The property path to the value.</param>
        /// <param name="value">the value we want to set.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <exception cref="InvalidOperationException">Thrown when no target has been set.</exception>
        public void SetValue<TValue>(PropertyPath path, TValue value)
        {
            if (null == m_BindingTarget)
            {
                throw new InvalidOperationException($"{nameof(PropertyElement)}: Could not set value of type `{typeof(TValue).Name}`, as no target was set.");
            }
            m_BindingTarget.SetAtPath(path, value);
        }
        
        /// <summary>
        /// Tries to set the value of type <see cref="TValue"/> at the given path.
        /// </summary>
        /// <param name="path">The property path to the value.</param>
        /// <param name="value">the value we want to set.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The value that was effectively set.</returns>
        public bool TrySetValue<TValue>(PropertyPath path, TValue value)
        {
            return m_BindingTarget?.TrySetAtPath(path, value) ?? false;
        }
        
        /// <summary>
        /// Gets the value of type <see cref="TValue"/> at the given path.
        /// </summary>
        /// <param name="path">The property path to the value.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns>The value at path.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no target has been set.</exception>
        public TValue GetValue<TValue>(PropertyPath path)
        {
            if (null == m_BindingTarget)
            {
                throw new InvalidOperationException($"{nameof(PropertyElement)}: Could not set value of type `{typeof(TValue).Name}`, as no target was set.");
            }
            return m_BindingTarget.GetAtPath<TValue>(path);
        }

        /// <summary>
        /// Tries to get the value of type <see cref="TValue"/> at the given path.
        /// </summary>
        /// <param name="path">The property path to the value.</param>
        /// <param name="value">The value at path.</param>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the getting the value was successful.</returns>
        public bool TryGetValue<TValue>(PropertyPath path, out TValue value)
        {
            if (null != m_BindingTarget && m_BindingTarget.TryGetAtPath(path, out value))
            {
                return true;
            }

            value = default;
            return false;
        }
        
        /// <summary>
        /// Returns <see langword="true"/> if the given path is valid from the target.
        /// </summary>
        /// <param name="path">the property path.</param>
        /// <returns><see langword="true"/> if the path is valid.</returns>
        public bool IsPathValid(PropertyPath path)
        {
            return m_BindingTarget?.IsPathValid(path) ?? false;
        }
        
        internal void NotifyChanged(PropertyPath path)
        {
            OnChanged(this, path);
        }
        
        internal void VisitAtPath(PropertyPath path, VisualElement localRoot)
        {
            m_BindingTarget?.VisitAtPath(path, localRoot);
        }

        internal void RegisterBindings(PropertyPath path, VisualElement element)
        {
            m_BindingTarget?.RegisterBindings(path, element);
        }

        internal bool TryGetProperty(PropertyPath path, out IProperty property)
        {
            return m_BindingTarget.TryGetProperty(path, out property);
        }

        internal Type GetTargetType()
        {
            return m_BindingTarget?.TargetType;
        }

        internal IInspectorVisitor GetVisitor()
        {
            return m_BindingTarget.Visitor;
        }

        internal void SwapWithInstance<TValue>(PropertyPath propertyPath, VisualElement current, TValue value)
        {
            var previousParent = current.parent;
            SetValue(propertyPath, value);
            NotifyChanged(propertyPath);
            var element = new VisualElement();
            VisitAtPath(propertyPath, element);
            var index = previousParent.IndexOf(current);
            for (var i = 0; i < element.childCount; ++i)
            {
                previousParent.Insert(index + i, element[i]);
            }

            if (typeof(TValue) ==typeof(string) && previousParent.childCount > index)
                previousParent[index]?.Q(className: UssClasses.Unity.BaseTextFieldInput)?.Focus();
            
            current.RemoveFromHierarchy();
            previousParent.GetFirstAncestorOfType<ICustomStyleApplier>()?.ApplyStyleAtPath(propertyPath);    
        }

        internal void StartHighlightAtPath(PropertyPath path)
        {
            foreach (var element in this.ChildrenOfType<IContextElement>().Where(c => c.Path.Equals(path)).OfType<Foldout>())
            {
                element.Q<Toggle>().AddToClassList(UssClasses.Highlight);
            }
        }
        
        internal void StopHighlightAtPath(PropertyPath path)
        {
            foreach (var element in this.ChildrenOfType<IContextElement>().Where(c => c.Path.Equals(path)).OfType<Foldout>())
            {
                element.Q<Toggle>().RemoveFromClassList(UssClasses.Highlight);
            }
        }
    }
}