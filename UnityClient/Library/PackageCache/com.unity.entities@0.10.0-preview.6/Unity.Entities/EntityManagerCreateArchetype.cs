using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Entities
{
    public unsafe partial struct EntityManager
    {
        // ----------------------------------------------------------------------------------------------------------
        // PUBLIC
        // ----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Creates an archetype from a set of component types.
        /// </summary>
        /// <remarks>
        /// Creates a new archetype in the ECS framework's internal type registry, unless the archetype already exists.
        /// </remarks>
        /// <param name="types">The component types to include as part of the archetype.</param>
        /// <returns>The EntityArchetype object for the archetype.</returns>
        public EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            fixed(ComponentType* typesPtr = types)
            {
                var access = GetCheckedEntityDataAccess();
                var ecs = access->EntityComponentStore;

                ecs->AssertCanCreateArchetype(typesPtr, types.Length);
                return access->CreateArchetype(typesPtr, types.Length);
            }
        }

        // ----------------------------------------------------------------------------------------------------------
        // INTERNAL
        // ----------------------------------------------------------------------------------------------------------

        internal EntityArchetype CreateArchetype(ComponentType* types, int count)
        {
            var access = GetCheckedEntityDataAccess();
            return access->CreateArchetype(types, count);
        }
    }
}
