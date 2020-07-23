using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Unity.Properties.UI.Internal
{
    static class GuiFactory
    {
        public static NullableFoldout<TValue> Foldout<TValue>(
            IProperty property,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => ConstructFoldout<NullableFoldout<TValue>>(property, path, visitorContext);

        public static IListElement<TList, TElement> Foldout<TList, TElement>(
            IProperty property,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            where TList : IList<TElement>
            => ConstructFoldout<IListElement<TList, TElement>>(property, path, visitorContext);

        public static DictionaryElement<TDictionary, TKey, TValue> Foldout<TDictionary, TKey, TValue>(
            IProperty property,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            where TDictionary : IDictionary<TKey, TValue>
            => ConstructFoldout<DictionaryElement<TDictionary, TKey, TValue>>(
                property, path, visitorContext);

        public static HashSetElement<TSet, TElement> SetFoldout<TContainer, TSet, TElement>(
            IProperty<TContainer> property,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            where TSet : ISet<TElement>
            => ConstructFoldout<HashSetElement<TSet, TElement>>(property, path, visitorContext);

        public static Toggle Toggle(
            IProperty property,
            ref bool value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<Toggle, bool>(property, ref value, path, visitorContext);

        public static IntegerField SByteField(
            IProperty property,
            ref sbyte value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<IntegerField, int, sbyte>(property, ref value, path,
                visitorContext);

        public static IntegerField ByteField(
            IProperty property,
            ref byte value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<IntegerField, int, byte>(property, ref value, path, visitorContext);

        public static IntegerField UShortField(
            IProperty property,
            ref ushort value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<IntegerField, int, ushort>(property, ref value, path,
                visitorContext);

        public static IntegerField ShortField(
            IProperty property,
            ref short value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<IntegerField, int, short>(property, ref value, path,
                visitorContext);

        public static IntegerField IntField(
            IProperty property,
            ref int value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<IntegerField, int>(property, ref value, path, visitorContext);

        public static LongField UIntField(
            IProperty property,
            ref uint value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<LongField, long, uint>(property, ref value, path, visitorContext);

        public static LongField LongField(
            IProperty property,
            ref long value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<LongField, long>(property, ref value, path, visitorContext);

        public static TextField ULongField(
            IProperty property,
            ref ulong value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<TextField, string, ulong>(property, ref value, path,
                visitorContext);

        public static FloatField FloatField<TContainer>(
            IProperty<TContainer> property,
            ref float value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<FloatField, float>(property, ref value, path, visitorContext);

        public static DoubleField DoubleField(
            IProperty property,
            ref double value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<DoubleField, double>(property, ref value, path, visitorContext);

        public static TextField CharField(
            IProperty property,
            ref char value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<TextField, string, char>(property, ref value, path, visitorContext);

        public static TextField TextField(
            IProperty property,
            ref string value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
            => Construct<TextField, string, string>(property, ref value, path,
                visitorContext);

        public static ObjectField ObjectField<TContainer>(
            IProperty<TContainer> property,
            ref UnityEngine.Object value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
        {
            var element = Construct<ObjectField, UnityEngine.Object, UnityEngine.Object>(
                property,
                ref value,
                path,
                visitorContext,
                field => field.objectType = property.DeclaredValueType());
            return element;
        }

        public static EnumFlagsField FlagsField<TValue>(
            IProperty property,
            ref TValue value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
        {
            if (!typeof(TValue).IsEnum)
            {
                throw new ArgumentException();
            }

            var element = Construct<EnumFlagsField, Enum, TValue>(
                property,
                ref value,
                path,
                visitorContext);
            element.Init(value as Enum);
            return element;
        }

        public static EnumField EnumField<TValue>(
            IProperty property,
            ref TValue value,
            PropertyPath path,
            InspectorVisitorContext visitorContext)
        {
            if (!typeof(TValue).IsEnum)
            {
                throw new ArgumentException();
            }

            var element = Construct<EnumField, Enum, TValue>(
                property,
                ref value,
                path,
                visitorContext);
            element.Init(value as Enum);
            return element;
        }


        static TElement ConstructBase<TElement, TFieldValue>(IProperty property, VisualElement parent)
            where TElement : BaseField<TFieldValue>, new()
        {
            var element = new TElement();
            SetNames(property, element);
            SetTooltip(property, element);
            SetDelayed(property, element);
            SetReadOnly(property, element);
            parent.contentContainer.Add(element);
            return element;
        }

        static TElement ConstructBase<TElement>(IProperty property, VisualElement parent)
            where TElement : Foldout, new()
        {
            var element = new TElement();
            SetNames(property, element);
            SetTooltip(property, element);
            SetReadOnly(property, element);
            parent.contentContainer.Add(element);
            return element;
        }

        static void SetNames<TValue>(IProperty property, BaseField<TValue> element)
        {
            SetCommonNames(property, element);
            element.label = GetDisplayName(property);
        }

        static void SetNames(IProperty property, Foldout element)
        {
            SetCommonNames(property, element);
            element.text = GetDisplayName(property);
        }

        static void SetCommonNames(IProperty property, BindableElement element)
        {
            var name = property.Name;
            element.name = name;
            element.bindingPath = name;
            element.AddToClassList(name);
        }

        internal static string GetDisplayName(IProperty property)
        {
            var name = property.Name;
            return property is ICollectionElementProperty
                ? $"Element {name}"
                : property.HasAttribute<DisplayNameAttribute>()
                    ? property.GetAttribute<DisplayNameAttribute>().Name
                    : property.HasAttribute<InspectorNameAttribute>()
                        ? property.GetAttribute<InspectorNameAttribute>().displayName
                        : ObjectNames.NicifyVariableName(name);
        }

        internal static void SetTooltip(IProperty property, VisualElement element)
        {
            if (property.HasAttribute<TooltipAttribute>())
            {
                element.tooltip = property.GetAttribute<TooltipAttribute>().tooltip;
            }
        }

        static void SetDelayed<TFieldValue>(IProperty property, BaseField<TFieldValue> element)
        {
            if (property.HasAttribute<DelayedAttribute>() && element is TextInputBaseField<TFieldValue> textInput)
            {
                textInput.isDelayed = true;
            }
        }

        static void SetReadOnly(IProperty property, VisualElement element)
        {
            if (property.IsReadOnly && (property.DeclaredValueType().IsValueType || !Unity.Properties.Internal.RuntimeTypeInfoCache.IsContainerType(property.DeclaredValueType())))
            {
                element.SetEnabledSmart(false);
            }
        }

        static TElement Construct<TElement, TValue>(
            IProperty property,
            ref TValue value,
            PropertyPath path,
            InspectorVisitorContext visitorContext
        )
            where TElement : BaseField<TValue>, new()
        {
            return Construct<TElement, TValue, TValue>(property, ref value, path,
                visitorContext);
        }

        static TElement ConstructFoldout<TElement>(
            IProperty property,
            PropertyPath path,
            InspectorVisitorContext visitorContext
        )
            where TElement : Foldout, IContextElement, new()
        {
            var element = ConstructBase<TElement>(property, visitorContext.Parent);
            element.SetContext(visitorContext.Root, path);
            var targetType = visitorContext.Root.GetTargetType();
            element.SetValueWithoutNotify(UiPersistentState.GetFoldoutState(targetType, path));
            element.RegisterCallback<ChangeEvent<bool>>(evt => UiPersistentState.SetFoldoutState(targetType, path, evt.newValue));
            return element;
        }

        abstract class UIBinding<TElement, TValue> : IBinding
            where TElement : VisualElement
        {
            protected PropertyElement Root;
            protected PropertyPath Path;
            protected TElement Element;

            protected UIBinding(TElement element, PropertyElement root, PropertyPath path)
            {
                Element = element;
                Root = root;
                Path = path;
            }
            
            public void PreUpdate()
            {
            }

            public abstract void Update();

            public void Release()
            {
            }
        }
        
        class TextureBinding<TValue> : UIBinding<BindableElement, TValue>
        {
            public TextureBinding(BindableElement element, PropertyElement root, PropertyPath path) : base(element, root, path)
            {
            }
            
            public override void Update()
            {
                if (!Root.TryGetValue<TValue>(Path, out var value))
                    return;

                if (!TypeConversion.TryConvert(value, out Texture2D texture))
                    return;

                Element.style.backgroundImage = texture;
            }
        }
        
        class LabelBinding<TValue> : UIBinding<Label, TValue>
        {
            public LabelBinding(Label element, PropertyElement root, PropertyPath path) : base(element, root, path)
            {
            }
            
            public override void Update()
            {
                if (!Root.TryGetValue<TValue>(Path, out var value))
                    return;

                if (!TypeConversion.TryConvert(value, out string strValue))
                    return;

                Element.text = strValue;
            }
        }
        
        class Binding<TFieldType, TValue> : UIBinding<BaseField<TFieldType>, TValue>
        {
            public Binding(BaseField<TFieldType> element, PropertyElement root, PropertyPath path) : base(element, root, path)
            {
            }
            
            public override void Update()
            {
                if (!Root.TryGetValue<TValue>(Path, out var value))
                    return;

                if (!TypeConversion.TryConvert(value, out TFieldType fieldValue))
                    return;

                if ((!typeof(TValue).IsValueType && null == value) || fieldValue.Equals(Element.value))
                    return;

                if (Element?.focusController?.focusedElement != Element)
                {
                    Element.SetValueWithoutNotify(fieldValue);
                }
            }
        }

        static TElement Construct<TElement, TFieldType, TValue>(
            IProperty property,
            ref TValue value,
            PropertyPath path,
            InspectorVisitorContext visitorContext,
            Action<TElement> initializer = null
        )
            where TElement : BaseField<TFieldType>, new()
        {
            var element = ConstructBase<TElement, TFieldType>(property, visitorContext.Parent);
            initializer?.Invoke(element);

            SetCallbacks(ref value, path, visitorContext.Root, element);
            visitorContext.Parent.contentContainer.Add(element);
            return element;
        }

        internal static void SetCallbacks<TFieldType, TValue>(
            ref TValue value,
            PropertyPath path,
            PropertyElement root,
            BaseField<TFieldType> field)
        {
            if (TypeConversion.TryConvert(value, out TFieldType fieldValue))
            {
                field.SetValueWithoutNotify(fieldValue);
                field.binding = new Binding<TFieldType, TValue>(field, root, path);
            }

            field.RegisterCallback<ChangeEvent<TFieldType>, PropertyPath>(ValueChanged<TFieldType, TValue>, path);
        }
        
        internal static void SetCallbacks<TValue>(
            ref TValue value,
            PropertyPath path,
            PropertyElement root,
            BindableElement element)
        {
            if (!TypeConversion.TryConvert(value, out Texture2D texture))
                return;
            
            element.style.backgroundImage = texture;
            element.binding = new TextureBinding<TValue>(element, root, path);
        }
        
        internal static void SetCallbacks<TValue>(
            ref TValue value,
            PropertyPath path,
            PropertyElement root,
            Label label)
        {
            if (!TypeConversion.TryConvert(value, out string strValue)) 
                return;
            
            label.text = strValue;
            label.binding = new LabelBinding<TValue>(label, root, path);
        }

        static void ValueChanged<TFieldType, TValue>(ChangeEvent<TFieldType> evt, PropertyPath path)
        {
            var field = evt.target as BaseField<TFieldType>;
            var fieldValue = evt.newValue;
            var element = field?.GetFirstAncestorOfType<PropertyElement>();
            if (null == element)
                return;

            if (!TypeConversion.TryConvert(fieldValue, out TValue value))
                return;

            var oldValue = element.GetValue<TValue>(path);

            if (!element.TrySetValue(path, value))
                return;

            var newValue = element.GetValue<TValue>(path);
            if (TypeConversion.TryConvert(newValue, out TFieldType newFieldValue))
            {
                field.SetValueWithoutNotify(newFieldValue);
            }

            if (!Unity.Properties.Internal.RuntimeTypeInfoCache<TValue>.IsValueType && null == newValue)
            {
                if (null != oldValue)
                {
                    element.NotifyChanged(path);
                }
            }
            else
            {
                if (!newValue.Equals(oldValue))
                    element.NotifyChanged(path);
            }
        }
    }
}