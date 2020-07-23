using System;
using System.Collections.Generic;
using Unity.Serialization.Editor;

namespace Unity.Properties.UI.Internal
{
    class UiPersistentState
    {
        public const string Key = "unity-properties__ui-persistent-state";

        internal struct PaginationData
        {
            public int PaginationSize;
            public int CurrentPage;
        }

        [CreateProperty]
        readonly Dictionary<int, bool> FoldoutState = new Dictionary<int, bool>();

        [CreateProperty]
        readonly Dictionary<int, PaginationData> PaginationState = new Dictionary<int, PaginationData>();

        //[MenuItem("Properties/UI/Clear PersistentState")]
        public static void ClearState()
        {
            UserSettings<UiPersistentState>.Clear(Key);
        }

        public static void SetFoldoutState(Type type, PropertyPath path, bool foldout)
        {
            if (null == type || null == path)
                return;

            var state = UserSettings<UiPersistentState>.GetOrCreate(Key);
            state.FoldoutState[ComputeHash(type, path)] = foldout;
        }

        public static bool GetFoldoutState(Type type, PropertyPath path, bool defaultValue = false)
        {
            if (null == type || null == path)
                return defaultValue;

            var state = UserSettings<UiPersistentState>.GetOrCreate(Key);
            return state.FoldoutState.TryGetValue(ComputeHash(type, path), out var foldout) ? foldout : defaultValue;
        }

        public static void SetPaginationState(Type type, PropertyPath path, int size, int page)
        {
            if (null == type || null == path)
                return;

            var state = UserSettings<UiPersistentState>.GetOrCreate(Key);
            state.PaginationState[ComputeHash(type, path)] = new PaginationData {PaginationSize = size, CurrentPage = page};
        }

        public static PaginationData GetPaginationState(Type type, PropertyPath path)
        {
            if (null == type || null == path)
                return default;

            var state = UserSettings<UiPersistentState>.GetOrCreate(Key);
            return state.PaginationState.TryGetValue(ComputeHash(type, path), out var data) ? data : default;
        }

        static int ComputeHash(Type type, PropertyPath path)
        {
            var hash = 19;
            hash = hash * 31 + type.FullName.GetHashCode();
            hash = hash * 31 + path.GetHashCode();
            return hash;
        }
    }
}