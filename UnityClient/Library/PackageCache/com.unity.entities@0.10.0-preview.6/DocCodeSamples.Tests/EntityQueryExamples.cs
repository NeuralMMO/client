using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Doc.CodeSamples.Tests
{
    #region singleton-type-example
    public struct Singlet : IComponentData
    {
        public int Value;
    }
    #endregion
    //Types used in examples below
    public struct Melee : IComponentData {}
    public struct Ranger : IComponentData {}
    public struct Player : IComponentData {}
    public struct Position : IComponentData {}

    public class EntityQueryExamples : SystemBase
    {
        void queryFromList()
        {
            #region query-from-list
            EntityQuery query = GetEntityQuery(typeof(Rotation),
                ComponentType.ReadOnly<RotationSpeed>());
            #endregion
        }

        void queryFromDescription()
        {
            #region query-from-description
            EntityQueryDesc description = new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(Frozen)
                },
                All = new ComponentType[]
                {
                    typeof(Rotation),
                    ComponentType.ReadOnly<RotationSpeed>()
                }
            };
            EntityQuery query = GetEntityQuery(description);

            #endregion
        }

        protected override void OnCreate()
        {
            #region query-description

            EntityQueryDesc description = new EntityQueryDesc
            {
                Any = new ComponentType[] { typeof(Melee), typeof(Ranger) },
                None = new ComponentType[] { typeof(Player) },
                All = new ComponentType[] { typeof(Position), typeof(Rotation) }
            };

            #endregion
            var query = GetEntityQuery(description);
            Entity entity = Entity.Null;
            #region entity-query-mask

            var mask = EntityManager.GetEntityQueryMask(query);
            bool doesMatch = mask.Matches(entity);

            #endregion
        }

        protected override void OnUpdate()
        {
            var queryForSingleton = EntityManager.CreateEntityQuery(typeof(Singlet));
            var entityManager = EntityManager;
            #region create-singleton

            Entity singletonEntity = entityManager.CreateEntity(typeof(Singlet));
            entityManager.SetComponentData(singletonEntity, new Singlet { Value = 1 });

            #endregion


            #region set-singleton
            queryForSingleton.SetSingleton<Singlet>(new Singlet {Value = 1});
            #endregion
        }
    }
}
