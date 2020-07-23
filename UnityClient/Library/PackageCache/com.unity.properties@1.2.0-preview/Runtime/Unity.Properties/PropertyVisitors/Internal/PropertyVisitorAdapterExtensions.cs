using System.Collections.Generic;
using Unity.Properties.Adapters;

namespace Unity.Properties.Internal
{
    static class PropertyVisitorAdapterExtensions
    {
        internal static VisitStatus TryExclude<TContainer, TValue>(this List<IPropertyVisitorAdapter>.Enumerator adapters, Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            while (adapters.MoveNext())
            {
                var adapter = adapters.Current;
                switch (adapter)
                {
                    case IExclude<TContainer, TValue> typed
                        when typed.IsExcluded(property, ref container, ref value):
                        return VisitStatus.Stop;
                    case Adapters.Contravariant.IExclude<TContainer, TValue> typed
                        when typed.IsExcluded(property, ref container, value):
                        return VisitStatus.Stop;
                    case IExclude<TValue> typed
                        when typed.IsExcluded(property, ref container, ref value):
                        return VisitStatus.Stop;
                    case Adapters.Contravariant.IExclude<TValue> typed
                        when typed.IsExcluded(property, ref container, value):
                        return VisitStatus.Stop;
                    case IExclude typed
                        when typed.IsExcluded(property, ref container, ref value):
                        return VisitStatus.Stop;
                }
            }

            return VisitStatus.Handled;
        }

        internal static VisitStatus TryVisit<TContainer, TValue>(this List<IPropertyVisitorAdapter>.Enumerator adapters, Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            var status = VisitStatus.Unhandled;

            while (adapters.MoveNext())
            {
                var adapter = adapters.Current;
                switch (adapter)
                {
                    case IVisit<TContainer, TValue> typed
                        when (status = typed.Visit(property, ref container, ref value)) != VisitStatus.Unhandled:
                        if (!property.IsReadOnly) property.SetValue(ref container, value);
                        return status;
                    case Adapters.Contravariant.IVisit<TContainer, TValue> typed
                        when (status = typed.Visit(property, ref container, value)) != VisitStatus.Unhandled:
                        return status;
                    case IVisit<TValue> typed
                        when (status = typed.Visit(property, ref container, ref value)) != VisitStatus.Unhandled:
                        if (!property.IsReadOnly) property.SetValue(ref container, value);
                        return status;
                    case Adapters.Contravariant.IVisit<TValue> typed
                        when (status = typed.Visit(property, ref container, value)) != VisitStatus.Unhandled:
                        return status;
                    case IVisit typed
                        when (status = typed.Visit(property, ref container, ref value)) != VisitStatus.Unhandled:
                        if (!property.IsReadOnly) property.SetValue(ref container, value);
                        return status;
                }
            }

            return status;
        }
    }
}