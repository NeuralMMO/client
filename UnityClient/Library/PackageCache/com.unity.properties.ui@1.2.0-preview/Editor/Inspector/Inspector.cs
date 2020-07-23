using System;
using System.Collections.Generic;
using Unity.Properties.Internal;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Base class for defining a custom inspector for values of type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to inspect.</typeparam>
    public abstract class Inspector<T> : IInspector<T>
    {
        CustomInspectorElement IInspector.Parent { get; set; }

        InspectorContext<T> IInspector<T>.Context { get; set; }

        IInspector<T> Internal => this;

        /// <summary>
        /// Accessor to the value being inspected. 
        /// </summary>
        protected T Target
        {
            get => Internal.Context.Data;
            set
            {
                var context = Internal.Context;
                context.Data = value;
            }
        }

        /// <summary>
        /// Returns the property name of the current value.
        /// </summary>
        protected string Name => Internal.Context.Name;
        
        /// <summary>
        /// Returns the property path of the current value.
        /// </summary>
        public PropertyPath.Part Part => Internal.Context.Part;
        
        /// <summary>
        /// Returns the display name of the current value.
        /// </summary>
        protected string DisplayName => Internal.Context.DisplayName;
        
        /// <summary>
        /// Returns the tooltip of the current value.
        /// </summary>
        protected string Tooltip => Internal.Context.Tooltip;
        
        /// <summary>
        /// Returns <see langword="true"/> if the value field was tagged with the <see cref="UnityEngine.DelayedAttribute"/>.
        /// </summary>
        protected bool IsDelayed => Internal.Context.IsDelayed;
        
        /// <summary>
        /// Returns the full property path of the current target.
        /// </summary>
        public PropertyPath PropertyPath => Internal.Context.PropertyPath;
        
        PropertyPath BasePath => Internal.Context.BasePath;
        List<Attribute> Attributes => Internal.Context.Attributes;
        PropertyElement Root => Internal.Context.Root;

        /// <inheritdoc/>
        public bool HasAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            for (var i = 0; i < Attributes?.Count; i++)
            {
                if (Attributes[i] is TAttribute)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public TAttribute GetAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            for (var i = 0; i < Attributes?.Count; i++)
            {
                if (Attributes[i] is TAttribute typed)
                {
                    return typed;
                }
            }

            return default;
        }

        /// <inheritdoc/>
        public IEnumerable<TAttribute> GetAttributes<TAttribute>()
            where TAttribute : Attribute
        {
            for (var i = 0; i < Attributes?.Count; i++)
            {
                if (Attributes[i] is TAttribute typed)
                {
                    yield return typed;
                }
            }
        }

        /// <inheritdoc/>
        public virtual VisualElement Build()
        {
            return DoDefaultGui();
        }

        /// <inheritdoc/>
        public virtual void Update()
        {
        }
        
        /// <inheritdoc/>
        public bool IsPathValid(PropertyPath path)
        {
            var p = new PropertyPath();
            p.PushPath(BasePath);
            p.PushPath(path);
            return Root.IsPathValid(p);
        }
        
        /// <inheritdoc/>
        public Type Type => typeof(T);
        
        /// <summary>
        /// Allows to register data-binding on <see cref="BindableElement"/> when `binding-path` is set. 
        /// </summary>
        /// <param name="path">The base <see cref="PropertyPath"/> to use with the <paramref name="element"/>.</param>
        /// <param name="element">The root element we wish to bind.</param>
        void IInspector.RegisterBindings(PropertyPath path, VisualElement element)
        {
            Root.RegisterBindings(path, element);
        }
       
        /// <summary>
        /// Allows to revert to the default drawing handler for a specific field.  
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="name">The name of the field that needs to be drawn.</param>
        public void DoDefaultGui(VisualElement parent, string name)
        {
            var path = new PropertyPath();
            path.PushPath(PropertyPath);
            path.PushName(name);
            Root.VisitAtPath(path, parent);
        }

        /// <summary>
        /// Generates the default inspector.
        /// </summary>
        /// <returns>The parent <see cref="VisualElement"/> containing the generated inspector.</returns>
        protected VisualElement DoDefaultGui()
        {
            var visitor = new InspectorVisitor<T>(Root, Target) {EnableRootCustomInspectors = false};
            using (visitor.VisitorContext.MakeParentScope(Internal.Parent))
            {
                visitor.AddToPath(PropertyPath);
                if (PropertyPath.Empty)
                {
                    var wrapper = new PropertyWrapper<T>(Target);
                    PropertyContainer.Visit(ref wrapper, visitor);
                }
                else
                {
                    if (!Root.TryGetProperty(PropertyPath, out var property))
                        return Internal.Parent;
                    
                    var value = Target;
                    visitor.RecurseProperty(ref value, property, PropertyPath);
                }
            }

            return Internal.Parent;
        }

        /// <summary>
        /// Notifies the root element that a change occured on this value. This must be called when doing manual
        /// data binding. 
        /// </summary>
        /// <remarks>
        /// This is called automatically when the "binding=path" is set to a valid value/field combination.
        /// </remarks>
        protected void NotifyChanged()
        {
            Root.NotifyChanged(PropertyPath);
        }
    }
}