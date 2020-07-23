using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    [UsedImplicitly]
    class GlobalObjectIdInspector : Inspector<GlobalObjectId>
    {
        enum IdentifierType
        {
            [UsedImplicitly] Null,
            [UsedImplicitly] ImportedAsset,
            [UsedImplicitly] SceneObject,
            [UsedImplicitly] SourceAsset
        }

        EnumField m_IdentifierType;
        TextField m_Guid;
        TextField m_FileId;
        
        public override VisualElement Build()
        {
            var root = new Foldout
            {
                text = DisplayName,
                tooltip = Tooltip
            };
            var id = Target;
            m_IdentifierType = new EnumField(ObjectNames.NicifyVariableName(nameof(GlobalObjectId.identifierType)), (IdentifierType)id.identifierType);
            m_IdentifierType.SetEnabled(false);
            root.contentContainer.Add(m_IdentifierType);
            m_Guid = new TextField(ObjectNames.NicifyVariableName(nameof(GlobalObjectId.assetGUID)));
            m_Guid.SetValueWithoutNotify(id.assetGUID.ToString());
            m_Guid.SetEnabled(false);
            root.contentContainer.Add(m_Guid);
            
            m_FileId = new TextField("File Id");
            m_FileId.SetValueWithoutNotify(id.targetObjectId.ToString());
            m_FileId.SetEnabled(false);
            root.contentContainer.Add(m_FileId);
            return root;
        }

        public override void Update()
        {
        }
    }
}