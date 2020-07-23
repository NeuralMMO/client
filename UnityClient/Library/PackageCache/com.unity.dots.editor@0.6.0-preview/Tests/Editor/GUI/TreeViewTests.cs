using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Editor.Bridge;

namespace Unity.Entities.Editor.Tests
{
    [TestFixture]
    class TreeViewTests
    {
        const int s_RootItemCount = 5;
        const int s_TreeViewSize = 200;

        const string s_LabelIdName = "label-id";
        const string s_LabelSiblingIndexName = "label-sibling-index";

        int m_FinalId;
        TreeView m_TreeView;
        ListView m_ListView;
        ScrollView m_ScrollView;
        IList<ITreeViewItem> m_RawItemList;

        [SetUp]
        public void TestsSetup()
        {
            int nextId = 0;
            m_RawItemList = GenerateItemList(s_RootItemCount, ref nextId);
            m_FinalId = nextId;

            Func<VisualElement> makeItem = () =>
            {
                var box = new VisualElement();
                box.style.flexDirection = FlexDirection.Row;
                box.style.flexGrow = 1f;
                box.style.flexShrink = 0f;
                box.style.flexBasis = 0f;

                var labelId = new Label() { name = s_LabelIdName };
                var labelSiblingIndex = new Label() { name = s_LabelSiblingIndexName };

                box.Add(labelId);
                box.Add(labelSiblingIndex);
                return box;
            };

            Action<VisualElement, ITreeViewItem> bindItem = (e, i) =>
            {
                e.Q<Label>(s_LabelIdName).text = i.id.ToString();
                e.Q<Label>(s_LabelSiblingIndexName).text = (i as TreeViewItem<int>).data.ToString();
            };

            m_TreeView = new TreeView(m_RawItemList, 20, makeItem, bindItem);
            m_ListView = m_TreeView.Q<ListView>();
            m_ScrollView = m_ListView.Q<ScrollView>();

            m_TreeView.selectionType = SelectionType.Single;
            m_TreeView.style.height = s_TreeViewSize;
            m_TreeView.style.width = s_TreeViewSize;
        }

        IList<ITreeViewItem> GenerateItemList(int count, ref int nextId)
        {
            var items = new List<ITreeViewItem>(count);

            for (int i = 0; i < count; ++i)
            {
                var currentId = nextId;
                nextId++;

                var newItem = new TreeViewItem<int>(currentId, i);

                if (count > 2)
                    newItem.AddChildren(GenerateItemList(count / 2, ref nextId));

                items.Add(newItem);
            }

            return items;
        }

        void CheckFirstItemExpansion()
        {
            Assert.AreEqual(s_RootItemCount + s_RootItemCount / 2, m_ListView.contentContainer.childCount);

            // Look at the first item, plus its immediate children.
            var iterator = m_ListView.contentContainer.Children().GetEnumerator();
            for (int i = 0; i < 1 + s_RootItemCount / 2; i++)
            {
                iterator.MoveNext();
                var currentElement = iterator.Current;
                Assert.AreEqual(i.ToString(), currentElement.Q<Label>(s_LabelIdName).text);
            }

            // Next item should be the second root item.
            iterator.MoveNext();
            Assert.AreEqual(1.ToString(), iterator.Current.Q<Label>(s_LabelSiblingIndexName).text);
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
        public void NoDataSource()
        {
            var emptyTreeView = new TreeView();

            Assert.Throws<ArgumentOutOfRangeException>(() => emptyTreeView.SetSelection(0));
            // Nothing should happen.
            emptyTreeView.Refresh();
        }

#endif

        [Test]
        public void CycleThroughAllItems()
        {
            int i = 0;
            foreach (var item in m_TreeView.items)
            {
                Assert.AreEqual(i, item.id);
                i++;
            }
            Assert.AreEqual(m_FinalId, i);
        }

#if UNITY_2020_1_OR_NEWER
        [Test]
        public void ShowBorderOption()
        {
            m_TreeView.showBorder = false;
            Assert.IsFalse(m_ListView.ClassListContains(ListView.borderUssClassName));
            m_TreeView.showBorder = true;
            Assert.IsTrue(m_ListView.ClassListContains(ListView.borderUssClassName));
            m_TreeView.showBorder = false;
            Assert.IsFalse(m_ListView.ClassListContains(ListView.borderUssClassName));
        }

#endif
    }
}
