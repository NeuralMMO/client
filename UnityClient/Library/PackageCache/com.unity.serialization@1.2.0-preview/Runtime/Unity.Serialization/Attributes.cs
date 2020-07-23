using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Unity.Serialization
{
    /// <summary>
    /// Use this attribute to rename a struct, class, field or property without losing its serialized value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class FormerNameAttribute : Attribute
    {
        /// <summary>
        /// The previous name of the member or type.
        /// </summary>
        public string OldName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FormerNameAttribute"/> with the specified name.
        /// </summary>
        /// <param name="oldName">The previous name of the member or type.</param>
        public FormerNameAttribute(string oldName)
        {
            OldName = oldName;
        }
        
        static readonly Dictionary<string, string> s_FormerlySerializedAsToCurrentName = new Dictionary<string, string>();
        static bool m_Registered;

        static void RegisterFormerlySerializedAsTypes()
        {
            if (m_Registered)
                return;

            m_Registered = true;
            
#if UNITY_EDITOR
            foreach (var type in UnityEditor.TypeCache.GetTypesWithAttribute<FormerNameAttribute>())
            {
                if (type.IsAbstract || type.IsGenericType)
                {
                    continue;
                }

                var attributes = (FormerNameAttribute[])type.GetCustomAttributes(typeof(FormerNameAttribute), false);
                foreach (var attribute in attributes)
                {
                    s_FormerlySerializedAsToCurrentName.Add(attribute.OldName, $"{type}, {type.Assembly.GetName().Name}");
                }
            }
#else
            var types = AppDomain.CurrentDomain.GetAssemblies()
                                      .SelectMany(a => a.GetTypes())
                                      .Where(t => !(t.IsAbstract || t.IsGenericType));

            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes<FormerNameAttribute>();

                foreach (var attribute in attributes)
                {
                    s_FormerlySerializedAsToCurrentName.Add(attribute.OldName, $"{type}, {type.Assembly.GetName().Name}");
                }
            }
#endif
        }

        /// <summary>
        /// Gets the current name based on the previous name.
        /// </summary>
        /// <param name="oldName">The previous name of the member or type.</param>
        /// <param name="currentName">When this method returns, contains the current type name, if the name exists; otherwise default string.</param>
        /// <returns>True if the given name exists in the remap table.</returns>
        public static bool TryGetCurrentTypeName(string oldName, out string currentName)
        {
            RegisterFormerlySerializedAsTypes();
            return s_FormerlySerializedAsToCurrentName.TryGetValue(oldName, out currentName);
        }
    }

    /// <summary>
    /// Use this attribute to flag a field or property to be ignored during serialization. This class cannot be inherited.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DontSerializeAttribute : Attribute
    {
        
    }
}