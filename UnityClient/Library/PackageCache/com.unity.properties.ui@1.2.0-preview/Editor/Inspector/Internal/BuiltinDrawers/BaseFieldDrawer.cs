using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    abstract class BaseFieldDrawer<TField, TFieldValue, TValue, TAttribute> : PropertyDrawer<TValue, TAttribute>
        where TField : BaseField<TFieldValue>, new()
        where TAttribute : UnityEngine.PropertyAttribute
    {
        protected TField m_Field;

        public override VisualElement Build()
        {
            m_Field = new TField
            {
                name = Name,
                label = DisplayName,
                tooltip = Tooltip,
                bindingPath = Part.ToString()
            };
            return m_Field;
        }
    }
    
    abstract class BaseFieldDrawer<TField, TValue, TAttribute> : BaseFieldDrawer<TField, TValue, TValue, TAttribute>
        where TField : BaseField<TValue>, new()
        where TAttribute : UnityEngine.PropertyAttribute
    {
    }
}
