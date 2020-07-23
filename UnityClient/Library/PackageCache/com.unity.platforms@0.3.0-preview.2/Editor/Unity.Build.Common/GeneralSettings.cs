using Unity.Serialization;

namespace Unity.Build.Common
{
    [FormerName("Unity.Build.Common.GeneralSettings, Unity.Build.Common")]
    public sealed class GeneralSettings : IBuildComponent
    {
        public string ProductName = "Product Name";
        public string CompanyName = "Company Name";
    }
}
