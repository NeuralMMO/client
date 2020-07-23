using NUnit.Framework;

namespace Unity.Platforms.Tests
{
    class BasicTests
    {
        [Test]
        public void VerifyCanReferenceEditorBuildTarget()
        {
            Assert.IsNotNull(typeof(EditorBuildTarget));
        }
    }
}
