using System;
using System.Collections.Generic;
using UnityEditor;

namespace Unity.QuickSearch
{
    [InitializeOnLoad]
    static class Dispatcher
    {
        private static volatile bool s_Active = false;
        private static readonly Queue<Action> s_ExecutionQueue = new Queue<Action>();

        static Dispatcher()
        {
            EditorApplication.update += Update;
        }

        public static void Enqueue(Action action)
        {
            lock (s_ExecutionQueue)
            {
                s_ExecutionQueue.Enqueue(action);
                s_Active = true;
            }
        }

        static void Update()
        {
            if (!s_Active)
                return;

            lock (s_ExecutionQueue)
            {
                while (s_ExecutionQueue.Count > 0)
                    s_ExecutionQueue.Dequeue().Invoke();
                s_Active = false;
            }
        }
    }
}
