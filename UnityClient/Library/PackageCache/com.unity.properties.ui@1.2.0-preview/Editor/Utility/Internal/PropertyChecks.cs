using System;

namespace Unity.Properties.UI.Internal
{
    static class PropertyChecks
    {
        public static string GetNotConstructableWarningMessage(Type type)
            => $"Could not create an instance of type `{TypeUtility.GetResolvedTypeName(type)}`. A public parameter-less constructor or an explicit construction method is required.";

        public static string GetNotAssignableWarningMessage(Type type, Type assignableTo)
            => $"Could not create an instance of type `{TypeUtility.GetResolvedTypeName(type)}`: Type must be assignable to `{TypeUtility.GetResolvedTypeName(assignableTo)}`";
    }
}