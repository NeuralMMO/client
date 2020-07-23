using System;
using JetBrains.Annotations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    abstract class
        SliderInspectorBase<TSlider, TFieldType, TValue> : BaseFieldDrawer<TSlider, TFieldType, TValue, RangeAttribute>
        where TSlider : BaseSlider<TFieldType>, new()
        where TFieldType : IComparable<TFieldType>
    {
        protected abstract float LowValue { get; }
        protected abstract float HighValue { get; }

        FloatField m_ValueField;

        public override VisualElement Build()
        {
            base.Build();
            var range = DrawerAttribute;
            m_Field.lowValue = TypeConversion.Convert<float, TFieldType>(Mathf.Max(range.min, LowValue));
            m_Field.highValue = TypeConversion.Convert<float, TFieldType>(Mathf.Min(range.max, HighValue));

            var root = new VisualElement();
            root.Add(m_Field);
            m_Field.style.flexGrow = 1;
            m_ValueField = new FloatField
            {
                name = Name,
                label = null,
                tooltip = Tooltip,
            };
            m_ValueField.formatString = "0.##";
            m_ValueField.RegisterValueChangedCallback(OnChanged);
            m_ValueField.style.flexGrow = 0;
            m_ValueField.style.width = 50;
            root.Add(m_ValueField);
            root.style.flexDirection = FlexDirection.Row;
            return root;
        }

        void OnChanged(ChangeEvent<float> evt)
        {
            var clampedValue = Mathf.Clamp(evt.newValue, DrawerAttribute.min, DrawerAttribute.max);
            var value = TypeConversion.Convert<float, TValue>(clampedValue);
            if (!value.Equals(Target))
            {
                Target = value;
                Update();
            }
        }

        public override void Update()
        {
            var value = TypeConversion.Convert<TValue, float>(Target);
            if (value != m_ValueField.value)
            {
                m_ValueField.SetValueWithoutNotify(TypeConversion.Convert<TValue, float>(Target));
            }
        }
    }

    [UsedImplicitly]
    sealed class SByteSliderInspectorBase : SliderInspectorBase<SliderInt, int, sbyte>
    {
        protected override float LowValue { get; } = sbyte.MinValue;
        protected override float HighValue { get; } = sbyte.MaxValue;
    }

    [UsedImplicitly]
    sealed class ByteSliderInspectorBase : SliderInspectorBase<SliderInt, int, byte>
    {
        protected override float LowValue { get; } = byte.MinValue;
        protected override float HighValue { get; } = byte.MaxValue;
    }

    [UsedImplicitly]
    sealed class ShortSliderInspectorBase : SliderInspectorBase<SliderInt, int, short>
    {
        protected override float LowValue { get; } = short.MinValue;
        protected override float HighValue { get; } = short.MaxValue;
    }

    [UsedImplicitly]
    sealed class UShortSliderInspectorBase : SliderInspectorBase<SliderInt, int, ushort>
    {
        protected override float LowValue { get; } = ushort.MinValue;
        protected override float HighValue { get; } = ushort.MaxValue;
    }

    [UsedImplicitly]
    sealed class SliderInspectorBase : SliderInspectorBase<SliderInt, int, int>
    {
        protected override float LowValue { get; } = int.MinValue;
        protected override float HighValue { get; } = int.MaxValue;
    }

    [UsedImplicitly]
    class SliderInspector : SliderInspectorBase<Slider, float, float>
    {
        protected override float LowValue { get; } = float.MinValue;
        protected override float HighValue { get; } = float.MaxValue;
    }

    [UsedImplicitly]
    class LongSliderInspector : SliderInspectorBase<Slider, float, long>
    {
        protected override float LowValue { get; } = long.MinValue;
        protected override float HighValue { get; } = long.MaxValue;
    }

    [UsedImplicitly]
    class ULongSliderInspector : SliderInspectorBase<Slider, float, ulong>
    {
        protected override float LowValue { get; } = ulong.MinValue;
        protected override float HighValue { get; } = ulong.MaxValue;
    }

    [UsedImplicitly]
    class UIntSliderInspector : SliderInspectorBase<Slider, float, uint>
    {
        protected override float LowValue { get; } = uint.MinValue;
        protected override float HighValue { get; } = uint.MaxValue;
    }

    [UsedImplicitly]
    class DoubleSliderInspector : SliderInspectorBase<Slider, float, double>
    {
        protected override float LowValue { get; } = float.MinValue;
        protected override float HighValue { get; } = float.MaxValue;
    }
}