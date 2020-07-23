using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties;
using Unity.Properties.Internal;

namespace Unity.Entities
{
    public readonly struct EntityContainer
    {
        static EntityContainer()
        {
            PropertyBagStore.AddPropertyBag(new EntityContainerPropertyBag());
        }

        public readonly EntityManager EntityManager;
        public readonly Entity Entity;
        public readonly bool IsReadOnly;

        public int GetComponentCount() => EntityManager.GetComponentCount(Entity);

        public EntityContainer(EntityManager entityManager, Entity entity, bool readOnly = true)
        {
            EntityManager = entityManager;
            Entity = entity;
            IsReadOnly = readOnly;
        }
    }

    class EntityContainerPropertyBag : PropertyBag<EntityContainer>, IPropertyNameable<EntityContainer>, IPropertyIndexable<EntityContainer>
    {
        interface IComponentProperty : IProperty<EntityContainer>
        {
        }

        class ComponentPropertyConstructor : IContainerTypeVisitor
        {
            public int TypeIndex { private get; set; }
            public bool IsReadOnly { private get; set; }

            public IComponentProperty Property { get; private set; }

            void IContainerTypeVisitor.Visit<TComponent>()
            {
                IComponentProperty CreateInstance(Type propertyType)
                    => (IComponentProperty)Activator.CreateInstance(propertyType.MakeGenericType(typeof(TComponent)), TypeIndex, IsReadOnly);

                var type = typeof(TComponent);
                if (typeof(IComponentData).IsAssignableFrom(type))
                {
                    if (TypeManager.IsChunkComponent(TypeIndex))
#if !UNITY_DISABLE_MANAGED_COMPONENTS
                        Property = CreateInstance(type.IsValueType
                        ? typeof(StructChunkComponentProperty<>)
                        : typeof(ClassChunkComponentProperty<>));
#else
                        Property = CreateInstance(typeof(StructChunkComponentProperty<>));
#endif
#if !UNITY_DISABLE_MANAGED_COMPONENTS
                    else if (TypeManager.IsManagedComponent(TypeIndex))
                        Property = CreateInstance(typeof(ClassComponentProperty<>));
#endif
                    else
                        Property = CreateInstance(typeof(StructComponentProperty<>));
                }
                else if (typeof(ISharedComponentData).IsAssignableFrom(type))
                    Property = CreateInstance(typeof(SharedComponentProperty<>));
                else if (typeof(IBufferElementData).IsAssignableFrom(type))
                    Property = CreateInstance(typeof(DynamicBufferProperty<>));
                else if (TypeManager.IsManagedComponent(TypeIndex))
                    Property = CreateInstance(typeof(ManagedComponentProperty<>));
                else
                    throw new InvalidOperationException();
            }
        }

        abstract class ComponentProperty<TComponent> : Property<EntityContainer, TComponent>, IComponentProperty
        {
            public override string Name => GetTypeName(typeof(TComponent));
            public override bool IsReadOnly { get; }
            public int TypeIndex { get; }

            public ComponentProperty(int typeIndex, bool isReadOnly)
            {
                TypeIndex = typeIndex;
                IsReadOnly = isReadOnly;
            }

            public override TComponent GetValue(ref EntityContainer container)
            {
                return IsZeroSize ? default : DoGetValue(ref container);
            }

            public override void SetValue(ref EntityContainer container, TComponent value)
            {
                if (IsReadOnly) throw new NotSupportedException("Property is ReadOnly");
                DoSetValue(ref container, value);
            }

            protected abstract TComponent DoGetValue(ref EntityContainer container);
            protected abstract void DoSetValue(ref EntityContainer container, TComponent value);
            protected abstract bool IsZeroSize { get; }
        }

        class SharedComponentProperty<TComponent> : ComponentProperty<TComponent> where TComponent : struct, ISharedComponentData
        {
            protected override bool IsZeroSize { get; } = false;

            public SharedComponentProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override TComponent DoGetValue(ref EntityContainer container)
            {
                return container.EntityManager.GetSharedComponentData<TComponent>(container.Entity);
            }

            protected override void DoSetValue(ref EntityContainer container, TComponent value)
            {
                throw new NotImplementedException();
            }
        }
        class StructComponentProperty<TComponent> : ComponentProperty<TComponent>
            where TComponent : struct, IComponentData
        {
            protected override bool IsZeroSize { get; } = TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TComponent>());

            public StructComponentProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override TComponent DoGetValue(ref EntityContainer container)
            {
                return container.EntityManager.GetComponentData<TComponent>(container.Entity);
            }

            protected override void DoSetValue(ref EntityContainer container, TComponent value)
            {
                throw new NotImplementedException();
            }
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        class ClassComponentProperty<TComponent> : ComponentProperty<TComponent>
            where TComponent : class, IComponentData
        {
            protected override bool IsZeroSize { get; } = TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TComponent>());

            public ClassComponentProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override TComponent DoGetValue(ref EntityContainer container)
            {
                return container.EntityManager.GetComponentData<TComponent>(container.Entity);
            }

            protected override void DoSetValue(ref EntityContainer container, TComponent value)
            {
                throw new NotImplementedException();
            }
        }
#endif

        class ManagedComponentProperty<TComponent> : ComponentProperty<TComponent>
            where TComponent : UnityEngine.Component
        {
            protected override bool IsZeroSize { get; } = TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TComponent>());

            public ManagedComponentProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override TComponent DoGetValue(ref EntityContainer container)
            {
                return container.EntityManager.GetComponentObject<TComponent>(container.Entity);
            }

            protected override void DoSetValue(ref EntityContainer container, TComponent value)
            {
                throw new NotImplementedException();
            }
        }

        class StructChunkComponentProperty<TComponent> : ComponentProperty<TComponent>, IComponentProperty
            where TComponent : struct, IComponentData
        {
            protected override bool IsZeroSize { get; } = TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TComponent>());

            public StructChunkComponentProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override TComponent DoGetValue(ref EntityContainer container)
            {
                return container.EntityManager.GetChunkComponentData<TComponent>(container.Entity);
            }

            protected override void DoSetValue(ref EntityContainer container, TComponent value)
            {
                throw new NotImplementedException();
            }
        }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
        class ClassChunkComponentProperty<TComponent> : ComponentProperty<TComponent>, IComponentProperty
            where TComponent : class, IComponentData
        {
            protected override bool IsZeroSize { get; } = TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TComponent>());

            public ClassChunkComponentProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override TComponent DoGetValue(ref EntityContainer container)
            {
                return container.EntityManager.GetChunkComponentData<TComponent>(container.Entity);
            }

            protected override void DoSetValue(ref EntityContainer container, TComponent value)
            {
                throw new NotImplementedException();
            }
        }
#endif

        unsafe class DynamicBufferProperty<TElement> : ComponentProperty<DynamicBufferContainer<TElement>>
            where TElement : struct, IBufferElementData
        {
            public override string Name => GetTypeName(typeof(TElement));
            protected override bool IsZeroSize { get; } = TypeManager.IsZeroSized(TypeManager.GetTypeIndex<TElement>());

            public DynamicBufferProperty(int typeIndex, bool isReadOnly) : base(typeIndex, isReadOnly)
            {
            }

            protected override DynamicBufferContainer<TElement> DoGetValue(ref EntityContainer container)
            {
                if (IsReadOnly)
                {
                    return new DynamicBufferContainer<TElement>((BufferHeader*)container.EntityManager.GetComponentDataRawRO(container.Entity, TypeIndex), IsReadOnly);
                }
                else
                {
                    return new DynamicBufferContainer<TElement>((BufferHeader*)container.EntityManager.GetComponentDataRawRW(container.Entity, TypeIndex), IsReadOnly);
                }
            }

            protected override void DoSetValue(ref EntityContainer container, DynamicBufferContainer<TElement> value)
            {
                throw new NotImplementedException();
            }
        }

        readonly Dictionary<int, IComponentProperty> m_ReadOnlyPropertyCache = new Dictionary<int, IComponentProperty>();
        readonly Dictionary<int, IComponentProperty> m_ReadWritePropertyCache = new Dictionary<int, IComponentProperty>();

        readonly ComponentPropertyConstructor m_ComponentPropertyConstructor = new ComponentPropertyConstructor();

        internal override IEnumerable<IProperty<EntityContainer>> GetProperties(ref EntityContainer container)
        {
            return EnumerateProperties(container);
        }

        IEnumerable<IProperty<EntityContainer>> EnumerateProperties(EntityContainer container)
        {
            var count = container.GetComponentCount();

            for (var i = 0; i < count; i++)
            {
                var typeIndex = container.EntityManager.GetComponentTypeIndex(container.Entity, i);
                var property = GetOrCreatePropertyForType(typeIndex, container.IsReadOnly);
                yield return property;
            }
        }

        bool IPropertyNameable<EntityContainer>.TryGetProperty(ref EntityContainer container, string name, out IProperty<EntityContainer> property)
        {
            foreach (var p in EnumerateProperties(container))
            {
                if (p.Name != name) continue;
                property = p;
                return true;
            }

            property = null;
            return false;
        }

        bool IPropertyIndexable<EntityContainer>.TryGetProperty(ref EntityContainer container, int index, out IProperty<EntityContainer> property)
        {
            property = GetOrCreatePropertyForType(container.EntityManager.GetComponentTypeIndex(container.Entity, index), container.IsReadOnly);
            return true;
        }

        IComponentProperty GetOrCreatePropertyForType(int typeIndex, bool isReadOnly)
        {
            var cache = isReadOnly ? m_ReadOnlyPropertyCache : m_ReadWritePropertyCache;

            if (cache.TryGetValue(typeIndex, out var property))
                return property;

            m_ComponentPropertyConstructor.TypeIndex = typeIndex;
            m_ComponentPropertyConstructor.IsReadOnly = isReadOnly;
            PropertyBagStore.GetPropertyBag(TypeManager.GetType(typeIndex)).Accept(m_ComponentPropertyConstructor);
            cache.Add(typeIndex, m_ComponentPropertyConstructor.Property);
            return m_ComponentPropertyConstructor.Property;
        }

        static string GetTypeName(Type type)
        {
            var index = 0;
            return GetTypeName(type, type.GetGenericArguments(), ref index);
        }

        static string GetTypeName(Type type, IReadOnlyList<Type> args, ref int argIndex)
        {
            var name = type.Name;

            if (type.IsGenericParameter)
            {
                return name;
            }

            if (type.IsNested)
            {
                name = $"{GetTypeName(type.DeclaringType, args, ref argIndex)}.{name}";
            }

            if (type.IsGenericType)
            {
                var tickIndex = name.IndexOf('`');

                if (tickIndex > -1)
                    name = name.Remove(tickIndex);

                var genericTypes = type.GetGenericArguments();
                var genericTypeNames = new StringBuilder();

                for (var i = 0; i < genericTypes.Length && argIndex < args.Count; i++, argIndex++)
                {
                    if (i != 0) genericTypeNames.Append(", ");
                    genericTypeNames.Append(GetTypeName(args[argIndex]));
                }

                if (genericTypeNames.Length > 0)
                {
                    name = $"{name}<{genericTypeNames}>";
                }
            }

            return name;
        }
    }
}
