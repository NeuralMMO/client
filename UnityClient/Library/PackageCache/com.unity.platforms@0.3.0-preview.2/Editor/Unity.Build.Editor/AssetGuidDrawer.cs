using JetBrains.Annotations;
using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    abstract class AssetGuidInspectorBase<T> : PropertyDrawer<T, AssetGuidAttribute>
    {
        protected ObjectField m_Field;

        public override VisualElement Build()
        {
            m_Field = new ObjectField(DisplayName)
            {
                allowSceneObjects = false
            };

            var asset = GetAttribute<AssetGuidAttribute>();
            Assert.IsTrue(typeof(UnityEngine.Object).IsAssignableFrom(asset.Type));
            m_Field.objectType = asset.Type;

            m_Field.RegisterValueChangedCallback(OnChanged);
            return m_Field;
        }

        public override void Update()
        {
            OnUpdate();
        }

        protected abstract void OnChanged(ChangeEvent<UnityEngine.Object> evt);
        protected abstract void OnUpdate();
    }

    [UsedImplicitly]
    sealed class GuidAssetInspector : AssetGuidInspectorBase<GUID>
    {
        protected override void OnChanged(ChangeEvent<Object> evt)
        {
            if (null != evt.newValue && evt.newValue)
            {
                Target = new GUID(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(evt.newValue)));
            }
            else
            {
                Target = default;
            }
        }

        protected override void OnUpdate()
        {
            m_Field.SetValueWithoutNotify(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(Target.ToString())));
        }
    }

    [UsedImplicitly]
    sealed class GlobalObjectIdAssetInspector : AssetGuidInspectorBase<GlobalObjectId>
    {
        protected override void OnChanged(ChangeEvent<Object> evt)
        {
            if (null != evt.newValue && evt.newValue)
            {
                Target = GlobalObjectId.GetGlobalObjectIdSlow(evt.newValue);
            }
            else
            {
                Target = default;
            }
        }

        protected override void OnUpdate()
        {
            var defaultId = new GlobalObjectId();
            m_Field.SetValueWithoutNotify(Target.assetGUID == defaultId.assetGUID ? null : GlobalObjectId.GlobalObjectIdentifierToObjectSlow(Target));
        }
    }
}
