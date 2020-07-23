using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Entities.Editor
{
    class EntityHierarchyWindow : DOTSEditorWindow
    {
        static readonly string k_WindowName = L10n.Tr("Entities");
        static readonly Vector2 k_MinWindowSize = new Vector2(600, 300);

        IEntityHierarchyGroupingStrategy m_Strategy;
        EntityHierarchyTreeView m_TreeView;

        [MenuItem(Constants.MenuItems.EntityHierarchyWindow, false, Constants.MenuItems.WindowPriority)]
        static void OpenWindow() => GetWindow<EntityHierarchyWindow>().Show();

        void OnEnable()
        {
            titleContent = new GUIContent(k_WindowName, EditorIcons.EntityGroup);
            minSize = k_MinWindowSize;

            Resources.Templates.CommonResources.AddStyles(rootVisualElement);
            Resources.Templates.DotsEditorCommon.AddStyles(rootVisualElement);
            rootVisualElement.AddToClassList(UssClasses.Resources.EntityHierarchy);

            var world = GetCurrentlySelectedWorld();
            if (world != null)
            {
                m_Strategy = new EntityHierarchyDefaultGroupingStrategy(world);
                EntityHierarchyDiffSystem.RegisterStrategy(m_Strategy);
            }

            CreateToolbar();
            CreateTreeView();
            RefreshTreeView();
        }

        void OnDisable()
        {
            m_TreeView.Dispose();
            if (m_Strategy != null)
            {
                EntityHierarchyDiffSystem.UnregisterStrategy(m_Strategy);
                m_Strategy.Dispose();
            }
        }

        void CreateToolbar()
        {
            Resources.Templates.EntityHierarchyToolbar.Clone(rootVisualElement);
            var leftSide = rootVisualElement.Q<VisualElement>(className: UssClasses.EntityHierarchyWindow.Toolbar.LeftSide);
            var rightSide = rootVisualElement.Q<VisualElement>(className: UssClasses.EntityHierarchyWindow.Toolbar.RightSide);

            leftSide.Add(CreateWorldSelector());

            AddSearchIcon(rightSide, UssClasses.DotsEditorCommon.SearchIcon);
            AddSearchFieldContainer(rootVisualElement, UssClasses.DotsEditorCommon.SearchFieldContainer);
        }

        void CreateTreeView()
        {
            m_TreeView = new EntityHierarchyTreeView();
            rootVisualElement.Add(m_TreeView);
        }

        void RefreshTreeView() => m_TreeView?.Refresh(m_Strategy);

        protected override void OnWorldSelected(World world)
        {
            if (world == m_Strategy?.World)
                return;

            // Maybe keep the previous strategy to keep its state
            // and reuse it when switching back to it.
            if (m_Strategy != null)
            {
                EntityHierarchyDiffSystem.UnregisterStrategy(m_Strategy);
                m_Strategy.Dispose();
            }

            m_Strategy = new EntityHierarchyDefaultGroupingStrategy(world);
            EntityHierarchyDiffSystem.RegisterStrategy(m_Strategy);

            RefreshTreeView();
        }

        protected override void OnFilterChanged(string filter) {}

        protected override void OnUpdate() {}
    }
}
