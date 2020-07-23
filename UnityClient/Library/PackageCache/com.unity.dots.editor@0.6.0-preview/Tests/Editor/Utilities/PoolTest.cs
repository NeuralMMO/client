using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Entities.Editor.Tests
{
    class PoolTest
    {
        [Test]
        public void CanGetAndReleasePooledList()
        {
            var pooled = Pooling.GetList<int>();
            Assert.That(pooled.List, Is.Not.Null);
            Assert.That(() => pooled.Dispose(), Throws.Nothing);
        }

        [Test]
        public void ReleasingAPooledListClearsIt()
        {
            const int count = 5;
            var pooled = Pooling.GetList<int>();
            var list = pooled.List;
            Assert.That(list.Count, Is.EqualTo(0));
            for (var i = 0; i < count; ++i)
            {
                list.Add(i);
            }

            Assert.That(list.Count, Is.EqualTo(count));
            Assert.That(() => pooled.Dispose(), Throws.Nothing);
            Assert.That(list.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAndReleaseClearList()
        {
            var list = new List<int> {0, 1, 2, 3, 4, 5};
            List<int> hackerList;
            using (var pooled = list.ToPooledList())
            {
                hackerList = pooled.List;
                Assert.That(list, Is.EqualTo(pooled.List));
                Assert.That(list, Is.Not.SameAs(pooled.List));
            }
            Assert.That(hackerList.Count, Is.EqualTo(0));
            hackerList.AddRange(list);
            Assert.That(hackerList.Count, Is.EqualTo(list.Count));

            using (var pooled = Pooling.GetList<int>())
            {
                // This relies on the fact that we internally uses a stack, so getting a new one will re-use the last returned one.
                Assert.That(pooled.List, Is.SameAs(hackerList));
                Assert.That(hackerList.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ReleasingMultipleTimesThrows()
        {
            var pooled = Pooling.GetList<int>();
            Assert.That(() => pooled.Dispose(), Throws.Nothing);
            Assert.That(() => pooled.Dispose(), Throws.InvalidOperationException);
        }

        [Test]
        public void CanGetAndReleaseInDifferentOrder()
        {
            var pooled = Pooling.GetList<int>();
            var pooled2 = Pooling.GetList<int>();
            Assert.That(() => pooled2.Dispose(), Throws.Nothing);
            Assert.That(() => pooled.Dispose(), Throws.Nothing);
        }

        [Test]
        public void MultipleGetResultsInDifferentPooledLists()
        {
            var pooled = Pooling.GetList<int>();
            var pooled2 = Pooling.GetList<int>();
            Assert.That(pooled.List, Is.Not.SameAs(pooled2.List));
            Assert.That(() => pooled2.Dispose(), Throws.Nothing);
            Assert.That(() => pooled.Dispose(), Throws.Nothing);
        }

        [Test]
        public void PooledListAreIndeedPooled()
        {
            var releasedPools = new HashSet<List<int>>();
            using (var pooled = Pooling.GetList<int>())
            using (var pooled2 = Pooling.GetList<int>())
            using (var pooled3 = Pooling.GetList<int>())
            {
                releasedPools.Add(pooled.List);
                releasedPools.Add(pooled2.List);
                releasedPools.Add(pooled3.List);
            }

            using (var pooled = Pooling.GetList<int>())
            using (var pooled2 = Pooling.GetList<int>())
            using (var pooled3 = Pooling.GetList<int>())
            {
                Assert.That(releasedPools, Contains.Item(pooled.List));
                Assert.That(releasedPools, Contains.Item(pooled2.List));
                Assert.That(releasedPools, Contains.Item(pooled3.List));
            }
        }

        [Test]
        public void CreatePooledListFromLinq()
        {
            var list = new List<int> {0, 1, 2, 3, 4, 5};
            using (var pooled = list.ToPooledList())
            {
                Assert.That(list, Is.EqualTo(pooled.List));
                Assert.That(list, Is.Not.SameAs(pooled.List));
            }
        }
    }
}
