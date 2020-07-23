using System.Collections.Generic;
using Unity.Properties.Internal;
using UnityEngine.Scripting;

namespace Unity.Properties
{
    /// <summary>
    /// Helper class to preserve generic methods for AOT platforms.
    /// </summary>
    public static class AOT
    {
        /// <summary>
        /// Helper class to preserve property bag code paths.
        /// </summary>
        /// <typeparam name="TContainer">The container type.</typeparam>
        public static class PropertyBagGenerator<TContainer>
        {
            /// <summary>
            /// Invoke this method to have all generic property code paths generated for the given <typeparamref cref="TContainer"/>.
            /// </summary>
            public static void Preserve() { }
            
            [Preserve]
            static void Generate()
            {
                ConcreteTypeVisitor.AOT.RegisterType<TContainer>();
            }
        }
        
        /// <summary>
        /// Helper class to preserve property property code paths.
        /// </summary>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        [Preserve]
        public static class PropertyGenerator<TContainer, TValue>
        {
            /// <summary>
            /// Invoke this method to have all generic property code paths generated for the given <typeparamref cref="TContainer"/> and <typeparamref name="TValue"/> combination.
            /// </summary>
            public static void Preserve() { }
            
            [Preserve]
            static void Generate()
            {
                PropertyVisitor.AOT.RegisterType<TContainer, TValue>();
                PathVisitor.AOT.RegisterType(default(Property<TContainer, TValue>));
            }
        }
        
        /// <summary>
        /// Helper class to preserve list property code paths.
        /// </summary>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TList">The list type.</typeparam>
        /// <typeparam name="TElement">The element type.</typeparam>
        [Preserve]
        public static class ListPropertyGenerator<TContainer, TList, TElement> where TList : IList<TElement>
        {
            /// <summary>
            /// Invoke this method to have all generic <see cref="IList{TElement}"/> property code paths generated.
            /// </summary>
            /// <remarks>
            /// This method is used to generate all generic combinations of <typeparamref cref="TContainer"/>, <typeparamref cref="TList"/> and <typeparamref cref="TElement"/>.
            /// </remarks>
            public static void Preserve() { }
            
            [Preserve]
            static void Generate()
            {
                PropertyGenerator<TContainer, TList>.Preserve();
                
                PropertyVisitor.AOT.RegisterCollection<TContainer, TList, TElement>();
                PropertyVisitor.AOT.RegisterList<TContainer, TList, TElement>();
            
                var container = default(TContainer);
                var list = default(TList);

#pragma warning disable 1720
                // ReSharper disable once PossibleNullReferenceException
                default(IListPropertyAccept<TList>).Accept(default, default, ref container, ref list);
                    
                // ReSharper disable once PossibleNullReferenceException
                default(IListPropertyVisitor).Visit<TContainer, TList, TElement>(default, ref container, ref list);
#pragma warning restore 1720
            }
        }
        
        /// <summary>
        /// Helper class to preserve set property code paths.
        /// </summary>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TSet">The set type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        [Preserve]
        public static class SetPropertyGenerator<TContainer, TSet, TValue> where TSet : ISet<TValue>
        {
            /// <summary>
            /// Invoke this method to have all generic <see cref="ISet{TValue}"/> property code paths generated.
            /// </summary>
            /// <remarks>
            /// This method is used to generate all generic combinations of <typeparamref cref="TContainer"/>, <typeparamref cref="TSet"/> and <typeparamref cref="TValue"/>.
            /// </remarks>
            public static void Preserve() { }
            
            [Preserve]
            static void Generate()
            {
                PropertyGenerator<TContainer, TSet>.Preserve();
                
                PropertyVisitor.AOT.RegisterCollection<TContainer, TSet, TValue>();
                PropertyVisitor.AOT.RegisterSet<TContainer, TSet, TValue>();
            
                var container = default(TContainer);
                var list = default(TSet);

#pragma warning disable 1720
                // ReSharper disable once PossibleNullReferenceException
                default(ISetPropertyAccept<TSet>).Accept(default, default, ref container, ref list);
                    
                // ReSharper disable once PossibleNullReferenceException
                default(ISetPropertyVisitor).Visit<TContainer, TSet, TValue>(default, ref container, ref list);
#pragma warning restore 1720
            }
        }
        
        /// <summary>
        /// Helper class to preserve dictionary property code paths.
        /// </summary>
        /// <typeparam name="TContainer">The container type.</typeparam>
        /// <typeparam name="TDictionary">The list type.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        [Preserve]
        public static class DictionaryPropertyGenerator<TContainer, TDictionary, TKey, TValue> where TDictionary : IDictionary<TKey, TValue>
        {
            /// <summary>
            /// Invoke this method to have all generic <see cref="IDictionary{TKey,TValue}"/> property code paths generated.
            /// </summary>
            /// <remarks>
            /// This method is used to generate all generic combinations of <typeparamref cref="TContainer"/>, <typeparamref cref="TDictionary"/>, <typeparamref cref="TKey"/> and <typeparamref cref="TValue"/>.
            /// </remarks>
            public static void Preserve() { }
            
            [Preserve]
            static void Generate()
            {
                PropertyGenerator<TContainer, TDictionary>.Preserve();
                
                PropertyVisitor.AOT.RegisterCollection<TContainer, TDictionary, KeyValuePair<TKey, TValue>>();
                PropertyVisitor.AOT.RegisterDictionary<TContainer, TDictionary, TKey, TValue>();
            
                var container = default(TContainer);
                var list = default(TDictionary);

#pragma warning disable 1720
                // ReSharper disable once PossibleNullReferenceException
                default(IDictionaryPropertyAccept<TDictionary>).Accept(default, default, ref container, ref list);
                    
                // ReSharper disable once PossibleNullReferenceException
                default(IDictionaryPropertyVisitor).Visit<TContainer, TDictionary, TKey, TValue>(default, ref container, ref list);
#pragma warning restore 1720
            }
        }
    }
}