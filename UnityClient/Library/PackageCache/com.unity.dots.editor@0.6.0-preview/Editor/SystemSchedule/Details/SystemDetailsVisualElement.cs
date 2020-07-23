using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Entities.Editor
{
    class SystemDetailsVisualElement : VisualElement
    {
        static readonly string k_QueriesTitle = L10n.Tr("Queries");
        static readonly string k_QueriesMatchTitle = L10n.Tr("Matches");
        static readonly string k_SchedulingTitle = L10n.Tr("Scheduling");
        static readonly string k_ShowDependencies = L10n.Tr("Show Dependencies");
        static readonly string k_ShowLess = L10n.Tr("Show less");

        const string k_ComponentToken = "c:";
        const string k_ScriptType = " t:Script";
        const int k_ShowMinimumQueryCount = 2;

        EntityQuery[] m_Query;
        string m_SearchFilter;
        VisualElement m_SystemIcon;
        Label m_SystemNameLabel;
        VisualElement m_ScriptIcon;
        VisualElement m_AllQueryResultContainer;
        bool m_ShowMoreBool = true;

        public VisualElement Parent { get; set; }
        public static event Action<string> OnAddComponentType;
        public static event Action<string> OnRemoveComponentType;

        SystemTreeViewItem m_Target;
        public SystemTreeViewItem Target
        {
            get => m_Target;

            set
            {
                if (m_Target == value)
                    return;

                m_Target = value;

                UpdateContent();
            }
        }

        SystemTreeViewItem m_LastSelectedItem;
        public SystemTreeViewItem LastSelectedItem
        {
            get => m_LastSelectedItem;

            set { m_LastSelectedItem = value; }
        }

        public string SearchFilter
        {
            get => m_SearchFilter;
            set
            {
                if (m_SearchFilter == value)
                    return;

                m_SearchFilter = value;

                m_Query = null;

                UpdateQueryResults();
            }
        }

        public SystemDetailsVisualElement()
        {
            Resources.Templates.CommonResources.AddStyles(this);
            Resources.Templates.SystemScheduleDetailContent.Clone(this);

            CreateToolBarForDetailSection();
            CreateQueryResultSection();
            CreateScheduleFilterSection();
        }

        void UpdateContent()
        {
            if (null == Target)
                return;

            if (Target.System != null && Target.System.World == null)
                return;

            switch (Target.System)
            {
                case null:
                case ComponentSystemGroup _:
                {
                    if ((null != Parent) && Parent.Contains(this))
                        Parent.Remove(this);

                    return;
                }
            }

            UpdateSystemIconName();
            UpdateQueryResults();
        }

        void UpdateSystemIconName()
        {
            m_SystemIcon.AddToClassList(GetDetailSystemClass(Target.System));

            var systemName = Target.System.GetType().Name;
            m_SystemNameLabel.text = systemName;

            var scriptFound = SearchForScript(systemName);
            if (scriptFound)
            {
                m_ScriptIcon.visible = true;
                m_ScriptIcon.RegisterCallback<MouseUpEvent, UnityEngine.Object>((evt, payload) => AssetDatabase.OpenAsset(payload), scriptFound);
            }
            else
            {
                m_ScriptIcon.visible = false;
            }
        }

        void UpdateQueryResults()
        {
            if (Target?.System == null)
                return;

            var currentQueries = Target.System.EntityQueries;
            if (m_Query == currentQueries)
                return;

            m_AllQueryResultContainer.Clear();
            m_Query = currentQueries;

            var index = 0;
            var toAddList = new List<VisualElement>();

            // Query result for each row.
            foreach (var query in m_Query)
            {
                var eachRowContainer = new VisualElement();
                Resources.Templates.CommonResources.AddStyles(eachRowContainer);
                Resources.Templates.SystemScheduleDetailQuery.Clone(eachRowContainer);

                // Sort the components by their access mode, readonly, readwrite, etc.
                using (var queryTypePooledList = query.GetQueryTypes().ToPooledList())
                {
                    var queryTypeList = queryTypePooledList.List;

                    queryTypeList.Sort(EntityQueryUtility.CompareTypes);

                    // Icon container
                    var queryIcon = eachRowContainer.Q(className: UssClasses.SystemScheduleWindow.Detail.QueryIconName);
                    queryIcon.style.flexShrink = 1;
                    queryIcon.AddToClassList(UssClasses.SystemScheduleWindow.Detail.QueryIcon);

                    var allComponentContainer = eachRowContainer.Q(className: UssClasses.SystemScheduleWindow.Detail.AllComponentContainer);
                    foreach (var queryType in queryTypeList)
                    {
                        var componentManagedType = queryType.GetManagedType();
                        var componentTypeName = EntityQueryUtility.SpecifiedTypeName(componentManagedType);

                        // Component toggle container.
                        var componentTypeNameToggleContainer = new ComponentToggleWithAccessMode(queryType);
                        var componentTypeNameToggle = componentTypeNameToggleContainer.ComponentTypeNameToggle;

                        componentTypeNameToggle.text = componentTypeName;
                        componentTypeNameToggle.value = SearchFieldContainsComponent(componentTypeName);
                        componentTypeNameToggle.RegisterValueChangedCallback(evt =>
                        {
                            HandleComponentsAddRemoveEvents(evt, componentTypeNameToggle, componentTypeName);
                        });
                        allComponentContainer.Add(componentTypeNameToggleContainer);
                    }
                }

                // Entity match label
                var matchCountContainer = eachRowContainer.Q(className: UssClasses.SystemScheduleWindow.Detail.EntityMatchCountContainer);
                var matchCountLabel = new EntityMatchCountVisualElement { Query = query, CurrentWorld = Target.System.World};
                matchCountContainer.Add(matchCountLabel);

                // Show more to unfold the results or less to fold.
                if (index < k_ShowMinimumQueryCount)
                {
                    m_AllQueryResultContainer.Add(eachRowContainer);
                }
                else
                {
                    toAddList.Add(eachRowContainer);
                }

                index++;
            }

            var queryHideCount = m_Query.Length - k_ShowMinimumQueryCount;
            if (toAddList.Any())
            {
                FoldPartOfResults(m_AllQueryResultContainer, toAddList, queryHideCount);
            }
        }

        void FoldPartOfResults(VisualElement allQueryResultContainer, IReadOnlyCollection<VisualElement> toAddList, int queryHideCount)
        {
            var showMoreLessLabel = new CustomLabelWithUnderline();
            showMoreLessLabel.AddToClassList(UssClasses.SystemScheduleWindow.Detail.ShowMoreLessLabel);

            var showMoreText = queryHideCount > 1
                ? $"Show {queryHideCount.ToString()} more queries"
                : $"Show {queryHideCount.ToString()} more query";

            allQueryResultContainer.Add(showMoreLessLabel);

            ShowMoreOrLess(showMoreLessLabel, showMoreText, toAddList, allQueryResultContainer);

            showMoreLessLabel.RegisterCallback<MouseUpEvent>(evt =>
            {
                m_ShowMoreBool = !m_ShowMoreBool;

                ShowMoreOrLess(showMoreLessLabel, showMoreText, toAddList, allQueryResultContainer);
            });
        }

        void ShowMoreOrLess(CustomLabelWithUnderline showMoreLessLabel, string showMoreText,
            IReadOnlyCollection<VisualElement> toAddList, VisualElement allQueryResultContainer)
        {
            if (m_ShowMoreBool)
            {
                showMoreLessLabel.text = showMoreText;
                foreach (var eachRow in toAddList)
                {
                    if (allQueryResultContainer.Contains(eachRow))
                    {
                        allQueryResultContainer.Remove(eachRow);
                    }
                }
            }
            else
            {
                showMoreLessLabel.text = k_ShowLess;
                var index = allQueryResultContainer.IndexOf(showMoreLessLabel);
                foreach (var eachRow in toAddList)
                {
                    if (!allQueryResultContainer.Contains(eachRow))
                    {
                        allQueryResultContainer.Insert(index - 1, eachRow);
                    }
                }
            }
        }

        void HandleComponentsAddRemoveEvents(ChangeEvent<bool> evt, CustomToolbarToggle componentTypeNameToggle, string componentTypeName)
        {
            componentTypeNameToggle.value = evt.newValue;

            var searchString = k_ComponentToken + componentTypeName;
            if (componentTypeNameToggle.value)
            {
                OnAddComponentType?.Invoke(searchString);
            }
            else
            {
                OnRemoveComponentType?.Invoke(searchString);
            }
        }

        bool SearchFieldContainsComponent(string componentTypeName)
        {
            if (string.IsNullOrEmpty(m_SearchFilter))
                return false;

            using (var stringList = SystemTreeViewItem.SplitSearchString(m_SearchFilter).ToPooledList())
            {
                foreach (var singleString in stringList.List)
                {
                    if (singleString.StartsWith(k_ComponentToken, StringComparison.OrdinalIgnoreCase))
                    {
                        if (singleString.ToLower() == k_ComponentToken + componentTypeName.ToLower())
                            return true;
                    }
                }
            }

            return false;
        }

        static string GetDetailSystemClass(ComponentSystemBase system)
        {
            switch (system)
            {
                case null:
                    return "";
                case EntityCommandBufferSystem _:
                    return UssClasses.SystemScheduleWindow.Detail.CommandBufferIcon;
                case ComponentSystemBase _:
                    return UssClasses.SystemScheduleWindow.Detail.SystemIcon;
            }
        }

        void CreateToolBarForDetailSection()
        {
            var systemDetailToolbar = new Toolbar();

            Resources.Templates.CommonResources.AddStyles(systemDetailToolbar);
            Resources.Templates.SystemScheduleDetailHeader.Clone(systemDetailToolbar);

            systemDetailToolbar.style.justifyContent = Justify.SpaceBetween;

            // Left side
            m_SystemIcon = systemDetailToolbar.Q(className: UssClasses.SystemScheduleWindow.Detail.SystemIconName);
            m_SystemNameLabel = systemDetailToolbar.Q<Label>(className: UssClasses.SystemScheduleWindow.Detail.SystemNameLabel);

            // Right side
            m_ScriptIcon = systemDetailToolbar.Q(className: UssClasses.SystemScheduleWindow.Detail.ScriptsIconName);
            m_ScriptIcon.AddToClassList(UssClasses.SystemScheduleWindow.Detail.ScriptsIcon);

            var closeIcon = systemDetailToolbar.Q(className: UssClasses.SystemScheduleWindow.Detail.CloseIconName);
            closeIcon.AddToClassList(UssClasses.SystemScheduleWindow.Detail.CloseIcon);
            closeIcon.RegisterCallback<MouseUpEvent>(evt =>
            {
                Parent.Remove(this);
            });

            this.Insert(0, systemDetailToolbar);
        }

        UnityEngine.Object SearchForScript(string systemName)
        {
            var assets = AssetDatabase.FindAssets(systemName + k_ScriptType);
            return assets.Select(asset => AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(asset))).FirstOrDefault(a => a.name == systemName);
        }

        void CreateQueryResultSection()
        {
            var queryTitleLabel = this.Q<Label>(className: UssClasses.SystemScheduleWindow.Detail.QueryTitleLabel);
            queryTitleLabel.text = k_QueriesTitle;

            var matchTitleLabel = this.Q<Label>(className: UssClasses.SystemScheduleWindow.Detail.MatchTitleLabel);
            matchTitleLabel.text = k_QueriesMatchTitle;

            m_AllQueryResultContainer = this.Q(className: UssClasses.SystemScheduleWindow.Detail.QueryRow2);
        }

        void CreateScheduleFilterSection()
        {
            var schedulingTitle = this.Q<Label>(className: UssClasses.SystemScheduleWindow.Detail.SchedulingTitle);
            schedulingTitle.text = k_SchedulingTitle;

            var schedulingToggle = this.Q<ToolbarToggle>(className: UssClasses.SystemScheduleWindow.Detail.SchedulingToggle);
            schedulingToggle.text = k_ShowDependencies;
        }
    }
}
