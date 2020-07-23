using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    abstract class MinDrawerBase<TElement, TFieldValue, TValue, TAttribute> : BaseFieldDrawer<TElement, TFieldValue, TValue, TAttribute>
        where TElement : BaseField<TFieldValue>, new()
        where TAttribute : PropertyAttribute
    {
        float m_MinValue;

        public override VisualElement Build()
        {
            base.Build();
            m_Field.bindingPath = string.Empty;
            RegisterValueChangedCallback();
            m_MinValue = GetMinValue();
            return m_Field;
        }

        protected abstract float GetMinValue();
        
        void RegisterValueChangedCallback()
        {
            m_Field.RegisterValueChangedCallback(evt =>
            {
                var input = m_Field as TextInputBaseField<TFieldValue>;
                if (null != input)
                {
                    input.isDelayed = false;
                }
                OnChanged(evt);
                Update();
                if (null != input)
                {
                    input.isDelayed = IsDelayed;
                }
            });
        }

        void OnChanged(ChangeEvent<TFieldValue> evt)
        {
            if (TypeConversion.TryConvert(evt.newValue, out float newValue)
                && TypeConversion.TryConvert(Mathf.Max(newValue, m_MinValue), out TValue value))
            {
                Target = value;
            }
        }

        public override void Update()
        {
            if (TypeConversion.TryConvert(Target, out TFieldValue value) && !value.Equals(m_Field.value))
            {
                m_Field.SetValueWithoutNotify(value);
            }
        }
    }

    abstract class MinValueDrawer<TElement, TFieldValue, TValue> : MinDrawerBase<TElement, TFieldValue, TValue, MinValueAttribute>
        where TElement : BaseField<TFieldValue>, new()
    {
        protected override float GetMinValue()
            => GetAttribute<MinValueAttribute>().Min;
    }
    
    abstract class MinDrawer<TElement, TFieldValue, TValue> : MinDrawerBase<TElement, TFieldValue, TValue, MinAttribute>
        where TElement : BaseField<TFieldValue>, new()
    {
        protected override float GetMinValue()
            => GetAttribute<MinAttribute>().min;
    }
    
    [UsedImplicitly] class MinSByteDrawer : MinDrawer<IntegerField, int, sbyte> { }
    [UsedImplicitly] class MinByteDrawer : MinDrawer<IntegerField, int, byte> { }
    [UsedImplicitly] class MinShortDrawer : MinDrawer<IntegerField, int, short> { }
    [UsedImplicitly] class MinUShortDrawer : MinDrawer<IntegerField, int, ushort> { }
    [UsedImplicitly] class MinIntDrawer : MinDrawer<IntegerField, int, int> { }
    [UsedImplicitly] class MinUIntDrawer : MinDrawer<LongField, long, uint> { }
    [UsedImplicitly] class MinLongDrawer : MinDrawer<LongField, long, long> { }
    [UsedImplicitly] class MinULongDrawer : MinDrawer<FloatField, float, ulong> { }
    [UsedImplicitly] class MinFloatDrawer : MinDrawer<FloatField, float, float> { }
    [UsedImplicitly] class MinDoubleDrawer : MinDrawer<DoubleField, double, double> { }
    
    [UsedImplicitly] class MinSByteValueDrawer : MinValueDrawer<IntegerField, int, sbyte> { }
    [UsedImplicitly] class MinByteValueDrawer : MinValueDrawer<IntegerField, int, byte> { }
    [UsedImplicitly] class MinShortValueDrawer : MinValueDrawer<IntegerField, int, short> { }
    [UsedImplicitly] class MinUShortValueDrawer : MinValueDrawer<IntegerField, int, ushort> { }
    [UsedImplicitly] class MinIntValueDrawer : MinValueDrawer<IntegerField, int, int> { }
    [UsedImplicitly] class MinUIntValueDrawer : MinValueDrawer<LongField, long, uint> { }
    [UsedImplicitly] class MinLongValueDrawer : MinValueDrawer<LongField, long, long> { }
    [UsedImplicitly] class MinULongValueDrawer : MinValueDrawer<FloatField, float, ulong> { }
    [UsedImplicitly] class MinFloatValueDrawer : MinValueDrawer<FloatField, float, float> { }
    [UsedImplicitly] class MinDoubleValueDrawer : MinValueDrawer<DoubleField, double, double> { }
}