using System;
using System.Collections.Generic;
using Unity.Properties.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class IListElement<TList, TElement> : NullableFoldout<TList>, ICustomStyleApplier
        where TList : IList<TElement>
    {
        readonly IntegerField m_Size;
        readonly Button m_AddItemButton;
        internal readonly VisualElement m_ContentRoot;
        readonly PaginationElement m_PaginationElement;

        bool UsesPagination { get; set; }

        public IListElement()
        {
            Resources.Templates.ListElement.Clone(this);
            Resources.Templates.ListElementDefaultStyling.AddStyles(this);
            binding = this;

            m_Size = new IntegerField();
            m_Size.AddToClassList(UssClasses.ListElement.Size);
            m_Size.RegisterValueChangedCallback(CountChanged);
            m_Size.RegisterCallback<KeyDownEvent>(TrapKeys);
            m_Size.isDelayed = true;

            var toggle = this.Q<Toggle>();
            var toggleInput = toggle.Q(className: UssClasses.Unity.ToggleInput);
            toggleInput.AddToClassList(UssClasses.ListElement.ToggleInput);
            toggle.Add(m_Size);

            m_AddItemButton = new Button(OnAddItem)
            {
                text = "+ Add Element"
            };
            m_AddItemButton.AddToClassList(UssClasses.ListElement.AddItemButton);

            m_ContentRoot = new VisualElement();
            m_ContentRoot.name = "properties-list-content";
            m_PaginationElement = new PaginationElement();
            Add(m_PaginationElement);
            Add(m_ContentRoot);
            Add(m_AddItemButton);
        }

        static void TrapKeys(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                evt.PreventDefault();
        }

        public override void OnContextReady()
        {
            var list = GetValue();
            if (list.IsReadOnly)
            {
                m_Size.SetEnabledSmart(false);
                m_AddItemButton.SetEnabledSmart(false);
            }

            UsesPagination = HasAttribute<PaginationAttribute>();
            if (!UsesPagination)
            {
                m_PaginationElement.Enabled = false;
            }

            var pagination = GetAttribute<PaginationAttribute>();
            if (null == pagination)
                return;

            m_PaginationElement.OnChanged += () =>
            {
                UiPersistentState.SetPaginationState(Root.GetTargetType(), Path, m_PaginationElement.PaginationSize,
                    m_PaginationElement.CurrentPage);
                Reload();
            };

            m_PaginationElement.SetPaginationSizes(pagination.Sizes);
            m_PaginationElement.AutoHide = pagination.AutoHide;
            var paginationData = UiPersistentState.GetPaginationState(Root.GetTargetType(), Path);
            if (!paginationData.Equals(default(UiPersistentState.PaginationData)))
            {
                m_PaginationElement.TotalCount = GetValue()?.Count ?? 0;
                m_PaginationElement.SetPaginationSize(paginationData.PaginationSize);
                m_PaginationElement.GoToPage(paginationData.CurrentPage);
            }
        }

        public override void Reload(IProperty property)
        {
            m_ContentRoot.Clear();

            var list = GetValue();
            if (null == list)
                return;

            m_PaginationElement.Update(list.Count);

            if (m_Size.focusController?.focusedElement != m_Size)
            {
                m_Size.isDelayed = false;
                m_Size.SetValueWithoutNotify(list.Count);
                m_Size.isDelayed = true;
            }

            var startIndex = 0;
            var endIndex = list.Count;

            if (UsesPagination)
            {
                startIndex = m_PaginationElement.StartIndex;
                endIndex = m_PaginationElement.EndIndex;
            }

            for (var i = startIndex; i < endIndex; ++i)
            {
                var index = i;
                Path.PushIndex(index);
                try
                {
                    var root = new VisualElement();
                    Root.VisitAtPath(Path, root);
                    MakeListElement(root, index);
                    m_ContentRoot.Add(root);
                }
                finally
                {
                    Path.Pop();
                }
            }
        }

        void CountChanged(ChangeEvent<int> evt)
        {
            evt.StopImmediatePropagation();
            evt.PreventDefault();
            var count = evt.newValue;
            if (count < 0)
            {
                m_Size.SetValueWithoutNotify(0);
                count = 0;
            }

            var iList = GetValue();
            if (null == iList)
                return;

            var constructContext = GetAttribute<CreateElementOnAddAttribute>();

            switch (iList)
            {
                case TElement[] array:
                    var newArray = new TElement[count];
                    for (var i = 0; i < Math.Min(array.Length, count); ++i)
                    {
                        newArray[i] = array[i];
                    }

                    for (var i = array.Length; i < newArray.Length; ++i)
                    {
                        newArray[i] = CreateInstance(constructContext);
                    }

                    Root.SetValue(Path, newArray);
                    break;
                case List<TElement> list:
                    while (list.Count > count)
                    {
                        list.RemoveAt(list.Count - 1);
                    }

                    while (list.Count < count)
                    {
                        list.Add(CreateInstance(constructContext));
                    }

                    break;
            }

            Root.NotifyChanged(Path);
            Reload();
        }

        static TElement CreateInstance(CreateElementOnAddAttribute context)
        {
            if (null == context)
                return default;

            var type = context.Type;
            return null == type
                ? TypeConstruction.Construct<TElement>()
                : TypeConstruction.Construct<TElement>(type);
        }

        protected override void OnUpdate()
        {
            var list = GetValue();
            if (null == list)
                return;

            if (!UsesPagination)
            {
                if (list.Count != m_ContentRoot.childCount)
                {
                    Reload();
                }
            }
            else
            {
                m_PaginationElement.Update(list.Count);

                var startIndex = m_PaginationElement.StartIndex;
                var endIndex = m_PaginationElement.EndIndex;

                if (m_PaginationElement.PaginationSize != m_ContentRoot.childCount)
                {
                    if (list.Count > 0 && m_PaginationElement.CurrentPage != m_PaginationElement.LastPage)
                    {
                        Reload();
                    }
                }

                for (var i = startIndex; i < endIndex; ++i)
                {
                    if (m_ContentRoot[i - startIndex].ClassListContains(UssClasses.ListElement.MakeListItem(i)))
                    {
                        continue;
                    }

                    Reload();
                    break;
                }
            }
        }

        void OnAddItem()
        {
            var iList = GetValue();
            if (null == iList)
                return;

            var item = CreateInstance(GetAttribute<CreateElementOnAddAttribute>());

            switch (iList)
            {
                case TElement[] array:
                    Root.SetValue(Path, ArrayUtility.InsertAt(array, array.Length, item));
                    break;
                case List<TElement> list:
                    list.Add(item);
                    break;
            }

            Root.NotifyChanged(Path);
            m_PaginationElement.TotalCount = iList.Count;
            m_PaginationElement.GoToLastPage();
            Reload();
        }

        void OnRemoveItem(int index)
        {
            var typedIList = GetValue();
            if (null == typedIList)
                return;

            switch (typedIList)
            {
                case TElement[] array:
                    Root.SetValue(Path, ArrayUtility.RemoveAt(array, index));
                    break;
                case List<TElement> list:
                    list.RemoveAt(index);
                    break;
            }

            Root.NotifyChanged(Path);

            m_PaginationElement.TotalCount = typedIList.Count;
            Reload();
        }

        void Swap(int index, int newIndex)
        {
            var iList = GetValue();
            if (null == iList)
                return;

            var temp = iList[index];
            iList[index] = iList[newIndex];
            iList[newIndex] = temp;
            Root.NotifyChanged(Path);
            Reload();
        }

        public void ApplyStyleAtPath(PropertyPath propertyPath)
        {
            var index = 0;
            for (; index < Path.PartsCount; ++index)
            {
                if (propertyPath.PartsCount == index)
                {
                    return;
                }

                if (!Path[index].Equals(propertyPath[index]))
                {
                    return;
                }
            }

            if (!propertyPath[index].IsIndex)
            {
                return;
            }

            var itemIndex = propertyPath[index].Index;
            if (UsesPagination)
            {
                itemIndex -= m_PaginationElement.StartIndex;
            }

            var current = m_ContentRoot[itemIndex];
            current.Q<Button>(className: UssClasses.ListElement.RemoveItemButton)?.RemoveFromHierarchy();

            MakeListElement(current, itemIndex);
        }

        void MakeListElement(VisualElement root, int index)
        {
            root.AddToClassList(UssClasses.ListElement.ItemContainer);
            root.AddToClassList(UssClasses.Variables);
            root.AddToClassList(UssClasses.ListElement.MakeListItem(index));
            var element = root[0];

            if (null == element)
                return;

            VisualElement toRemoveParent;
            VisualElement contextMenuParent;

            if (element is Foldout foldout)
            {
                foldout.AddToClassList(UssClasses.ListElement.Item);
                var toggle = foldout.Q<Toggle>();
                toggle.AddToClassList(UssClasses.ListElement.ItemFoldout);
                contextMenuParent = foldout.Q<VisualElement>(className: UssClasses.Unity.ToggleInput);

                toRemoveParent = toggle;
                foldout.contentContainer.AddToClassList(UssClasses.ListElement.ItemContent);
                root.style.flexDirection = new StyleEnum<FlexDirection>(StyleKeyword.Auto);
            }
            else
            {
                toRemoveParent = root;
                contextMenuParent = root.Q<Label>();
                element.AddToClassList(UssClasses.ListElement.ItemNoFoldout);
                root.style.flexDirection = FlexDirection.Row;
            }

            contextMenuParent.AddManipulator(
                new ContextualMenuManipulator(evt =>
                {
                    var list = GetValue();
                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Delete", action => { OnRemoveItem(index); });
                    evt.menu.AppendSeparator();
                    evt.menu.AppendAction("Move Up", action => { Swap(index, index - 1); },
                        list.Count > 1 && index - 1 >= 0
                            ? DropdownMenuAction.Status.Normal
                            : DropdownMenuAction.Status.Disabled);
                    evt.menu.AppendAction("Move Down", action => { Swap(index, index + 1); },
                        list.Count > 1 && index + 1 < list.Count
                            ? DropdownMenuAction.Status.Normal
                            : DropdownMenuAction.Status.Disabled);
                }));

            var button = new Button();
            button.AddToClassList(UssClasses.ListElement.RemoveItemButton);
            button.clickable.clicked += () => { OnRemoveItem(index); };
            toRemoveParent.Add(button);
        }
    }
}