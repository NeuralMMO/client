using Unity.Serialization.Json;

namespace Unity.Build.DotsRuntime
{
    public interface IDotsRuntimeBuildModifier : IBuildComponent
    {
        void Modify(JsonObject jsonObject);
    }
}
