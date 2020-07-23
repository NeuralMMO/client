namespace Unity.Serialization.Json.Adapters
{
    /// <summary>
    /// Base interface used to hold a reference to a typed <see cref="IJsonMigration{TValue}"/>.
    /// </summary>
    public interface IJsonMigration
    {
        
    }
    
    /// <summary>
    /// Interface used to describe how a specified type should be migrated from one version to another.
    /// </summary>
    /// <typeparam name="TValue">The type this interface defines migration for.</typeparam>
    public interface IJsonMigration<TValue> : IJsonMigration
    {
        /// <summary>
        /// The current serialized version for the type.
        /// </summary>
        int Version { get; }
        
        /// <summary>
        /// Implement this method to manage migration for <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="context">A context object used to unpack and transfer the data.</param>
        /// <returns>The deserialized and migrated value.</returns>
        TValue Migrate(JsonMigrationContext context);
    }

    namespace Contravariant
    {
        /// <summary>
        /// Interface used to describe how a specified type should be migrated from one version to another.
        /// </summary>
        /// <typeparam name="TValue">The type this interface defines migration for.</typeparam>
        public interface IJsonMigration<in TValue> : IJsonMigration
        {
            /// <summary>
            /// The current serialized version for the type.
            /// </summary>
            int Version { get; }
        
            /// <summary>
            /// Implement this method to manage migration for <typeparamref name="TValue"/>.
            /// </summary>
            /// <param name="context">A context object used to unpack and transfer the data.</param>
            /// <returns>The deserialized and migrated value.</returns>
            object Migrate(JsonMigrationContext context);
        }
    }
}