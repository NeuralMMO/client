using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Properties.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class BindingVisitor : PathVisitor
    {
        public VisualElement Element;
        public PropertyElement Root;
        static readonly MethodInfo BaseBinderMethod;

        static readonly Dictionary<TypePairKey, MethodInfo> s_RegistrationMethods =
            new Dictionary<TypePairKey, MethodInfo>();

        static BindingVisitor()
        {
            BaseBinderMethod = typeof(BindingVisitor)
                .GetMethod(nameof(SetCallbacks), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        protected override void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property,
            ref TContainer container, ref TValue value)
        {
            switch (Element)
            {
                case BaseField<TValue> field:
                    GuiFactory.SetCallbacks(ref value, Path, Root, field);
                    break;
                case Label label when TypeConversion.TryConvert(value, out string _):
                    GuiFactory.SetCallbacks(ref value, Path, Root, label);
                    break;
                case BindableElement bindable when TypeConversion.TryConvert(value, out Texture2D _):
                    GuiFactory.SetCallbacks(ref value, Path, Root, bindable);
                    break;
                default:
                    // Use reflection to figure out if we can map it.
                    TrySetCallbacksThroughReflection(ref value);
                    break;
            }
        }

        void TrySetCallbacksThroughReflection<TValue>(ref TValue value)
        {
            var type = Element.GetType();
            var baseFieldType = GetBaseFieldType(type);

            if (null == baseFieldType)
                return;

            var fieldType = baseFieldType.GenericTypeArguments[0];
            var key = new TypePairKey(fieldType, typeof(TValue));
            if (!s_RegistrationMethods.TryGetValue(key, out var method))
            {
                s_RegistrationMethods[key] = method = BaseBinderMethod.MakeGenericMethod(fieldType, typeof(TValue));
            }

            method.Invoke(this, new object[] {value, Element});
        }

        void SetCallbacks<TFieldType, TValue>(ref TValue value, BaseField<TFieldType> field)
        {
            GuiFactory.SetCallbacks(ref value, Path, Root, field);
        }

        static Type GetBaseFieldType(Type type)
        {
            var generic = typeof(BaseField<>);
            while (type != null && type != typeof(object))
            {
                var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (generic == current)
                {
                    return type;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}