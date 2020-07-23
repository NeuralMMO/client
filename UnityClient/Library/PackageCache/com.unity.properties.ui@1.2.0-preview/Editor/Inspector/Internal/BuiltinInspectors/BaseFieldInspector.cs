using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    abstract class BaseFieldInspector<TField, TFieldValue, TValue> : Inspector<TValue>
        where TField : BaseField<TFieldValue>, new()
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
    
    abstract class BaseFieldInspector<TField, TValue> : BaseFieldInspector<TField, TValue, TValue>
        where TField : BaseField<TValue>, new()
    {
    }
}
