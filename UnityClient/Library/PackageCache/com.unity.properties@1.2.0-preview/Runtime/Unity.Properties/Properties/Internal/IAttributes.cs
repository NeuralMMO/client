using System;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// Scope for using a given set of attributes.
    /// </summary>
    struct AttributesScope : IDisposable
    {
        readonly IAttributes m_Target;
        readonly List<Attribute> m_Previous;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributesScope"/> struct assigns the given attributes to the specified target.
        /// </summary>
        /// <param name="target">The target to set the attributes for.</param>
        /// <param name="attributes">The attributes to set.</param>
        public AttributesScope(IAttributes target, List<Attribute> attributes)
        {
            m_Target = target;
            m_Previous = target.Attributes;
            target.Attributes = attributes;
        }

        /// <summary>
        /// Re-assigns the original attributes to the target.
        /// </summary>
        public void Dispose()
        {
            m_Target.Attributes = m_Previous;
        }
    }
    
    /// <summary>
    /// Interface for attaching attributes to an object. This is an internal interface.
    /// </summary>
    interface IAttributes
    {
        /// <summary>
        /// Gets access the the internal <see cref="Attribute"/> storage.
        /// </summary>
        List<Attribute> Attributes { get; set; }
        
        /// <summary>
        /// Adds an attribute to this object.
        /// </summary>
        /// <param name="attribute">The attribute to add.</param>
        void AddAttribute(Attribute attribute);

        /// <summary>
        /// Adds a set of attributes to this object.
        /// </summary>
        /// <param name="attributes"></param>
        void AddAttributes(IEnumerable<Attribute> attributes);

        /// <summary>
        /// Sets the attributes for the duration of the scope.
        /// </summary>
        /// <param name="attributes"></param>
        AttributesScope CreateAttributesScope(IAttributes attributes);
    }
}