using System.Linq;
using Unity.Properties.Adapters;
using Unity.Properties.Editor;
using Unity.Properties.Internal;
using UnityEngine;

namespace Unity.Properties.UI.Internal
{
    class NullAdapter<T> : InspectorAdapter<T>, IVisit
    {
        public NullAdapter(InspectorVisitor<T> visitor) : base(visitor)
        {
        }
        
        public VisitStatus Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container,
            ref TValue value)
        {
            if (!RuntimeTypeInfoCache<TValue>.CanBeNull || null != value)
                return VisitStatus.Unhandled;

            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TValue)))
                return VisitStatus.Unhandled;
            
            if (!property.IsReadOnly && property.HasAttribute<CreateInstanceOnInspectionAttribute>() && !(property is ICollectionElementProperty))
            {
                var attribute = property.GetAttribute<CreateInstanceOnInspectionAttribute>();
                if (null == attribute.Type)
                {
                    if (TypeConstruction.CanBeConstructed<TValue>())
                    {
                        value = TypeConstruction.Construct<TValue>();
                        property.SetValue(ref container, value);
                        return VisitStatus.Unhandled;
                    }

                    Debug.LogWarning(PropertyChecks.GetNotConstructableWarningMessage(typeof(TValue)));
                }
                else
                {
                    var isAssignable = typeof(TValue).IsAssignableFrom(attribute.Type);
                    var isConstructable = TypeConstruction.GetAllConstructableTypes(typeof(TValue))
                        .Contains(attribute.Type);
                    if (isAssignable && isConstructable)
                    {
                        value = TypeConstruction.Construct<TValue>(attribute.Type);
                        property.SetValue(ref container, value);
                        return VisitStatus.Unhandled;
                    }

                    Debug.LogWarning(isAssignable
                        ? PropertyChecks.GetNotConstructableWarningMessage(attribute.Type)
                        : PropertyChecks.GetNotAssignableWarningMessage(attribute.Type, typeof(TValue)));
                }
            }

            Visitor.AddToPath(property);
            try
            {
                var path = Visitor.GetCurrentPath();
                var element = new NullElement<TValue>(VisitorContext.Root, property, path);
                VisitorContext.Parent.contentContainer.Add(element);
            }
            finally
            {
                Visitor.RemoveFromPath(property);
            }
            return VisitStatus.Stop;
        }
    }
}