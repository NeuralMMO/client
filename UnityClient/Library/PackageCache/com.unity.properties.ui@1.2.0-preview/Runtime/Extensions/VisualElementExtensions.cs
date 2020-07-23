using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Properties
{
    static class VisualElementExtensions
    {
        public static void Show(this VisualElement element)
        {
            element.style.display = DisplayStyle.Flex;
        }
        
        public static void Hide(this VisualElement element)
        {
            element.style.display = DisplayStyle.None;
        }

        public static void SetEnabledSmart(this VisualElement element, bool value)
        {
            if (element is Foldout foldout)
            {
                if (value)
                {
                    foldout.RemoveFromClassList("unity-disabled");
                }
                else
                {
                    foldout.AddToClassList("unity-disabled");
                }
                foldout.SetEnabled(true);
                foldout.contentContainer.SetEnabled(value);
            }
            else
            {
                element.SetEnabled(value);
            }
        }

        public static IEnumerable<T> ChildrenOfType<T>(this VisualElement element)
        {
            foreach (var child in element.Children())
            {
                if (child is T t)
                {
                    yield return t;
                }

                foreach (var e in child.ChildrenOfType<T>())
                {
                    yield return e;
                }
            }
        }
    }
}