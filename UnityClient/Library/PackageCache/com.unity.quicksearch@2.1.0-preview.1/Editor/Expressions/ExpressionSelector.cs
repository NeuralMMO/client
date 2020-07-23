using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Unity.QuickSearch
{
    delegate IEnumerable<SearchItem> ExpressionSelect(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items);
    delegate void ExpressionSelectDrawer(IExpressionInspector inspector);

    [AttributeUsage(AttributeTargets.Method)]
    class ExpressionSelectAttribute : Attribute
    {
        public string name;

        public ExpressionSelectAttribute(string name)
        {
            this.name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    class ExpressionSelectDrawerAttribute : Attribute
    {
        public string name;

        public ExpressionSelectDrawerAttribute(string name)
        {
            this.name = name;
        }
    }

    interface IExpressionInspector
    {
        T GetProperty<T>(string name, T defaultValue);
        int GetProperty(string name, int defaultValue);
        void SetProperty(string name, object value);
        void DrawSelectionPopup(string label, string value, IEnumerable<string> choices, Action<int> selectedHandler, float extraWidth = 200f);
    }

    static class ExpressionSelectors
    {
        struct DerivedTypeInfo
        {
            public List<string> names;
            public List<string> labels;
            public Type[] types;
        }

        struct PropertyInfo
        {
            public string name;
            public string label;
        }

        static string[] s_SelectorNames;
        static MethodInfo[] s_SelectMethods;
        static MethodInfo[] s_DrawerMethods;

        static readonly Dictionary<Type, DerivedTypeInfo> s_DerivedTypes = new Dictionary<Type, DerivedTypeInfo>();
        static readonly Dictionary<Type, List<PropertyInfo>> s_TypeProperties = new Dictionary<Type, List<PropertyInfo>>();
        static readonly Dictionary<string, ExpressionSelect> s_SelectorDelegates = new Dictionary<string, ExpressionSelect>();
        static readonly Dictionary<string, ExpressionSelectDrawer> s_SelectorDrawers = new Dictionary<string, ExpressionSelectDrawer>();

        static MethodInfo[] selectMethods
        {
            get
            {
                if (s_SelectMethods == null)
                    s_SelectMethods = TypeCache.GetMethodsWithAttribute<ExpressionSelectAttribute>().ToArray();
                return s_SelectMethods;
            }
        }

        public static string[] names
        {
            get
            {
                if (s_SelectorNames == null)
                    s_SelectorNames = selectMethods.Select(info => GetSelectAttribute(info).name).OrderBy(n=>n).ToArray();
                return s_SelectorNames;
            }
        }

        private static MethodInfo[] drawerMethods
        {
            get
            {
                if (s_DrawerMethods == null)
                    s_DrawerMethods = TypeCache.GetMethodsWithAttribute<ExpressionSelectDrawerAttribute>().ToArray();
                return s_DrawerMethods;
            }
        }

        public static ExpressionSelect GetDelegate(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Default;

            name = name.ToLowerInvariant();
            if (s_SelectorDelegates.TryGetValue(name, out var handler))
                return handler;

            foreach (var methodInfo in selectMethods)
            {
                var attribute = GetSelectAttribute(methodInfo);
                if (name.Equals(attribute.name, StringComparison.OrdinalIgnoreCase))
                {
                    handler = (ExpressionSelect)methodInfo.CreateDelegate(typeof(ExpressionSelect));
                    s_SelectorDelegates[name] = handler;
                    return handler;
                }
            }

            Debug.LogWarning($"Cannot find any expression selector for {name}");
            s_SelectorDelegates[name] = Default;
            return Default;
        }

        public static bool Draw(string selectType, IExpressionInspector inspector)
        {
            selectType = selectType.ToLowerInvariant();
            if (s_SelectorDrawers.TryGetValue(selectType, out var handler))
            {
                if (handler == null)
                    return false;

                handler(inspector);
                return true;
            }

            foreach (var methodInfo in drawerMethods)
            {
                var attribute = (ExpressionSelectDrawerAttribute)methodInfo.GetCustomAttribute(typeof(ExpressionSelectDrawerAttribute));
                if (!selectType.Equals(attribute.name, StringComparison.OrdinalIgnoreCase))
                    continue;

                handler = (ExpressionSelectDrawer)methodInfo.CreateDelegate(typeof(ExpressionSelectDrawer));
                s_SelectorDrawers[selectType] = handler;
                handler(inspector);
                return true;
            }

            s_SelectorDrawers[selectType] = null;
            return false;
        }

        private static ExpressionSelectAttribute GetSelectAttribute(MethodInfo mi)
        {
            return (ExpressionSelectAttribute)mi.GetCustomAttribute(typeof(ExpressionSelectAttribute));
        }

        [ExpressionSelect("Default")]
        internal static IEnumerable<SearchItem> Default(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            return items;
        }

        [ExpressionSelectDrawer("Default")]
        internal static void DrawSelectDefaultEditor(IExpressionInspector m_Node)
        {
            GUILayout.Space(40f);
        }

        [ExpressionSelect("Type")]
        internal static IEnumerable<SearchItem> Type(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            return items.SelectMany(item => selectRequest.SelectTypes(item));
        }

        [ExpressionSelect("Component")]
        internal static IEnumerable<SearchItem> Component(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            var objectType = node.GetProperty<string>("type", null);
            var propertyName = node.GetProperty<string>("field", null);
            var mapped = node.GetProperty(ExpressionKeyName.Mapped, false);
            var overrides = node.GetProperty(ExpressionKeyName.Overrides, true);

            return items.SelectMany(item => selectRequest.SelectComponent(item, objectType, propertyName, mapped, overrides));
        }

        [ExpressionSelectDrawer("Component")]
        internal static void DrawSelectComponentEditor(IExpressionInspector inode)
        {
            var ctypes = GetDerivedTypeInfo(typeof(Component));
            var typeName = inode.GetProperty<string>("type", null);
            var selectedTypeIndex = ctypes.names.FindIndex(n => n == typeName);
            var typeLabel = selectedTypeIndex == -1 ? null : ctypes.labels[selectedTypeIndex];

            inode.DrawSelectionPopup("Type", typeLabel ?? "Select type...", ctypes.labels, selectedIndex => inode.SetProperty("type", ctypes.names[selectedIndex]));
            if (typeName != null)
            {
                if (selectedTypeIndex != -1)
                {
                    var selectedType = ctypes.types[selectedTypeIndex];
                    var properties = GetTypePropertyNames(selectedType);
                    var propertyName = inode.GetProperty<string>("field", null);
                    var propertyLabel = properties.FirstOrDefault(p => p.name == propertyName).label;

                    inode.DrawSelectionPopup("Property", propertyLabel ?? "Select property...", properties.Select(p => p.label), selectedIndex =>
                    {
                        inode.SetProperty("field", properties[selectedIndex].name);
                    });
                }
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            var originalSource = EditorGUILayout.Toggle("Overrides", inode.GetProperty(ExpressionKeyName.Overrides, true));
            if (EditorGUI.EndChangeCheck())
                inode.SetProperty(ExpressionKeyName.Overrides, originalSource);

            DrawMappedControl(inode);

            EditorGUILayout.EndHorizontal();
        }

        [ExpressionSelect("Object")]
        internal static IEnumerable<SearchItem> Object(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            var objectType = node.GetProperty<string>("type", null);
            var propertyName = node.GetProperty<string>("field", null);
            var mapped = node.GetProperty(ExpressionKeyName.Mapped, false);
            var overrides = node.GetProperty(ExpressionKeyName.Overrides, true);

            return items.SelectMany(item => selectRequest.SelectObject(item, objectType, propertyName, mapped, overrides));
        }

        [ExpressionSelectDrawer("Object")]
        internal static void DrawSelectObjectEditor(IExpressionInspector inode)
        {
            var otypes = GetDerivedTypeInfo(typeof(UnityEngine.Object));
            var typeName = inode.GetProperty<string>("type", null);
            var selectedTypeIndex = otypes.names.FindIndex(n => n == typeName);
            var typeLabel = selectedTypeIndex == -1 ? null : otypes.labels[selectedTypeIndex];

            inode.DrawSelectionPopup("Type", typeLabel ?? "Select type...", otypes.labels, selectedIndex => inode.SetProperty("type", otypes.names[selectedIndex]));
            if (typeName != null)
            {
                if (selectedTypeIndex != -1)
                {
                    var selectedType = otypes.types[selectedTypeIndex];
                    var properties = GetTypePropertyNames(selectedType);
                    var propertyName = inode.GetProperty<string>("field", null);
                    var propertyLabel = properties.FirstOrDefault(p => p.name == propertyName).label;

                    inode.DrawSelectionPopup("Property", propertyLabel ?? "Select property...", properties.Select(p => p.label), selectedIndex =>
                    {
                        inode.SetProperty("field", properties[selectedIndex].name);
                    });
                }
            }

            DrawMappedControl(inode);
        }

        [ExpressionSelect("Asset")]
        internal static IEnumerable<SearchItem> Asset(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            return Object(selectRequest, node, items);
        }

        [ExpressionSelectDrawer("Asset")]
        internal static void DrawSelectAssetEditor(IExpressionInspector inspector)
        {
            DrawSelectObjectEditor(inspector);
        }

        [ExpressionSelect("Path")]
        internal static IEnumerable<SearchItem> Path(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            return items.SelectMany(item => selectRequest.SelectPath(item));
        }

        [ExpressionSelect("References")]
        internal static IEnumerable<SearchItem> References(SearchRequest selectRequest, SearchExpressionNode node, IEnumerable<SearchItem> items)
        {
            var objectType = node.GetProperty<string>("type", null);
            var refDepth = node.GetProperty("depth", 1);

            return items.SelectMany(item => selectRequest.SelectReferences(item, objectType, refDepth));
        }

        [ExpressionSelectDrawer("References")]
        internal static void DrawSelectReferencesEditor(IExpressionInspector m_Node)
        {
            var refPropertyType = m_Node.GetProperty("type", "");
            var refDepth = m_Node.GetProperty("depth", 1);
            EditorGUI.BeginChangeCheck();
            refPropertyType = EditorGUILayout.DelayedTextField("Type", refPropertyType);
            refDepth = EditorGUILayout.IntSlider("Depth", refDepth, 0, 9);
            if (EditorGUI.EndChangeCheck())
            {
                m_Node.SetProperty("type", refPropertyType);
                m_Node.SetProperty("depth", refDepth);
            }
        }

        private static void DrawMappedControl(IExpressionInspector m_Node)
        {
            EditorGUI.BeginChangeCheck();
            var mapped = EditorGUILayout.Toggle("Mapped", m_Node.GetProperty(ExpressionKeyName.Mapped, false));
            if (EditorGUI.EndChangeCheck())
                m_Node.SetProperty(ExpressionKeyName.Mapped, mapped);
        }

        private static DerivedTypeInfo GetDerivedTypeInfo(Type type)
        {
            if (s_DerivedTypes.TryGetValue(type, out var typeInfo))
                return typeInfo;
            var derivedTypes = TypeCache.GetTypesDerivedFrom(type)
                .Where(t => !t.IsAbstract)
                .Where(t => !t.IsSubclassOf(typeof(Editor)) &&
                            !t.IsSubclassOf(typeof(Shader)) &&
                            !t.IsSubclassOf(typeof(EditorWindow)) &&
                            !t.IsSubclassOf(typeof(AssetImporter)))
                .OrderBy(t => t.Name).ToArray();
            typeInfo = new DerivedTypeInfo()
            {
                types = derivedTypes,
                names = derivedTypes.Select(t => t.Name).ToList(),
                labels = derivedTypes.Select(t =>
                {
                    if (t.FullName.Contains("Unity"))
                        return t.Name;
                    return "<b>" + t.Name + "</b>";
                }).ToList()
            };
            s_DerivedTypes[type] = typeInfo;
            return typeInfo;
        }

        private static List<PropertyInfo> GetTypePropertyNames(Type type)
        {
            if (s_TypeProperties.TryGetValue(type, out var properties))
                return properties;

            properties = new List<PropertyInfo>();
            GameObject go = null;
            try
            {
                bool objectCreated = false;
                UnityEngine.Object obj = null;
                try
                {
                    if (obj == null)
                    {
                        if (typeof(Component).IsAssignableFrom(type))
                        {
                            go = new GameObject
                            {
                                layer = 0
                            };
                            go.AddComponent(typeof(BoxCollider));
                            obj = go.GetComponent(type);
                            if (!obj)
                            {
                                obj = ObjectFactory.AddComponent(go, type);
                                objectCreated = true;
                            }
                        }
                        else
                        {
                            obj = ObjectFactory.CreateInstance(type);
                            objectCreated = true;
                        }
                    }

                    using (var so = new SerializedObject(obj))
                    {
                        var p = so.GetIterator();
                        var next = p.NextVisible(true);
                        while (next)
                        {
                            if (p.propertyType != SerializedPropertyType.Generic &&
                                p.propertyType != SerializedPropertyType.LayerMask &&
                                p.propertyType != SerializedPropertyType.Character &&
                                p.propertyType != SerializedPropertyType.ArraySize &&
                                !p.isArray && !p.isFixedBuffer)
                            {
                                properties.Add(new PropertyInfo()
                                {
                                    name = p.propertyPath,
                                    label = FormatPropertyLabel(p)
                                });
                            }

                            next = p.NextVisible(!p.isArray && !p.isFixedBuffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.Message);
                }
                finally
                {
                    if (objectCreated)
                        UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
            finally
            {
                if (go)
                    UnityEngine.Object.DestroyImmediate(go);
            }

            s_TypeProperties[type] = properties;
            return properties;
        }

        private static string FormatPropertyLabel(SerializedProperty property)
        {
            if (property.depth > 0)
                return $"<i>{property.propertyPath.Replace("m_", "")}</i> (<b>{property.propertyType}</b>)";
            return $"{property.displayName} (<b>{property.propertyType}</b>)";
        }
    }
}
