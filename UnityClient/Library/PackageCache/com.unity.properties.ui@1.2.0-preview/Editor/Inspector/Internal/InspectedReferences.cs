using System.Collections.Generic;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// The <see cref="InspectedReferences"/> class can be used to prevent circular object references in the UI.
    /// </summary>
    class InspectedReferences
    {
        readonly HashSet<object> m_References = new HashSet<object>();
        readonly Dictionary<object, PropertyPath> m_ReferenceToPath = new Dictionary<object, PropertyPath>();

        /// <summary>
        /// Flags the specified object as being gathered during the inspector visitation. This is an internal method.
        /// </summary>
        /// <param name="value">The object being visited.</param>
        /// <returns><see langword="true"/> if this is the first time encountering this object; otherwise, <see langword="false"/>.</returns>
        internal bool PushReference(object value, PropertyPath path)
        {
            if (m_References.Add(value))
            {
                m_ReferenceToPath[value] = path;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the specified object from the current visitation.
        /// </summary>
        /// <param name="value">The object being visited.</param>
        /// <returns><see langword="true"/> if the object has been removed.</returns>
        internal bool PopReference(object value)
            => m_References.Remove(value);

        /// <summary>
        /// Returns the path to the first reference that was visited.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Path to the first visited object.</returns>
        internal PropertyPath GetPath(object value)
        {
            return m_ReferenceToPath.TryGetValue(value, out var path) ? path : null;
        }
        
        /// <summary>
        /// Clears this object for re-use. This is an internal method.
        /// </summary>
        internal void Clear()
        {
            m_References.Clear();
        }
    }
}