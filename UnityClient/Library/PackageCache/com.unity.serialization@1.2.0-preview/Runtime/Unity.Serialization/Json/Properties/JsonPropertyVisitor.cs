using System;
using Unity.Properties;

namespace Unity.Serialization.Json
{
    abstract class JsonPropertyVisitor
    {
        /// <summary>
        /// The key used as an id when a given object has references to it.
        /// </summary>
        internal const string k_SerializedId = "$id";
            
        /// <summary>
        /// The key used when an object is a serialized reference. This member will refers to the <see cref="k_SerializedId"/> of another object.
        /// </summary>
        internal const string k_SerializedReferenceKey = "$ref";
        
        /// <summary>
        /// The key used when writing out custom type information.
        /// This can be consumed during deserialization to reconstruct the concrete type.
        /// </summary>
        internal const string k_SerializedTypeKey = "$type";
            
        /// <summary>
        /// The key used when writing out serialized version information.
        /// This can be consumed during deserialization to handle migration.
        /// </summary>
        internal const string k_SerializedVersionKey = "$version";

        /// <summary>
        /// The key used to hold collection elements when extra mata-data is present.
        /// </summary>
        internal const string k_SerializedElementsKey = "$elements";
        
        /// <summary>
        /// Scope used to lock the current visitor as being in use.
        /// </summary>
        internal readonly struct LockScope : IDisposable
        {
            readonly JsonPropertyVisitor m_Visitor;

            /// <summary>
            /// Initializes a new instance of <see cref="LockScope"/>.
            /// </summary>
            /// <param name="visitor">The current visitor.</param>
            public LockScope(JsonPropertyVisitor visitor)
            {
                m_Visitor = visitor;
                visitor.IsLocked = true;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                m_Visitor.IsLocked = false;
            }
        }
        
        protected readonly struct PropertyScope : IDisposable
        {
            readonly JsonPropertyVisitor m_Visitor;
            readonly IProperty m_Property;

            public PropertyScope(JsonPropertyVisitor visitor, IProperty property)
            {
                m_Visitor = visitor;
                m_Property = m_Visitor.Property;
                m_Visitor.Property = property;
            }

            public void Dispose()
            {
                m_Visitor.Property = m_Property;
            }
        }
        
        /// <summary>
        /// Returns true if the reader is currently in use.
        /// </summary>
        internal bool IsLocked { get; private set; }
        
        /// <summary>
        /// Returns the property for the currently visited container.
        /// </summary>
        internal IProperty Property { get; private set; }
        
        /// <summary>
        /// Creates a container property scope for the visitation.
        /// </summary>
        /// <param name="property">The current container property being visited.</param>
        /// <returns>A disposable scope.</returns>
        protected PropertyScope CreatePropertyScope(IProperty property)
            => new PropertyScope(this, property);

        /// <summary>
        /// Creates a lock scope for the visitation.
        /// </summary>
        /// <returns>A disposable scope.</returns>
        internal LockScope Lock()
            => new LockScope(this);
    }
}