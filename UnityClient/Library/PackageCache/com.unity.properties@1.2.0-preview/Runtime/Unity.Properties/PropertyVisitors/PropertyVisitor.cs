using System.Collections.Generic;
using Unity.Properties.Adapters;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// Enum used to describe how visitation should be handled.
    /// </summary>
    public enum VisitStatus
    {
        /// <summary>
        /// Visitation is not being handled. Scopes should not be opened and control should fall the the next available handler.
        /// </summary>
        Unhandled,
        
        /// <summary>
        /// Visitation is being handled. Use the default behaviour of nesting in to containers and visiting list elements.
        /// </summary>
        Handled,
        
        /// <summary>
        /// Visitation is being handled. Override the default pattern of nesting in to containers or visiting list elements.
        /// </summary>
        Stop
    }
    
    /// <summary>
    /// The base interface representing an adapter.
    /// </summary>
    public interface IPropertyVisitorAdapter
    {

    }
    
    /// <summary>
    /// Base class for implementing algorithms using properties. This is an abstract class.
    /// </summary>
    public abstract class PropertyVisitor : 
        IPropertyBagVisitor, 
        IListPropertyBagVisitor,
        IDictionaryPropertyBagVisitor,
        IPropertyVisitor, 
        ICollectionPropertyVisitor,
        IListPropertyVisitor,
        ISetPropertyVisitor,
        IDictionaryPropertyVisitor
    {
        readonly List<IPropertyVisitorAdapter> m_Adapters = new List<IPropertyVisitorAdapter>();
        
        /// <summary>
        /// Adds an adapter to the visitor.
        /// </summary>
        /// <param name="adapter">The adapter to add.</param>
        public void AddAdapter(IPropertyVisitorAdapter adapter) => m_Adapters.Add(adapter);
        
        /// <summary>
        /// Removes an adapter from the visitor.
        /// </summary>
        /// <param name="adapter">The adapter to remove.</param>
        public void RemoveAdapter(IPropertyVisitorAdapter adapter) => m_Adapters.Remove(adapter);

        /// <inheritdoc/>
        void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
        {
            if (properties is IPropertyList<TContainer> propertyList)
            {
                // no boxing
                foreach (var property in propertyList.GetProperties(ref container))
                {
                    ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
                }
                return;
            }
            
            // boxing
            foreach (var property in properties.GetProperties(ref container))
            {
                ((IPropertyAccept<TContainer>) property).Accept(this, ref container);
            }
        }

        /// <inheritdoc/>
        void IListPropertyBagVisitor.Visit<TList, TElement>(IListPropertyBag<TList, TElement> properties, ref TList container)
        {
            foreach (var property in properties.GetProperties(ref container))
            {
                ((IPropertyAccept<TList>) property).Accept(this, ref container);
            }
        }

        /// <inheritdoc/>
        void IDictionaryPropertyBagVisitor.Visit<TDictionary, TKey, TValue>(IDictionaryPropertyBag<TDictionary, TKey, TValue> properties, ref TDictionary container)
        {
            foreach (var property in properties.GetProperties(ref container))
            {
                ((IPropertyAccept<TDictionary>) property).Accept(this, ref container);
            }
        }

        /// <inheritdoc/>
        void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
        {
            var value = property.GetValue(ref container);

            // Give adapters a chance to exclude.
            if (m_Adapters.Count > 0 && m_Adapters.GetEnumerator().TryExclude(property, ref container, ref value) == VisitStatus.Stop)
                return;

            // Give the explicit overrides a chance to exclude.
            if (IsExcluded(property, ref container, ref value))
                return;
            
            // Give user defined adapters a chance to handle this value type.
            if (m_Adapters.Count > 0 && m_Adapters.GetEnumerator().TryVisit(property, ref container, ref value) == VisitStatus.Stop)
                return;

            if (PropertyBagStore.TryGetPropertyBagForValue(ref value, out var valuePropertyBag))
            {
                switch (valuePropertyBag)
                {
                    case IDictionaryPropertyAccept<TValue> accept:
                        accept.Accept(this, property, ref container, ref value);
                        return;
                    case IListPropertyAccept<TValue> accept:
                        accept.Accept(this, property, ref container, ref value);
                        return;
                    case ISetPropertyAccept<TValue> accept:
                        accept.Accept(this, property, ref container, ref value);
                        return;
                    case ICollectionPropertyAccept<TValue> accept:
                        accept.Accept(this, property, ref container, ref value);
                        return;
                }
            }

            VisitProperty(property, ref container, ref value);
            
            if (!property.IsReadOnly) property.SetValue(ref container, value);
        }
        
        /// <inheritdoc/>
        void ICollectionPropertyVisitor.Visit<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection collection)
        {
            VisitCollection<TContainer, TCollection, TElement>(property, ref container, ref collection);
        }
        
        /// <inheritdoc/>
        void IListPropertyVisitor.Visit<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList list)
        {
            VisitList<TContainer, TList, TElement>(property, ref container, ref list);
        }

        /// <inheritdoc/>
        void ISetPropertyVisitor.Visit<TContainer, TSet, TElement>(Property<TContainer, TSet> property, ref TContainer container, ref TSet set)
        {
            VisitSet<TContainer, TSet, TElement>(property, ref container, ref set);
        }
        
        /// <inheritdoc/>
        void IDictionaryPropertyVisitor.Visit<TContainer, TDictionary, TKey, TValue>(Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary dictionary)
        {
            VisitDictionary<TContainer, TDictionary, TKey, TValue>(property, ref container, ref dictionary);
        }

        /// <summary>
        /// Called before visiting each property to determine if the property should be visited.
        /// </summary>
        /// <remarks>
        /// This method is called after all <see cref="Unity.Properties.Adapters.IExclude{TContainer}"/> have had a chance to run.
        /// </remarks>
        /// <param name="property">The property providing access to the data.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The value being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <returns><see langword="true"/> if the property should be skipped; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            return false;
        }

        /// <summary>
        /// Called when visiting any leaf property with no specialized handling.
        /// </summary>
        /// <param name="property">The property providing access to the value.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The value being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        protected virtual void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
            property.Visit(this, ref value);
        }

        /// <summary>
        /// Called when visiting any non-specialized collection property.
        /// </summary>
        /// <remarks>
        /// When visiting a specialized collection the appropriate method will be called.
        /// * <seealso cref="VisitList{TContainer,TList,TElement}"/>
        /// </remarks>
        /// <param name="property">The property providing access to the collection.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The collection being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TCollection">The collection type.</typeparam>
        /// <typeparam name="TElement">The element type.</typeparam>
        protected virtual void VisitCollection<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection value)
            where TCollection : ICollection<TElement>
        {
            VisitProperty(property, ref container, ref value);
        }
        
        /// <summary>
        /// Called when visiting a specialized list property.
        /// </summary>
        /// <param name="property">The property providing access to the list.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The list being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TList">The list type.</typeparam>
        /// <typeparam name="TElement">The element type.</typeparam>
        protected virtual void VisitList<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList value)
            where TList : IList<TElement>
        {
            VisitCollection<TContainer, TList, TElement>(property, ref container, ref value);
        }
        
        /// <summary>
        /// Called when visiting a specialized set property.
        /// </summary>
        /// <param name="property">The property providing access to the list.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The list being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TSet">The set type.</typeparam>
        /// <typeparam name="TValue">The element type.</typeparam>
        protected virtual void VisitSet<TContainer, TSet, TValue>(Property<TContainer, TSet> property, ref TContainer container, ref TSet value)
            where TSet : ISet<TValue>
        {
            VisitCollection<TContainer, TSet, TValue>(property, ref container, ref value);
        }
        
        /// <summary>
        /// Called when visiting a specialized dictionary property.
        /// </summary>
        /// <param name="property">The property providing access to the dictionary.</param>
        /// <param name="container">The container being visited.</param>
        /// <param name="value">The dictionary being visited.</param>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TDictionary">The dictionary type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        protected virtual void VisitDictionary<TContainer, TDictionary, TKey, TValue>(Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary value)
            where TDictionary : IDictionary<TKey, TValue>
        {
            VisitCollection<TContainer, TDictionary, KeyValuePair<TKey, TValue>>(property, ref container, ref value);
        }

        internal static class AOT
        {
            internal static void RegisterType<TContainer, TValue>(Property<TContainer, TValue> property = default, TContainer container = default, TValue value = default)
            {
                ((PropertyVisitor) default).IsExcluded(property, ref container, ref value);
                ((PropertyVisitor) default).VisitProperty(property, ref container, ref value);
            }
            
            internal static void RegisterCollection<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property = default, TContainer container = default, TCollection value = default)
                where TCollection : ICollection<TElement>
            {
                ((PropertyVisitor) default).VisitCollection<TContainer, TCollection, TElement>(property, ref container, ref value);
            }
            
            internal static void RegisterList<TContainer, TList, TElement>(Property<TContainer, TList> property = default, TContainer container = default, TList value = default)
                where TList : IList<TElement>
            {
                ((PropertyVisitor) default).VisitList<TContainer, TList, TElement>(property, ref container, ref value);
            }
            
            internal static void RegisterSet<TContainer, TSet, TValue>(Property<TContainer, TSet> property = default, TContainer container = default, TSet value = default)
                where TSet : ISet<TValue>
            {
                ((PropertyVisitor) default).VisitSet<TContainer, TSet, TValue>(property, ref container, ref value);
            }
            
            internal static void RegisterDictionary<TContainer, TDictionary, TKey, TValue>(Property<TContainer, TDictionary> property = default, TContainer container = default, TDictionary value = default)
                where TDictionary : IDictionary<TKey, TValue>
            {
                ((PropertyVisitor) default).VisitDictionary<TContainer, TDictionary, TKey, TValue>(property, ref container, ref value);
                RegisterType<KeyValuePair<TKey, TValue>, TKey>();
                RegisterType<KeyValuePair<TKey, TValue>, TValue>();
            }
        }
    }
}