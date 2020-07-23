using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Editor
{
    public class EntitySelectionProxy : ScriptableObject
    {
        internal static event EntityDebugger.SelectionChangeCallback OnEntitySelectionChanged;

        public delegate void EntityControlSelectButtonHandler(World world, Entity entity);

        public event EntityControlSelectButtonHandler EntityControlSelectButton;

        public EntityContainer Container { get; private set; }
        public Entity Entity
        {
            get { return new Entity() {Index = entityIndex, Version = entityVersion}; }
            private set
            {
                entityIndex = value.Index;
                entityVersion = value.Version;
            }
        }
        [SerializeField] int entityIndex;
        [SerializeField] int entityVersion;
        public EntityManager EntityManager { get; private set; }
        public World World { get; private set; }

        public bool Exists => EntityManager.IsCreated && EntityManager.Exists(Entity);

        public void OnEntityControlSelectButton(World world, Entity entity)
        {
            EntityControlSelectButton(world, entity);
        }

        public void SetEntity(World world, Entity entity)
        {
            World = world;
            Entity = entity;
            EntityManager = world.EntityManager;
            Container = new EntityContainer(EntityManager, Entity);
            EditorUtility.SetDirty(this);
            OnEntitySelectionChanged?.Invoke(this);
        }
    }
}
