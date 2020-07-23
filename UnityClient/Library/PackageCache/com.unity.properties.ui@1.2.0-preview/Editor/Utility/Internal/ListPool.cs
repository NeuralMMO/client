using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties.UI.Internal
{
    static class ListPool<T>
    {
        static readonly Pool<List<T>> s_Pool = new Pool<List<T>>(CreateInstanceFunc, OnRelease);
        static List<T> CreateInstanceFunc() => new List<T>();

        static void OnRelease(List<T> list) => list.Clear();

        public static List<T> Get() => s_Pool.Get();
        public static void Release(List<T> list) => s_Pool.Release(list);

    }
}