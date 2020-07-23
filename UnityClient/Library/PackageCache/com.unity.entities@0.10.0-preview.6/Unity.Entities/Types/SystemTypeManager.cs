using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Unity.Entities
{
    static public unsafe partial class TypeManager
    {
#pragma warning disable 414
        static int s_SystemCount;
#pragma warning restore 414
        static List<Type> s_SystemTypes = new List<Type>();
        static List<string> s_SystemTypeNames = new List<string>();
        static NativeList<bool> s_SystemIsGroupList;
#if NET_DOTS
        static List<int> s_SystemTypeDelegateIndexRanges = new List<int>();
        static List<TypeRegistry.CreateSystemFn> s_AssemblyCreateSystemFn = new List<TypeRegistry.CreateSystemFn>();
        static List<TypeRegistry.GetSystemAttributesFn> s_AssemblyGetSystemAttributesFn = new List<TypeRegistry.GetSystemAttributesFn>();
#endif

        // While we provide a public interface for the TypeManager the init/shutdown
        // of the TypeManager owned by the TypeManager so we mark these functions as internal
        private static void InitializeSystemsState()
        {
            s_SystemTypes = new List<Type>();
            s_SystemTypeNames = new List<string>();
            s_SystemIsGroupList = new NativeList<bool>(Allocator.Persistent);
            s_SystemCount = 0;
        }

        private static void ShutdownSystemsState()
        {
            s_SystemTypes.Clear();
            s_SystemTypeNames.Clear();
            s_SystemIsGroupList.Dispose();
            s_SystemCount = 0;
        }

        /// <summary>
        /// Construct a System from a Type. Uses the same list in GetSystems()
        /// </summary>
        ///
        public static ComponentSystemBase ConstructSystem(Type systemType)
        {
#if !NET_DOTS
            if (!typeof(ComponentSystemBase).IsAssignableFrom(systemType))
                throw new ArgumentException($"'{systemType.FullName}' cannot be constructed as it does not inherit from ComponentSystemBase");
            return (ComponentSystemBase)Activator.CreateInstance(systemType);
#else
            Assertions.Assert.IsTrue(s_Initialized, "The TypeManager must be initialized before the TypeManager can be used.");

            var obj = CreateSystem(systemType);
            if (!(obj is ComponentSystemBase))
                throw new ArgumentException("Null casting in Construct System. Bug in TypeManager.");
            return obj as ComponentSystemBase;
#endif
        }

        public static T ConstructSystem<T>() where T : ComponentSystemBase
        {
            return (T)ConstructSystem(typeof(T));
        }

        public static T ConstructSystem<T>(Type systemType) where T : ComponentSystemBase
        {
            return (T)ConstructSystem(systemType);
        }

        /// <summary>
        /// Return an array of all the Systems in use. (They are found
        /// at compile time, and inserted by code generation.)
        /// </summary>
        public static Type[] GetSystems()
        {
            Assertions.Assert.IsTrue(s_Initialized, "The TypeManager must be initialized before the TypeManager can be used.");

            return s_SystemTypes.ToArray();
        }

        public static string GetSystemName(Type t)
        {
#if !NET_DOTS
            return t.FullName;
#else
            Assertions.Assert.IsTrue(s_Initialized, "The TypeManager must be initialized before the TypeManager can be used.");

            int index = GetSystemTypeIndex(t);
            if (index < 0 || index >= s_SystemTypeNames.Count) return "null";
            return s_SystemTypeNames[index];
#endif
        }

        public static int GetSystemTypeIndex(Type t)
        {
            Assertions.Assert.IsTrue(s_Initialized, "The TypeManager must be initialized before the TypeManager can be used.");

            for (int i = 0; i < s_SystemTypes.Count; ++i)
            {
                if (t == s_SystemTypes[i]) return i;
            }
            throw new ArgumentException($"Could not find a matching system type for passed in type.");
        }

        public static bool IsSystemAGroup(Type t)
        {
#if !NET_DOTS
            return t.IsSubclassOf(typeof(ComponentSystemGroup));
#else
            Assertions.Assert.IsTrue(s_Initialized, "The TypeManager must be initialized before the TypeManager can be used.");

            int index = GetSystemTypeIndex(t);
            var isGroup = s_SystemIsGroupList[index];
            return isGroup;
#endif
        }

        /// <summary>
        /// Get all the attribute objects of Type attributeType for a System.
        /// </summary>
        public static Attribute[] GetSystemAttributes(Type systemType, Type attributeType)
        {
            Assertions.Assert.IsTrue(s_Initialized, "The TypeManager must be initialized before the TypeManager can be used.");

#if !NET_DOTS
            var objArr = systemType.GetCustomAttributes(attributeType, true);
            var attr = new Attribute[objArr.Length];
            for (int i = 0; i < objArr.Length; i++)
            {
                attr[i] = objArr[i] as Attribute;
            }
            return attr;
#else
            Attribute[] attr = GetSystemAttributes(systemType);
            int count = 0;
            for (int i = 0; i < attr.Length; ++i)
            {
                if (attr[i].GetType() == attributeType)
                {
                    ++count;
                }
            }
            Attribute[] result = new Attribute[count];
            count = 0;
            for (int i = 0; i < attr.Length; ++i)
            {
                if (attr[i].GetType() == attributeType)
                {
                    result[count++] = attr[i];
                }
            }
            return result;
#endif
        }

#if NET_DOTS
        static object CreateSystem(Type systemType)
        {
            int systemIndex = 0;
            for (; systemIndex < s_SystemTypes.Count; ++systemIndex)
            {
                if (s_SystemTypes[systemIndex] == systemType)
                    break;
            }

            for (int i = 0; i < s_SystemTypeDelegateIndexRanges.Count; ++i)
            {
                if (systemIndex < s_SystemTypeDelegateIndexRanges[i])
                    return s_AssemblyCreateSystemFn[i](systemType);
            }

            throw new ArgumentException("No function was generated for the provided type.");
        }

        internal static Attribute[] GetSystemAttributes(Type system)
        {
            int typeIndexNoFlags = 0;
            for (; typeIndexNoFlags < s_SystemTypes.Count; ++typeIndexNoFlags)
            {
                if (s_SystemTypes[typeIndexNoFlags] == system)
                    break;
            }

            for (int i = 0; i < s_SystemTypeDelegateIndexRanges.Count; ++i)
            {
                if (typeIndexNoFlags < s_SystemTypeDelegateIndexRanges[i])
                    return s_AssemblyGetSystemAttributesFn[i](system);
            }

            throw new ArgumentException("No function was generated for the provided type.");
        }

        internal static void RegisterAssemblySystemTypes(TypeRegistry typeRegistry)
        {
            foreach (var type in typeRegistry.SystemTypes)
            {
                s_SystemTypes.Add(type);
                s_SystemCount++;
            }

            foreach (var typeName in typeRegistry.SystemTypeNames)
            {
                s_SystemTypeNames.Add(typeName);
            }

            foreach (var isSystemGroup in typeRegistry.IsSystemGroup)
            {
                s_SystemIsGroupList.Add(isSystemGroup);
            }

            if (typeRegistry.SystemTypes.Length > 0)
            {
                s_SystemTypeDelegateIndexRanges.Add(s_SystemCount);

                s_AssemblyCreateSystemFn.Add(typeRegistry.CreateSystem);
                s_AssemblyGetSystemAttributesFn.Add(typeRegistry.GetSystemAttributes);
            }
        }

#endif
    }
}
