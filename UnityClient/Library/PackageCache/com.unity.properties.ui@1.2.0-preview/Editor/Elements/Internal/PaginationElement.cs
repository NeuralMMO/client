using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class PaginationElement : VisualElement
    {
        readonly List<int> m_PaginationSizes = new List<int> {5, 10, 20};

        readonly Button m_RangeButton;
        readonly IntegerField m_RangeInput;
        readonly VisualElement m_RangeInputRoot;
        readonly RepeatButton m_PreviousButton;
        readonly RepeatButton m_NextButton;
        readonly PopupField<int> m_SizeElement;

        int m_TotalCount;

        public int PaginationSize { get; private set; }
        public int CurrentPage { get; private set; }

        public int TotalCount
        {
            get => m_TotalCount;
            set
            {
                m_TotalCount = value;
                if (EndIndex >= m_TotalCount)
                {
                    GoToLastPage();
                }
            }
        }

        public bool AutoHide { get; set; } = true;
        public bool Enabled { get; set; } = true;

        public int StartIndex => CurrentPage * PaginationSize;

        public int EndIndex => Math.Min(TotalCount, StartIndex + PaginationSize);

        public int FirstPage => 0;
        public int LastPage => TotalCount / PaginationSize - (TotalCount % PaginationSize > 0 ? 0 : 1);

        public void SetPaginationSizes(int[] sizes)
        {
            m_PaginationSizes.Clear();
            m_PaginationSizes.AddRange(sizes);
            if (!m_PaginationSizes.Contains(m_SizeElement.value))
            {
                PaginationSize = m_SizeElement.value = sizes[0];
            }
        }

        public int LowestPaginationSize => m_PaginationSizes[0];

        public event Action OnChanged = delegate { };

        public PaginationElement()
        {
            Resources.Templates.PaginationElement.Clone(this);
            
            var paginationSize = this.Q(className: UssClasses.PaginationElement.PaginationSize);
            m_SizeElement = new PopupField<int>(
                m_PaginationSizes,
                0,
                FormatSelectedValueCallback,
                FormatSelectedValueCallback);
            m_SizeElement.RegisterValueChangedCallback(Callback);

            PaginationSize = m_PaginationSizes[0];
            paginationSize.Add(m_SizeElement);
            

            m_PreviousButton =
                this.Q<RepeatButton>(className: UssClasses.PaginationElement.PreviousPageButton);
            m_PreviousButton.SetAction(NavigateToPreviousPage, 500, 100);

            m_NextButton =
                this.Q<RepeatButton>(className: UssClasses.PaginationElement.NextPageButton);
            m_NextButton.SetAction(NavigateToNextPage, 500, 100);

            m_RangeButton = this.Q<Button>(className: UssClasses.PaginationElement.ElementsRange);
            m_RangeButton.clickable.clicked += OnRangeClicked;
            m_RangeInputRoot = this.Q(className: UssClasses.PaginationElement.RangeInputRoot);
            m_RangeInputRoot.style.display = DisplayStyle.None;

            m_RangeInput =
                this.Q<IntegerField>(className: UssClasses.PaginationElement.RangeInput);
            m_RangeInput.isDelayed = true;
            m_RangeInput.RegisterValueChangedCallback(OnIndexSelected);
            m_RangeInput.Q(className: UssClasses.Unity.BaseFieldInput).RegisterCallback<FocusOutEvent>(Callback);
            RebindRange();
        }

        void Callback(FocusOutEvent evt)
        {
            m_RangeInputRoot.Hide();;
            m_RangeButton.Show();
        }

        void OnIndexSelected(ChangeEvent<int> evt)
        {
            if (evt.newValue <= 0)
            {
                GoToFirstPage();
            }
            else if (evt.newValue >= TotalCount)
            {
                GoToLastPage();
            }
            else
            {
                GoToPage(evt.newValue / PaginationSize);
            }

            m_RangeInputRoot.Hide();
            m_RangeButton.Show();
        }

        void OnRangeClicked()
        {
            m_RangeButton.style.display = DisplayStyle.None;
            m_RangeInput.SetValueWithoutNotify(StartIndex + 1);
            m_RangeInputRoot.Show();
            m_RangeInput.Q(className: UssClasses.Unity.BaseFieldInput).Focus();
        }

        public void GoToFirstPage()
        {
            GoToPage(FirstPage);
        }

        public void GoToLastPage()
        {
            GoToPage(LastPage);
        }

        void NavigateToPreviousPage()
        {
            CurrentPage = Math.Max(CurrentPage - 1, FirstPage);
            OnChanged();
            RebindRange();
        }

        void NavigateToNextPage()
        {
            CurrentPage = Math.Min(CurrentPage + 1, LastPage);
            OnChanged();
            RebindRange();
        }

        public void GoToPage(int page)
        {
            if (CurrentPage == page)
                return;

            if (page < FirstPage || page > LastPage)
                return;

            CurrentPage = page;
            OnChanged();
            Update();
        }

        void Callback(ChangeEvent<int> evt)
        {
            SetPaginationSize(evt.newValue);
            OnChanged();
            RebindRange();
        }

        public void SetPaginationSize(int size)
        {
            if (PaginationSize == size)
                return;

            var startIndex = StartIndex;
            PaginationSize = size;
            GoToPage(startIndex / PaginationSize);
        }

        public void Update(int totalCount)
        {
            TotalCount = totalCount;
            Update();
        }

        void Update()
        {
            if (!Enabled)
            {
                this.Hide();
                return;
            }
            else
            {
                style.display = TotalCount > LowestPaginationSize || !AutoHide ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RebindRange();
            m_SizeElement.SetValueWithoutNotify(PaginationSize);
            m_PreviousButton.SetEnabledSmart(CurrentPage != FirstPage);
            m_NextButton.SetEnabledSmart(CurrentPage != LastPage && LastPage >= 0);
        }

        void RebindRange()
        {
            if (TotalCount > 0)
            {
                m_RangeButton.text = $"{StartIndex + 1} to {EndIndex}";
            }
            else
            {
                m_RangeButton.text = "0 to 0";
            }
        }

        string FormatSelectedValueCallback(int count)
        {
            return count <= 0 ? $"All ({TotalCount})" : count.ToString();
        }
    }
}