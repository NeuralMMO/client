using System.Linq;
using Unity.BuildSystem.NativeProgramSupport;

namespace Unity.Build.Classic.Private
{
    class BuildPipelineSelector : BuildPipelineSelectorBase
    {
        public override BuildPipelineBase SelectFor(Platform platform)
        {
            var classicPipelineBases = TypeCacheHelper.ConstructTypesDerivedFrom<ClassicPipelineBase>();
            return classicPipelineBases.FirstOrDefault(pipeline => pipeline.Platform.GetType() == platform.GetType());
        }
    }
}
