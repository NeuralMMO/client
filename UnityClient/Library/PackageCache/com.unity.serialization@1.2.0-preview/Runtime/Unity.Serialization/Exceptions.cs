using System;

namespace Unity.Serialization
{
    /// <summary>
    /// The exception thrown when an error occurs during serialization or deserialization.
    /// </summary>
    [Serializable]
    public class SerializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public SerializationException(string message) : base(message)
        {
            
        }
    }
}