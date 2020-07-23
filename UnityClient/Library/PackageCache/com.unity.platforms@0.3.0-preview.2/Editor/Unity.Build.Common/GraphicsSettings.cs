using Unity.Serialization;
using UnityEngine;

namespace Unity.Build.Common
{
    [FormerName("Unity.Build.Common.GraphicsSettings, Unity.Build.Common")]
    public sealed class GraphicsSettings : IBuildComponent
    {
        public ColorSpace ColorSpace = ColorSpace.Uninitialized;
    }
}
