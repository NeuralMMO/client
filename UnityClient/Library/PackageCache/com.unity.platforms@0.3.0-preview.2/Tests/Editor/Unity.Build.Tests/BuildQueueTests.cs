using NUnit.Framework;
using System.Collections.Generic;

namespace Unity.Build.Tests
{
    public class BuildQueueTests
    {
        [Test]
        public void CanSortBuildsCorrectly()
        {
            var sorterActiveTargetAndroid = new BuildQueue.BuildStorter(UnityEditor.BuildTarget.Android);
            var sorterActiveTargetStandaloneWindows = new BuildQueue.BuildStorter(UnityEditor.BuildTarget.StandaloneWindows);

            var builds = new List<BuildQueue.QueuedBuild>(new[]
            {
                new BuildQueue.QueuedBuild(){ sortingIndex = (int)UnityEditor.BuildTarget.StandaloneWindows},
                new BuildQueue.QueuedBuild(){ sortingIndex = (int)UnityEditor.BuildTarget.NoTarget},
                new BuildQueue.QueuedBuild(){ sortingIndex = (int)UnityEditor.BuildTarget.iOS},
                new BuildQueue.QueuedBuild(){ sortingIndex = (int)UnityEditor.BuildTarget.Android},
            });

            builds.Sort(sorterActiveTargetAndroid.Compare);

            Assert.That(builds[0].sortingIndex == (int)UnityEditor.BuildTarget.NoTarget || builds[0].sortingIndex == (int)UnityEditor.BuildTarget.Android, Is.True);
            Assert.That(builds[2].sortingIndex == (int)UnityEditor.BuildTarget.StandaloneWindows, Is.True);
            Assert.That(builds[3].sortingIndex == (int)UnityEditor.BuildTarget.iOS, Is.True);

            builds.Sort(sorterActiveTargetStandaloneWindows.Compare);

            Assert.That(builds[0].sortingIndex == (int)UnityEditor.BuildTarget.NoTarget || builds[0].sortingIndex == (int)UnityEditor.BuildTarget.StandaloneWindows, Is.True);
            Assert.That(builds[2].sortingIndex == (int)UnityEditor.BuildTarget.iOS, Is.True);
            Assert.That(builds[3].sortingIndex == (int)UnityEditor.BuildTarget.Android, Is.True);
        }
    }
}
