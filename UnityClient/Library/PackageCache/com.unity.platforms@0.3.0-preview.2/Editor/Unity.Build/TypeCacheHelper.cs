using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using UnityEditor;

namespace Unity.Build
{
    static class TypeCacheHelper
    {
        public static IEnumerable<T> ConstructTypesDerivedFrom<T>(bool fromUnityAssembliesOnly = true)
        {
            var types = TypeCache.GetTypesDerivedFrom<T>().Where(type => !type.IsAbstract && !type.IsGenericType);
            if (fromUnityAssembliesOnly)
            {
                types = types.Where(type => type.Assembly.GetName().Name.StartsWith("Unity."));
            }
            return types.Select(type => TypeConstruction.Construct<T>(type));
        }

        public static IEnumerable<T> ConstructTypesDerivedFrom<T>(Func<T, bool> predicate, bool fromUnityAssembliesOnly = true) =>
            ConstructTypesDerivedFrom<T>(fromUnityAssembliesOnly)
            .Where(predicate);

        public static T ConstructTypeDerivedFrom<T>(bool fromUnityAssembliesOnly = true) =>
            ConstructTypesDerivedFrom<T>(fromUnityAssembliesOnly)
            .FirstOrDefault();

        public static T ConstructTypeDerivedFrom<T>(Func<T, bool> predicate, bool fromUnityAssembliesOnly = true) =>
            ConstructTypesDerivedFrom(predicate, fromUnityAssembliesOnly)
            .FirstOrDefault();
    }
}
