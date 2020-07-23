using System;

namespace Unity.Entities.Tests
{
#if !UNITY_DISABLE_MANAGED_COMPONENTS
    [GenerateAuthoringComponent]
    public class CodeGenManagedTestComponent : IComponentData
    {
        public Entity[] Entities;
        public string String;
    }
#endif
}
