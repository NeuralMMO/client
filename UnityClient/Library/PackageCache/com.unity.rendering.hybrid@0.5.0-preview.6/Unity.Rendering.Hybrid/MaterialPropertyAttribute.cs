using System;

namespace Unity.Rendering
{
    // TODO: Hybrid V2 doesn't need this anymore, but Hybrid V1 does. This type can be removed once
    // Hybrid V1 is no longer supported.
    public enum MaterialPropertyFormat
    {
        Float,
        Float2,
        Float3,
        Float4,
        Float2x4,
        Float4x4,
    }

    // Use this to mark an IComponentData as an input to a material property on a particular shader.
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public class MaterialPropertyAttribute : Attribute
    {
        public MaterialPropertyAttribute(string materialPropertyName, MaterialPropertyFormat format, int overrideSize = -1)
        {
            Name = materialPropertyName;
            Format = format;
            OverrideSize = overrideSize;
        }

        public string Name { get; }
        public MaterialPropertyFormat Format { get; }

        public int OverrideSize { get; }
    }
}
