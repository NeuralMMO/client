using System;
using Unity.Properties.Editor;
using Unity.Serialization;

namespace Unity.Build
{
    internal static class TypeConstructionHelper
    {
        public static T ConstructFromAssemblyQualifiedTypeName<T>(string assemblyQualifiedTypeName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedTypeName))
            {
                throw new ArgumentException(nameof(assemblyQualifiedTypeName));
            }

            var type = Type.GetType(assemblyQualifiedTypeName);
            if (null == type && FormerNameAttribute.TryGetCurrentTypeName(assemblyQualifiedTypeName, out var currentTypeName))
            {
                type = Type.GetType(currentTypeName);
            }
            return TypeConstruction.Construct<T>(type);
        }

        public static bool TryConstructFromAssemblyQualifiedTypeName<T>(string assemblyQualifiedTypeName, out T value)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedTypeName))
            {
                value = default;
                return false;
            }

            try
            {
                var type = Type.GetType(assemblyQualifiedTypeName);
                if (null == type && FormerNameAttribute.TryGetCurrentTypeName(assemblyQualifiedTypeName, out var currentTypeName))
                {
                    type = Type.GetType(currentTypeName);
                }
                return TypeConstruction.TryConstruct(type, out value);
            }
            catch
            {
                value = default;
                return false;
            }
        }
    }
}
