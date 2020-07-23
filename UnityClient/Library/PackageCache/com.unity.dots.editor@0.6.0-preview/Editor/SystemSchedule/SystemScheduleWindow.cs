using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Entities.Editor
{
    class SystemScheduleWindow : DOTSEditorWindow
    {
        static readonly string k_WindowName = L10n.Tr("Systems");
        static readonly string k_ShowFullPlayerLoopString = L10n.Tr("Show Full Player Loop");
        static readonly string k_ShowInactiveSystemsString = L10n.Tr("Show Inactive Systems");
        static readonly Vector2 k_MinWindowSize = new Vector2(600, 300);

        static World CurrentWorld { get; set; }

        SystemScheduleTreeView m_SystemTreeView;
        ToolbarMenu m_WorldMenu;

        // To get information after domain reload.
        const string k_StateKey = nameof(SystemScheduleWindow) + "." + nameof(State);

        /// <summary>
        /// Helper container to store session state data.
        /// </summary>
        class State
        {
            /// <summary>
            /// This field controls the showing of full player loop state.
            /// </summary>
            public bool ShowFullPlayerLoop;

            /// <summary>
            /// This field controls the showing of inactive system state.
            /// </summary>
            public bool ShowInactiveSystems;
        }

        /// <summary>
        /// State data for <see cref="SystemScheduleWindow"/>. This data is persisted between domain reloads.
        /// </summary>
        State m_State;

        [MenuItem(Constants.MenuItems.SystemScheduleWindow, false, Constants.MenuItems.WindowPriority)]
        static void OpenWindow()
        {
            var window = GetWindow<SystemScheduleWindow>();
            window.Show();
        }

        /// <summary>
        /// Build the GUI for the system window.
        /// </summary>
        public void OnEnable()
        {
            titleContent = EditorGUIUtility.TrTextContent(k_WindowName, EditorIcons.System);
            minSize = k_MinWindowSize;

            m_State = SessionState<State>.GetOrCreateState(k_StateKey);

            Resources.Templates.SystemSchedule.AddStyles(rootVisualElement);
            Resources.Templates.DotsEditorCommon.AddStyles(rootVisualElement);

            CreateToolBar(rootVisualElement);
            CreateTreeViewHeader(rootVisualElement);
            CreateTreeView(rootVisualElement);

            if (World.All.Count > 0)
                BuildAll();

            PlayerLoopSystemGraph.OnGraphChanged += BuildAll;
            SystemDetailsVisualElement.OnAddComponentType += OnAddComponentType;
            SystemDetailsVisualElement.OnRemoveComponentType += OnRemoveComponentType;
        }

        void OnDisable()
        {
            PlayerLoopSystemGraph.OnGraphChanged -= BuildAll;
            SystemDetailsVisualElement.OnAddComponentType -= OnAddComponentType;
            SystemDetailsVisualElement.OnRemoveComponentType -= OnRemoveComponentType;
        }

        void CreateToolBar(VisualElement root)
        {
            var toolbar = new Toolbar();
            toolbar.AddToClassList(UssClasses.SystemScheduleWindow.ToolbarContainer);
            root.Add(toolbar);

            m_WorldMenu = CreateWorldSelector();
            toolbar.Add(m_WorldMenu);

            var rightSideContainer = new VisualElement();
            rightSideContainer.AddToClassList(UssClasses.SystemScheduleWindow.ToolbarRightSideContainer);

            AddSearchIcon(rightSideContainer, UssClasses.DotsEditorCommon.SearchIcon);
            AddSearchFieldContainer(root, UssClasses.DotsEditorCommon.SearchFieldContainer);

            var dropDownSettings = CreateDropDownSettings(UssClasses.DotsEditorCommon.SettingsIcon);
            UpdateDropDownSettings(dropDownSettings);
            rightSideContainer.Add(dropDownSettings);

            toolbar.Add(rightSideContainer);
        }

        void UpdateDropDownSettings(ToolbarMenu dropdownSettings)
        {
            var menu = dropdownSettings.menu;

            menu.AppendAction(k_ShowFullPlayerLoopString, a =>
            {
                m_State.ShowFullPlayerLoop = !m_State.ShowFullPlayerLoop;

                if (World.All.Count > 0)
                    BuildAll();
            }, a => m_State.ShowFullPlayerLoop ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            menu.AppendAction(k_ShowInactiveSystemsString, a =>
            {
                m_State.ShowInactiveSystems = !m_State.ShowInactiveSystems;
                if (World.All.Count > 0)
                    BuildAll();
            }, a => m_State.ShowInactiveSystems ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        }

        // Manually create header for the tree view.
        void CreateTreeViewHeader(VisualElement root)
        {
            var systemTreeViewHeader = new Toolbar();
            systemTreeViewHeader.AddToClassList(UssClasses.SystemScheduleWindow.TreeView.Header);

            var systemHeaderLabel = new Label("Systems");
            systemHeaderLabel.AddToClassList(UssClasses.SystemScheduleWindow.TreeView.System);

            var entityHeaderLabel = new Label("Matches")
            {
                tooltip = "The number of entities that match the queries at the end of the frame."
            };
            entityHeaderLabel.AddToClassList(UssClasses.SystemScheduleWindow.TreeView.Matches);

            var timeHeaderLabel = new Label("Time (ms)")
            {
                tooltip = "Average running time."
            };
            timeHeaderLabel.AddToClassList(UssClasses.SystemScheduleWindow.TreeView.Time);

            systemTreeViewHeader.Add(systemHeaderLabel);
            systemTreeViewHeader.Add(entityHeaderLabel);
            systemTreeViewHeader.Add(timeHeaderLabel);

            root.Add(systemTreeViewHeader);
        }

        void CreateTreeView(VisualElement root)
        {
            m_SystemTreeView = new SystemScheduleTreeView();
            m_SystemTreeView.style.flexGrow = 1;
            m_SystemTreeView.SearchFilter = SearchFilter;
            root.Add(m_SystemTreeView);
        }

        void BuildAll()
        {
            CurrentWorld = !m_State.ShowFullPlayerLoop ? GetCurrentlySelectedWorld() : null;
            m_SystemTreeView.Refresh(CurrentWorld, m_State.ShowInactiveSystems);
        }

        protected override void OnUpdate()
        {
            if (m_State.ShowFullPlayerLoop)
            {
                var menu = m_WorldMenu.menu;
                var menuItemsCount = menu.MenuItems().Count;

                for (var i = 0; i < menuItemsCount; i++)
                {
                    menu.RemoveItemAt(0);
                }

                m_WorldMenu.text = k_ShowFullPlayerLoopString;
            }

            if (GetCurrentlySelectedWorld() == null)
                return;

            UpdateTimings();
        }

        int m_LastTimedFrame;

        void UpdateTimings()
        {
            if (Time.frameCount == m_LastTimedFrame)
                return;

            var data = PlayerLoopSystemGraph.Current;
            foreach (var recorder in data.RecordersBySystem.Values)
            {
                recorder.Update();
            }

            m_LastTimedFrame = Time.frameCount;
        }

        protected override void OnWorldSelected(World world)
        {
            BuildAll();
        }

        void OnAddComponentType(string componentTypeName)
        {
            if (!string.IsNullOrEmpty(SearchFilter)
                && SearchFilter.IndexOf(componentTypeName, StringComparison.OrdinalIgnoreCase) >= 0)
                return;

            SearchFilter += string.IsNullOrEmpty(SearchFilter)
                ? componentTypeName + " "
                : " " + componentTypeName + " ";
        }

        void OnRemoveComponentType(string componentTypeName)
        {
            if (string.IsNullOrEmpty(SearchFilter))
                return;

            var found = SearchFilter.IndexOf(componentTypeName, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
                return;

            SearchFilter = SearchFilter.Remove(found, componentTypeName.Length).Trim();
        }

        protected override void OnFilterChanged(string filter)
        {
            SearchFilter = filter;
            m_SystemTreeView.SearchFilter = SearchFilter;
            BuildAll();
        }
    }
}
