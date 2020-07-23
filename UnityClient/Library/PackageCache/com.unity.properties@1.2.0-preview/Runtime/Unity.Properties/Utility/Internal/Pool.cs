using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Properties.Internal
{
    class Pool<T>
    {
        readonly Stack<T> m_Stack;
        readonly Func<T> m_CreateFunc;
        readonly Action<T> m_OnRelease;

        public Pool(Func<T> createInstanceFunc, Action<T> onRelease)
        {
            m_CreateFunc = createInstanceFunc;
            m_Stack = new Stack<T>();
            m_OnRelease = onRelease;
        }
        
        public int Size()
        {
            return m_Stack.Count;
        }

        public void Clear()
        {
            m_Stack.Clear();
        }

        public T Get()
        {
            return m_Stack.Count == 0 ? m_CreateFunc() : m_Stack.Pop();
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && m_Stack.Contains(element))
            {
                Debug.LogError($"Trying to release object of type `{typeof(T).Name}` that is already pooled.");
                return;
            }

            m_OnRelease?.Invoke(element);
            m_Stack.Push(element);
        }
    }
}