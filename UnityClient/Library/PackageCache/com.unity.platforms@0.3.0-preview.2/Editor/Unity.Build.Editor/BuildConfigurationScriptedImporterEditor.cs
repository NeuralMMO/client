using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties.Editor;
using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    using BuildConfigurationElement = HierarchicalComponentContainerElement<BuildConfiguration, IBuildComponent, IBuildComponent>;

    [CustomEditor(typeof(BuildConfigurationScriptedImporter))]
    internal sealed class BuildConfigurationScriptedImporterEditor : ScriptedImporterEditor
    {
        static class ClassNames
        {
            public const string BaseClassName = nameof(BuildConfiguration);
            public const string Dependencies = BaseClassName + "__asset-dependencies";
            public const string Header = BaseClassName + "__asset-header";
            public const string HeaderLabel = BaseClassName + "__asset-header-label";
            public const string BuildAction = BaseClassName + "__build-action";
            public const string BuildDropdown = BaseClassName + "__build-dropdown";
            public const string AddComponent = BaseClassName + "__add-component-button";
            public const string OptionalComponents = BaseClassName + "__optional-components";
        }

        internal struct BuildAction : IEquatable<BuildAction>
        {
            public string Name;
            public Action<BuildConfiguration> Action;

            public bool Equals(BuildAction other)
            {
                return Name == other.Name;
            }
        }

        internal static readonly BuildAction s_BuildAction = new BuildAction
        {
            Name = "Build",
            Action = (config) =>
            {
                if (config != null && config)
                {
                    config.Build()?.LogResult();
                }
            }
        };

        internal static readonly BuildAction s_BuildAndRunAction = new BuildAction
        {
            Name = "Build and Run",
            Action = (config) =>
            {
                if (config != null && config)
                {
                    var buildResult = config.Build();
                    buildResult.LogResult();
                    if (buildResult.Failed)
                    {
                        return;
                    }

                    using (var runResult = config.Run())
                    {
                        runResult.LogResult();
                    }
                }
            }
        };

        internal static readonly BuildAction s_RunAction = new BuildAction
        {
            Name = "Run",
            Action = (config) =>
            {
                if (config != null && config)
                {
                    using (var result = config.Run())
                    {
                        result.LogResult();
                    }
                }
            }
        };

        void ExecuteCurrentBuildAction()
        {
            if (HasModified())
            {
                var path = AssetDatabase.GetAssetPath(assetTarget);
                int option = EditorUtility.DisplayDialogComplex("Unapplied import settings",
                    $"Unapplied import settings for '{path}'", "Apply", "Revert", "Cancel");
                switch (option)
                {
                    case 0: // Apply
                        Apply();
                        break;

                    case 1: // Revert
                        ResetValues();
                        break;

                    case 2: // Cancel
                        return;
                }
            }
            EditorApplication.delayCall += () => CurrentBuildAction.Action(assetTarget as BuildConfiguration);
        }

        // Needed because properties don't handle root collections well.
        class DependenciesWrapper
        {
            public List<BuildConfiguration> Dependencies;
        }

        const string k_CurrentActionKey = "BuildAction-CurrentAction";

        bool m_LastEditState;
        BindableElement m_BuildConfigurationRoot;
        readonly DependenciesWrapper m_DependenciesWrapper = new DependenciesWrapper();

        protected override bool needsApplyRevert { get; } = true;
        public override bool showImportedObject { get; } = false;
        internal static BuildAction CurrentBuildAction => s_BuildActions[CurrentActionIndex];

        static List<BuildAction> s_BuildActions { get; } = new List<BuildAction>
        {
            s_BuildAction,
            s_BuildAndRunAction,
            s_RunAction,
        };

        static int CurrentActionIndex
        {
            get => EditorPrefs.HasKey(k_CurrentActionKey) ? EditorPrefs.GetInt(k_CurrentActionKey) : s_BuildActions.IndexOf(s_BuildAndRunAction);
            set => EditorPrefs.SetInt(k_CurrentActionKey, value);
        }

        protected override Type extraDataType => typeof(BuildConfiguration);

        protected override void InitializeExtraDataInstance(UnityEngine.Object extraData, int targetIndex)
        {
            var target = targets[targetIndex];
            if (target == null || !target)
            {
                return;
            }

            var assetImporter = target as AssetImporter;
            if (assetImporter == null || !assetImporter)
            {
                return;
            }

            var config = extraData as BuildConfiguration;
            if (config == null || !config)
            {
                return;
            }

            if (BuildConfiguration.DeserializeFromPath(config, assetImporter.assetPath))
            {
                config.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
            }
        }

        protected override void OnHeaderGUI()
        {
            // Intentional
            //base.OnHeaderGUI();
        }

        protected override void Apply()
        {
            base.Apply();
            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (target == null || !target)
                {
                    continue;
                }

                var assetImporter = target as AssetImporter;
                if (assetImporter == null || !assetImporter)
                {
                    continue;
                }

                var config = extraDataTargets[i] as BuildConfiguration;
                if (config == null || !config)
                {
                    continue;
                }

                config.SerializeToPath(assetImporter.assetPath);
            }
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            for (int i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                if (target == null || !target)
                {
                    continue;
                }

                var assetImporter = target as AssetImporter;
                if (assetImporter == null || !assetImporter)
                {
                    continue;
                }

                var config = extraDataTargets[i] as BuildConfiguration;
                if (config == null || !config)
                {
                    continue;
                }

                if (BuildConfiguration.DeserializeFromPath(config, assetImporter.assetPath))
                {
                    config.name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
                }
            }
            Refresh();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            m_BuildConfigurationRoot = new BindableElement();
            m_BuildConfigurationRoot.AddStyleSheetAndVariant(ClassNames.BaseClassName);

            Refresh(m_BuildConfigurationRoot);

            root.contentContainer.Add(m_BuildConfigurationRoot);
            root.contentContainer.Add(new IMGUIContainer(ApplyRevertGUI));
            return root;
        }

        void Refresh()
        {
            if (m_BuildConfigurationRoot != null)
            {
                Refresh(m_BuildConfigurationRoot);
            }
        }

        void Refresh(BindableElement root)
        {
            root.Clear();

            var asset = assetTarget as BuildConfiguration;
            if (asset == null)
            {
                return;
            }

            m_LastEditState = AssetDatabase.IsOpenForEdit(asset);
            var openedForEditUpdater = UIUpdaters.MakeBinding(asset, root);
            openedForEditUpdater.OnPreUpdate += updater =>
            {
                if (!updater.Source)
                {
                    return;
                }
                m_LastEditState = AssetDatabase.IsOpenForEdit(updater.Source);
            };
            root.binding = openedForEditUpdater;

            var config = extraDataTarget as BuildConfiguration;
            if (config == null)
            {
                return;
            }

            RefreshHeader(root, config);
            RefreshDependencies(root, config);
            RefreshComponents(root, config);
        }

        void RefreshHeader(BindableElement root, BuildConfiguration config)
        {
            var headerRoot = new VisualElement();
            headerRoot.AddToClassList(ClassNames.Header);
            root.Add(headerRoot);

            // Refresh Name Label
            var nameLabel = new Label(config.name);
            nameLabel.AddToClassList(ClassNames.HeaderLabel);
            headerRoot.Add(nameLabel);

            var labelUpdater = UIUpdaters.MakeBinding(config, nameLabel);
            labelUpdater.OnUpdate += (binding) =>
            {
                if (binding.Source != null && binding.Source)
                {
                    binding.Element.text = binding.Source.name;
                }
            };
            nameLabel.binding = labelUpdater;

            // Refresh Build&Run Button
            var dropdownButton = new VisualElement();
            dropdownButton.style.flexDirection = FlexDirection.Row;
            dropdownButton.style.justifyContent = Justify.FlexEnd;
            nameLabel.Add(dropdownButton);

            var dropdownActionButton = new Button { text = s_BuildActions[CurrentActionIndex].Name };
            dropdownActionButton.AddToClassList(ClassNames.BuildAction);
            dropdownActionButton.clickable = new Clickable(ExecuteCurrentBuildAction);
            dropdownActionButton.SetEnabled(true);
            dropdownButton.Add(dropdownActionButton);

            var actionUpdater = UIUpdaters.MakeBinding(this, dropdownActionButton);
            actionUpdater.OnUpdate += (binding) =>
            {
                if (binding.Source != null && binding.Source)
                {
                    binding.Element.text = CurrentBuildAction.Name;
                }
            };
            dropdownActionButton.binding = actionUpdater;

            var dropdownActionPopup = new PopupField<BuildAction>(s_BuildActions, CurrentActionIndex, a => string.Empty, a => a.Name);
            dropdownActionPopup.AddToClassList(ClassNames.BuildDropdown);
            dropdownActionPopup.RegisterValueChangedCallback(evt =>
            {
                CurrentActionIndex = s_BuildActions.IndexOf(evt.newValue);
                dropdownActionButton.clickable = new Clickable(ExecuteCurrentBuildAction);
                actionUpdater.Update();
            });
            dropdownButton.Add(dropdownActionPopup);

            // Refresh Asset Field
            var assetField = new ObjectField { objectType = typeof(BuildConfiguration) };
            assetField.Q<VisualElement>(className: "unity-object-field__selector").SetEnabled(false);
            assetField.SetValueWithoutNotify(config);
            headerRoot.Add(assetField);

            var assetUpdater = UIUpdaters.MakeBinding(config, assetField);
            assetField.SetEnabled(m_LastEditState);
            assetUpdater.OnPreUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            assetField.binding = assetUpdater;
        }

        void RefreshDependencies(BindableElement root, BuildConfiguration config)
        {
#if UNITY_2020_1_OR_NEWER
            m_DependenciesWrapper.Dependencies = FilterDependencies(config, config.Dependencies.Select(d => d.asset)).ToList();
#else
            m_DependenciesWrapper.Dependencies = FilterDependencies(config, config.Dependencies).ToList();
#endif

            var dependencyElement = new PropertyElement();
            dependencyElement.AddToClassList(ClassNames.BaseClassName);
            dependencyElement.SetTarget(m_DependenciesWrapper);
            dependencyElement.OnChanged += (element, path) =>
            {
                config.Dependencies.Clear();
#if UNITY_2020_1_OR_NEWER
                config.Dependencies.AddRange(FilterDependencies(config, m_DependenciesWrapper.Dependencies)
                    .Select(asset => new LazyLoadReference<BuildConfiguration>(asset)));
#else
                config.Dependencies.AddRange(FilterDependencies(config, m_DependenciesWrapper.Dependencies));
#endif
            };
            dependencyElement.SetEnabled(m_LastEditState);
            root.Add(dependencyElement);

            var foldout = dependencyElement.Q<Foldout>();
            foldout.AddToClassList(ClassNames.Dependencies);
            foldout.Q<Toggle>().AddToClassList(BuildConfigurationElement.ClassNames.Header);
            foldout.contentContainer.AddToClassList(BuildConfigurationElement.ClassNames.Fields);

            var dependencyUpdater = UIUpdaters.MakeBinding(config, dependencyElement);
            dependencyUpdater.OnPreUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            dependencyElement.binding = dependencyUpdater;
        }

        IEnumerable<BuildConfiguration> FilterDependencies(BuildConfiguration config, IEnumerable<BuildConfiguration> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                if (dependency == null || !dependency || dependency == config || dependency.HasDependency(config))
                {
                    yield return null;
                }
                else
                {
                    yield return dependency;
                }
            }
        }

        void RefreshComponents(BindableElement root, BuildConfiguration config)
        {
            var componentRoot = new BindableElement();
            componentRoot.SetEnabled(m_LastEditState);
            root.Add(componentRoot);

            var componentUpdater = UIUpdaters.MakeBinding(config, componentRoot);
            componentUpdater.OnUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            componentRoot.binding = componentUpdater;

            // Refresh components
            var components = config.GetComponents();
            foreach (var component in components)
            {
                var componentType = component.GetType();
                if (componentType.HasAttribute<HideInInspector>())
                {
                    continue;
                }
                componentRoot.Add(GetComponentElement(config, component, false));
            }

            // Refresh optional components
            var pipeline = config.GetBuildPipeline();
            if (pipeline != null)
            {
                var optionalComponentsRoot = new Foldout();
                optionalComponentsRoot.AddToClassList(ClassNames.OptionalComponents);
                optionalComponentsRoot.text = "Suggested Components";
                optionalComponentsRoot.value = false;
                optionalComponentsRoot.Q<VisualElement>("unity-content").style.marginLeft = 0;
                componentRoot.Add(optionalComponentsRoot);

                foreach (var type in pipeline.UsedComponents)
                {
                    if (type.IsAbstract || type.IsGenericType || type.IsInterface ||
                        type.HasAttribute<HideInInspector>() || config.HasComponent(type))
                    {
                        continue;
                    }
                    optionalComponentsRoot.Add(GetComponentElement(config, config.GetComponentOrDefault(type), true));
                }
            }

            // Refresh add component button
            var addComponentButton = new Button();
            addComponentButton.AddToClassList(ClassNames.AddComponent);
            addComponentButton.RegisterCallback<MouseUpEvent>(evt =>
            {
                var database = TypeSearcherDatabase.Populate<IBuildComponent>((type) =>
                    !type.HasAttribute<ObsoleteAttribute>() &&
                    !type.HasAttribute<HideInInspector>() &&
                    !config.GetComponentTypes().Contains(type));
                var searcher = new Searcher(database, new AddTypeSearcherAdapter("Add Component"));
                var editorWindow = EditorWindow.focusedWindow;
                var button = evt.target as Button;

                SearcherWindow.Show(editorWindow, searcher, AddType,
                    button.worldBound.min + Vector2.up * 15.0f, a => { },
                    new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top, SearcherWindow.Alignment.Horizontal.Left));
            });
            addComponentButton.SetEnabled(m_LastEditState);
            root.contentContainer.Add(addComponentButton);

            var addComponentButtonUpdater = UIUpdaters.MakeBinding(config, addComponentButton);
            addComponentButtonUpdater.OnPreUpdate += updater => updater.Element.SetEnabled(m_LastEditState);
            addComponentButton.binding = addComponentButtonUpdater;
        }

        bool AddType(SearcherItem arg)
        {
            if (!(arg is TypeSearcherItem typeItem))
            {
                return false;
            }

            var config = extraDataTarget as BuildConfiguration;
            if (config == null)
            {
                return false;
            }

            var type = typeItem.Type;
            config.SetComponent(type, TypeConstruction.Construct<IBuildComponent>(type));
            Refresh(m_BuildConfigurationRoot);
            return true;
        }

        VisualElement GetComponentElement(BuildConfiguration container, object component, bool optional)
        {
            var componentType = component.GetType();
            var element = (VisualElement)Activator.CreateInstance(typeof(HierarchicalComponentContainerElement<,,>)
                .MakeGenericType(typeof(BuildConfiguration), typeof(IBuildComponent), componentType), container, component, optional);
            ((IChangeHandler)element).OnChanged += Refresh;
            return element;
        }
    }
}
