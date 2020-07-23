using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.QuickSearch
{
    enum ProjectSize
    {
        Small,
        Medium,
        Large
    }

    enum IndexToCreateType
    {
        Minimal,
        Default,
        Extended
    }

    enum IndexOptions
    {
        None,
        Dependencies,
        PropertiesAndDependencies
    }

    struct IndexCreationInfo
    {
        internal IndexToCreateType type { get; private set; }
        internal IndexOptions optionsToAdd;

        public IndexCreationInfo(IndexToCreateType type)
        {
            this.type = type;
            optionsToAdd = IndexOptions.None;
            if (type == IndexToCreateType.Extended)
                optionsToAdd = IndexOptions.PropertiesAndDependencies;
        }

        public IndexCreationInfo(IndexToCreateType type, IndexOptions optionsToAdd)
        {
            this.type = type;
            this.optionsToAdd = optionsToAdd;
        }

        internal string text
        {
            get
            {
                if (type == IndexToCreateType.Minimal)
                    return "Enable a minimal set of indexing options.";
                else
                {
                    string result = "Index all the assets of your project with:\n\n - File information\n - Type information";
                    if (optionsToAdd == IndexOptions.Dependencies)
                        return result + "\n - Asset dependencies";
                    else if (optionsToAdd == IndexOptions.PropertiesAndDependencies)
                        return result + "\n - Asset properties\n - Asset dependencies";
                    else
                        return result;
                }
            }
        }
    }

    class OnBoardingWindow : EditorWindow
    {
        const string k_RootIndexPath = "Assets/Assets.index";
        static string k_AssetIndexPath = $"{Utils.packageFolderName}/Templates/Assets.index.template";

        [MenuItem("Window/Quick Search/Setup Wizard")]
        public static void OpenWindow()
        {
            var window = CreateWindow<OnBoardingWindow>();
            var windowSize = new Vector2(600f, 420f);
            window.minSize = window.maxSize = windowSize;
            window.position = Utils.GetMainWindowCenteredPosition(windowSize);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            titleContent.image = Icons.quicksearch;
            titleContent.text = "Quick Search Setup";

            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Utils.GetPackagePath("Editor/StyleSheets/OnBoarding.uss")));
            if (EditorGUIUtility.isProSkin)
                rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Utils.GetPackagePath("Editor/StyleSheets/OnBoarding_Dark.uss")));
            else
                rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Utils.GetPackagePath("Editor/StyleSheets/OnBoarding_Light.uss")));
            rootVisualElement.AddToClassList("index-manager-variables");

            var container = new VisualElement() { name = "Container" };
            rootVisualElement.Add(container);
            container.Add(new Label("Choose the size of your project. This will help select the most suited settings for your project."));

            var containerSizeProject = new VisualElement() { name = "ContainerSizeProject" };
            containerSizeProject.style.flexDirection = FlexDirection.Row;

            var toggleGroupIndex = new List<IndexToggle>();
            var extendedIndexToggle = new IndexToggle("Extended", toggleGroupIndex, new IndexCreationInfo(IndexToCreateType.Extended));
            toggleGroupIndex.Add(new IndexToggle("Minimal", toggleGroupIndex, new IndexCreationInfo(IndexToCreateType.Minimal)));
            toggleGroupIndex.Add(new IndexToggle("Default", toggleGroupIndex, new IndexCreationInfo(IndexToCreateType.Default)) { value = true });
            toggleGroupIndex.Add(extendedIndexToggle);
            var toggleGroup = new List<ProjectSizeToggle>();
            toggleGroup.Add(new ProjectSizeToggle("Small", "A project that contains less than 1000 assets.", toggleGroup, ProjectSize.Small, null));
            toggleGroup.Add(new ProjectSizeToggle("Medium", "Seems reasonable.", toggleGroup, ProjectSize.Medium, null) { value = true });
            toggleGroup.Add(new ProjectSizeToggle("Large", "A project that contains more than 20000 assets.", toggleGroup, ProjectSize.Large, extendedIndexToggle));

            containerSizeProject.Add(toggleGroup[0]);
            containerSizeProject.Add(toggleGroup[1]);
            containerSizeProject.Add(toggleGroup[2]);

            container.Add(containerSizeProject);
            container.Add(new Label("Choose which indexing option you prefer. Indexing your assets will provide better search capabilities. Note that the more options you select, the longer it will take to create your first index."));

            var containerIndex = new VisualElement() { name = "ContainerIndex" };
            containerIndex.style.flexDirection = FlexDirection.Row;
            containerIndex.Add(toggleGroupIndex[0]);
            containerIndex.Add(toggleGroupIndex[1]);
            containerIndex.Add(toggleGroupIndex[2]);

            container.Add(containerIndex);

            var containerButtons = new VisualElement() { name = "ContainerButtons" };
            containerButtons.style.flexDirection = FlexDirection.Row;
            containerButtons.Add(FlexibleSpace());
            containerButtons.Add(new Button(() => OnFinish(toggleGroup, toggleGroupIndex)) { name = "FinishButton", text = "Finish" });
            containerButtons.Add(new Button(Close) { name = "CancelButton", text = "Cancel" });

            container.Add(containerButtons);
        }

        private void OnFinish(List<ProjectSizeToggle> toggleGroup, List<IndexToggle> toggleGroupIndex)
        {
            ProjectSize? projectSize = null;
            foreach (var projectSizeToggle in toggleGroup)
            {
                if (projectSizeToggle.value)
                {
                    projectSize = projectSizeToggle.projectSize;
                    break;
                }
            }

            IndexCreationInfo? indexType = null;
            foreach (var indexTypeToggle in toggleGroupIndex)
            {
                if (indexTypeToggle.value)
                {
                    indexType = indexTypeToggle.indexCreationInfo;
                    break;
                }
            }
            if(!projectSize.HasValue)
            {
                Debug.LogError("Project size was not selected");
                return;
            }
            if (!indexType.HasValue)
            {
                Debug.LogError("Index type was not selected");
                return;
            }

            EditorApplication.delayCall += () => ApplySettings(projectSize.Value, indexType.Value);
            Close();
        }

        private VisualElement FlexibleSpace()
        {
            var space = new VisualElement();
            space.style.flexGrow = 1;
            return space;
        }

        private void ApplySettings(ProjectSize projectSize, IndexCreationInfo indexCreationInfo)
        {
            bool fetchPreview = false;
            bool trackSelection = false;
            bool wantsMore = false;
            switch (projectSize)
            {
                case ProjectSize.Small:
                    wantsMore = true;
                    fetchPreview = true;
                    trackSelection = true;
                    switch (indexCreationInfo.type)
                    {
                        case IndexToCreateType.Minimal:
                            break;
                        case IndexToCreateType.Default:
                        case IndexToCreateType.Extended:
                            GenerateIndex(indexCreationInfo.optionsToAdd);
                            break;
                    }
                    break;
                case ProjectSize.Medium:
                    wantsMore = true;
                    fetchPreview = true;
                    trackSelection = false;
                    switch (indexCreationInfo.type)
                    {
                        case IndexToCreateType.Minimal:
                            break;
                        case IndexToCreateType.Default:
                        case IndexToCreateType.Extended:
                            GenerateIndex(indexCreationInfo.optionsToAdd);
                            break;
                    }
                    break;
                case ProjectSize.Large:
                    fetchPreview = false;
                    trackSelection = false;
                    switch (indexCreationInfo.type)
                    {
                        case IndexToCreateType.Minimal:
                            wantsMore = true;
                            break;
                        case IndexToCreateType.Default:
                        case IndexToCreateType.Extended:
                            wantsMore = false;
                            GenerateIndex(indexCreationInfo.optionsToAdd);
                            break;
                    }
                    break;
            }
            SetSettingsFromProjectSize(fetchPreview, trackSelection, wantsMore);
        }

        private static void SetSettingsFromProjectSize(bool newFetchPreview, bool newTrackSelection, bool newWantsMore)
        {
            SearchSettings.fetchPreview = newFetchPreview;
            SearchSettings.trackSelection = newTrackSelection;
            SearchSettings.wantsMore = newWantsMore;
            SearchSettings.Save();
        }

        private static void GenerateIndex(IndexOptions optionsToAdd)
        {
            var indexSettings = IndexManager.ExtractIndexFromFile(k_AssetIndexPath);
            if (optionsToAdd == IndexOptions.Dependencies)
                indexSettings.options.dependencies = true;
            else if (optionsToAdd == IndexOptions.PropertiesAndDependencies)
            {
                indexSettings.options.properties = true;
                indexSettings.options.dependencies = true;
            }
            try
            {
                var json = JsonUtility.ToJson(indexSettings, true);
                File.WriteAllText(k_RootIndexPath, json);
                AssetDatabase.ImportAsset(k_RootIndexPath, ImportAssetOptions.ForceSynchronousImport);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        abstract class ToggleWithTitleAndDescription : ToolbarToggle
        {
            protected Label m_TitleLabel;
            protected Label m_DescriptionLabel;
            protected ToggleWithTitleAndDescription(string title, string description, IEnumerable<ToggleWithTitleAndDescription> toggleGroup) : base()
            {
                this.AddToClassList("ToggleWithTitleAndDescription");
                var toggle = this.Children().First();
                m_TitleLabel = new Label(title) { name = "TitleLabel" };
                toggle.Add(m_TitleLabel);
                m_DescriptionLabel = new Label(description) { name = "DescriptionLabel" };
                toggle.Add(m_DescriptionLabel);

                this.RegisterValueChangedCallback(
                evt =>
                {
                    OnValueChangedCallback(evt);
                    if (this.value)
                    {
                        foreach (var item in toggleGroup)
                        {
                            if (item != this)
                                item.value = false;
                        }
                    }
                    else
                    {
                        foreach (var item in toggleGroup)
                        {
                            if (item.value)
                            {
                                return;
                            }
                        }
                        this.value = true;
                    }
                });
            }

            protected virtual void OnValueChangedCallback(ChangeEvent<bool> evt)
            {
            }
        }

        class IndexToggle : ToggleWithTitleAndDescription
        {
            IndexCreationInfo m_IndexCreationInfo;
            internal IndexCreationInfo indexCreationInfo
            {
            get { return m_IndexCreationInfo; }
            set {
                    m_IndexCreationInfo = value;
                    m_DescriptionLabel.text = value.text;
                }
            }
            public IndexToggle(string title, IEnumerable<ToggleWithTitleAndDescription> toggleGroup, IndexCreationInfo indexCreationInfo) : base(title, indexCreationInfo.text, toggleGroup)
            {
                this.indexCreationInfo = indexCreationInfo;
            }
        }

        class ProjectSizeToggle : ToggleWithTitleAndDescription
        {
            IndexToggle m_ExtendedIndexToggle;
            internal ProjectSize projectSize { get; private set; }
            public ProjectSizeToggle(string title, string description, IEnumerable<ToggleWithTitleAndDescription> toggleGroup, ProjectSize projectSize, IndexToggle extendedIndexToggle) : base(title, description, toggleGroup)
            {
                this.m_ExtendedIndexToggle = extendedIndexToggle;
                this.projectSize = projectSize;
            }
            protected override void OnValueChangedCallback(ChangeEvent<bool> evt)
            {
                if (projectSize == ProjectSize.Large)
                {
                    if (evt.newValue)
                        m_ExtendedIndexToggle.indexCreationInfo = new IndexCreationInfo(m_ExtendedIndexToggle.indexCreationInfo.type, IndexOptions.Dependencies);
                    else
                        m_ExtendedIndexToggle.indexCreationInfo = new IndexCreationInfo(m_ExtendedIndexToggle.indexCreationInfo.type, IndexOptions.PropertiesAndDependencies);
                }
            }
        }
    }
}
