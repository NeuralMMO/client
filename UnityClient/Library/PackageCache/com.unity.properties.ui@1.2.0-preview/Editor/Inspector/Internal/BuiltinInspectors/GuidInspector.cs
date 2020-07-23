using JetBrains.Annotations;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    [UsedImplicitly]
    class GuidInspector : Inspector<GUID>
    {
        TextField m_Field;

        public override VisualElement Build()
        {
            m_Field = new TextField
            {
                label = DisplayName,
                tooltip = Tooltip
            };
            m_Field.SetValueWithoutNotify(Target.ToString());
            m_Field.SetEnabled(false);
            return m_Field;
        }

        public override void Update()
        {
            // Nothing to do..
        }
    }
}