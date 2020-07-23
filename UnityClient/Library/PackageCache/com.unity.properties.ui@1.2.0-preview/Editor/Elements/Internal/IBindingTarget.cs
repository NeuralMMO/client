using System;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    interface IBindingTarget
    {
        Type TargetType { get; }
        PropertyElement Root { get; }
        IInspectorVisitor Visitor { get; }
        bool TryGetTarget<T>(out T t);

        bool IsPathValid(PropertyPath path);
        void RegisterBindings(PropertyPath path, VisualElement element);
        void VisitAtPath(PropertyPath path, VisualElement parent);
        void SetAtPath<TValue>(PropertyPath path, TValue value);
        bool TrySetAtPath<TValue>(PropertyPath path, TValue value);
        TValue GetAtPath<TValue>(PropertyPath path);
        bool TryGetAtPath<TValueType>(PropertyPath path, out TValueType value);
        bool TryGetProperty(PropertyPath path, out IProperty property);
        void GenerateHierarchy();
        void Release();
    }
    
    interface IBindingTarget<out TTarget> : IBindingTarget
    {
        TTarget Target { get; }
    }
}