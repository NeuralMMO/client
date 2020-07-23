using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEditorInternal;

#if UNITY_2020_1_OR_NEWER
using UnityEngine.UIElements;
#endif

[assembly: InternalsVisibleTo("com.unity.quicksearch.tests")]

namespace Unity.QuickSearch
{
    static class Utils
    {
        const string packageName = "com.unity.quicksearch";

        public static readonly string packageFolderName = $"Packages/{packageName}";
        public static readonly bool isDeveloperBuild = false;

        private static string[] _ignoredAssemblies =
        {
            "^UnityScript$", "^System$", "^mscorlib$", "^netstandard$",
            "^System\\..*", "^nunit\\..*", "^Microsoft\\..*", "^Mono\\..*", "^SyntaxTree\\..*"
        };

        static Utils()
        {
            isDeveloperBuild = Directory.Exists($"{packageFolderName}/.git");
        }

        private static Type[] GetAllEditorWindowTypes()
        {
            return GetAllDerivedTypes(AppDomain.CurrentDomain, typeof(EditorWindow));
        }

        /// <summary>
        /// Opens the Quick Search documentation page
        /// </summary>
        public static void OpenDocumentationUrl()
        {
            string documentationUrl = $"https://docs.unity3d.com/Packages/com.unity.quicksearch{GetQuickSearchDocVersion()}/manual/search-syntax.html";
            var uri = new Uri(documentationUrl);
            Process.Start(uri.AbsoluteUri);
        }

        public static void OpenInBrowser(string baseUrl, List<Tuple<string, string>> query = null)
        {
            var url = baseUrl;

            if (query != null)
            {
                url += "?";
                for (var i = 0; i < query.Count; ++i)
                {
                    var item = query[i];
                    url += item.Item1 + "=" + item.Item2;
                    if (i < query.Count - 1)
                    {
                        url += "&";
                    }
                }
            }

            var uri = new Uri(url);
            Process.Start(uri.AbsoluteUri);
        }

        internal static Type GetProjectBrowserWindowType()
        {
            return GetAllEditorWindowTypes().FirstOrDefault(t => t.Name == "ProjectBrowser");
        }

        internal static string GetNameFromPath(string path)
        {
            var lastSep = path.LastIndexOf('/');
            if (lastSep == -1)
                return path;

            return path.Substring(lastSep + 1);
        }

        public static Texture2D GetAssetThumbnailFromPath(string path)
        {
            Texture2D thumbnail = AssetDatabase.GetCachedIcon(path) as Texture2D;
            return thumbnail ?? UnityEditorInternal.InternalEditorUtility.FindIconForFile(path);
        }

        public static Texture2D GetAssetPreviewFromPath(string path, FetchPreviewOptions previewOptions)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex)
                return tex;

            if (!previewOptions.HasFlag(FetchPreviewOptions.Large))
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == typeof(AudioClip))
                    return GetAssetThumbnailFromPath(path);
            }

            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null)
                return null;
            return GetAssetPreview(obj, previewOptions);
        }

        public static Texture2D GetAssetPreview(UnityEngine.Object obj, FetchPreviewOptions previewOptions)
        {
            var preview = AssetPreview.GetAssetPreview(obj);
            if (preview == null || previewOptions.HasFlag(FetchPreviewOptions.Large))
            {
                var largePreview = AssetPreview.GetMiniThumbnail(obj);
                if (preview == null || (largePreview != null && largePreview.width > preview.width))
                    preview = largePreview;
            }
            return preview;
        }

        internal static int Wrap(int index, int n)
        {
            return ((index % n) + n) % n;
        }

        public static void SelectObject(UnityEngine.Object obj, bool ping = false)
        {
            if (!obj)
                return;
            Selection.activeObject = obj;
            if (ping)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorWindow.FocusWindowIfItsOpen(GetProjectBrowserWindowType());
                    EditorApplication.delayCall += () => EditorGUIUtility.PingObject(obj);
                };
            }
        }

        public static UnityEngine.Object SelectAssetFromPath(string path, bool ping = false)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            SelectObject(asset, ping);
            return asset;
        }

        public static void FrameAssetFromPath(string path)
        {
            var asset = SelectAssetFromPath(path);
            if (asset != null)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorWindow.FocusWindowIfItsOpen(GetProjectBrowserWindowType());
                    EditorApplication.delayCall += () => EditorGUIUtility.PingObject(asset);
                };
            }
            else
            {
                EditorUtility.RevealInFinder(path);
            }
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        internal static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
            return TypeCache.GetTypesDerivedFrom(aType).ToArray();
        }

        internal static string FormatProviderList(IEnumerable<SearchProvider> providers, bool fullTimingInfo = false, bool showFetchTime = true)
        {
            return string.Join(fullTimingInfo ? "\r\n" : ", ", providers.Select(p =>
            {
                var fetchTime = p.fetchTime;
                if (fullTimingInfo)
                    return $"{p.name.displayName} ({fetchTime:0.#} ms, Enable: {p.enableTime:0.#} ms, Init: {p.loadTime:0.#} ms)";

                var avgTimeLabel = String.Empty;
                if (showFetchTime && fetchTime > 9.99)
                    avgTimeLabel = $" ({fetchTime:#} ms)";
                return $"<b>{p.name.displayName}</b>{avgTimeLabel}";
            }));
        }

        public static string FormatBytes(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static string ToGuid(string assetPath)
        {
            string metaFile = $"{assetPath}.meta";
            if (!File.Exists(metaFile))
                return null;

            string line;
            using (var file = new StreamReader(metaFile))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.StartsWith("guid:", StringComparison.Ordinal))
                        continue;
                    return line.Substring(6);
                }
            }

            return null;
        }

        private static bool IsIgnoredAssembly(AssemblyName assemblyName)
        {
            var name = assemblyName.Name;
            return _ignoredAssemblies.Any(candidate => Regex.IsMatch(name, candidate));
        }

        internal static MethodInfo[] GetAllStaticMethods(this AppDomain aAppDomain, bool showInternalAPIs)
        {
            var result = new List<MethodInfo>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (IsIgnoredAssembly(assembly.GetName()))
                    continue;
                var types = assembly.GetLoadableTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | (showInternalAPIs ? BindingFlags.Public | BindingFlags.NonPublic : BindingFlags.Public) | BindingFlags.DeclaredOnly);
                    foreach (var m in methods)
                    {
                        if (m.IsPrivate)
                            continue;

                        if (m.IsGenericMethod)
                            continue;

                        if (m.Name.Contains("Begin") || m.Name.Contains("End"))
                            continue;

                        if (m.GetParameters().Length == 0)
                            result.Add(m);
                    }
                }
            }
            return result.ToArray();
        }

        static UnityEngine.Object s_MainWindow = null;
        internal static Rect GetEditorMainWindowPos()
        {
            if (s_MainWindow == null)
            {
                var containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).FirstOrDefault(t => t.Name == "ContainerWindow");
                if (containerWinType == null)
                    throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
                var showModeField = containerWinType.GetField("m_ShowMode", BindingFlags.NonPublic | BindingFlags.Instance);
                if (showModeField == null)
                    throw new MissingFieldException("Can't find internal fields 'm_ShowMode'. Maybe something has changed inside Unity");
                var windows = Resources.FindObjectsOfTypeAll(containerWinType);
                foreach (var win in windows)
                {
                    var showMode = (int)showModeField.GetValue(win);
                    if (showMode == 4) // main window
                    {
                        s_MainWindow = win;
                        break;
                    }
                }
            }

            if (s_MainWindow == null)
                return new Rect(0, 0, 800, 600);

            var positionProperty = s_MainWindow.GetType().GetProperty("position", BindingFlags.Public | BindingFlags.Instance);
            if (positionProperty == null)
                throw new MissingFieldException("Can't find internal fields 'position'. Maybe something has changed inside Unity.");
            return (Rect)positionProperty.GetValue(s_MainWindow, null);
        }

        internal static Rect GetCenteredWindowPosition(Rect parentWindowPosition, Vector2 size)
        {
            var pos = new Rect
            {
                x = 0, y = 0,
                width = Mathf.Min(size.x, parentWindowPosition.width * 0.90f),
                height = Mathf.Min(size.y, parentWindowPosition.height * 0.90f)
            };
            var w = (parentWindowPosition.width - pos.width) * 0.5f;
            var h = (parentWindowPosition.height - pos.height) * 0.5f;
            pos.x = parentWindowPosition.x + w;
            pos.y = parentWindowPosition.y + h;
            return pos;
        }

        internal static IEnumerable<MethodInfo> GetAllMethodsWithAttribute<T>() where T : System.Attribute
        {
            return TypeCache.GetMethodsWithAttribute<T>();
        }

        internal static IEnumerable<MethodInfo> GetAllMethodsWithAttribute(Type attributeType)
        {
            return TypeCache.GetMethodsWithAttribute(attributeType);
        }

        internal static Rect GetMainWindowCenteredPosition(Vector2 size)
        {
            var mainWindowRect = GetEditorMainWindowPos();
            return GetCenteredWindowPosition(mainWindowRect, size);
        }

        internal static void ShowDropDown(this EditorWindow window, Vector2 size)
        {
            window.maxSize = window.minSize = size;
            window.position = GetMainWindowCenteredPosition(size);
            window.ShowPopup();

            Assembly assembly = typeof(EditorWindow).Assembly;

            var editorWindowType = typeof(EditorWindow);
            var hostViewType = assembly.GetType("UnityEditor.HostView");
            var containerWindowType = assembly.GetType("UnityEditor.ContainerWindow");

            var parentViewField = editorWindowType.GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var parentViewValue = parentViewField.GetValue(window);

            hostViewType.InvokeMember("AddToAuxWindowList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, parentViewValue, null);

            // Dropdown windows should not be saved to layout
            var containerWindowProperty = hostViewType.GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
            var parentContainerWindowValue = containerWindowProperty.GetValue(parentViewValue);
            var dontSaveToLayoutField = containerWindowType.GetField("m_DontSaveToLayout", BindingFlags.Instance | BindingFlags.NonPublic);
            dontSaveToLayoutField.SetValue(parentContainerWindowValue, true);
            Debug.Assert((bool) dontSaveToLayoutField.GetValue(parentContainerWindowValue));
        }

        internal static string JsonSerialize(object obj)
        {
            var assembly = typeof(Selection).Assembly;
            var managerType = assembly.GetTypes().First(t => t.Name == "Json");
            var method = managerType.GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static);
            var jsonString = "";
            var arguments = new object[] { obj, false, "  " };
            jsonString = method.Invoke(null, arguments) as string;
            return jsonString;
        }

        internal static object JsonDeserialize(object obj)
        {
            Assembly assembly = typeof(Selection).Assembly;
            var managerType = assembly.GetTypes().First(t => t.Name == "Json");
            var method = managerType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);
            var arguments = new object[] { obj };
            return method.Invoke(null, arguments);
        }

        private static MethodInfo s_GetNumCharactersThatFitWithinWidthMethod;
        internal static int GetNumCharactersThatFitWithinWidth(GUIStyle style, string text, float width)
        {
            if (s_GetNumCharactersThatFitWithinWidthMethod == null)
            {
                var kType = typeof(GUIStyle);
                s_GetNumCharactersThatFitWithinWidthMethod = kType.GetMethod("Internal_GetNumCharactersThatFitWithinWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            var arguments = new object[] { text, width };
            return (int)s_GetNumCharactersThatFitWithinWidthMethod.Invoke(style, arguments);
        }

        private static MethodInfo s_GetPackagesPathsMethod;
        internal static string[] GetPackagesPaths()
        {
            if (s_GetPackagesPathsMethod == null)
            {
                Assembly assembly = typeof(UnityEditor.PackageManager.Client).Assembly;
                var type = assembly.GetTypes().First(t => t.FullName == "UnityEditor.PackageManager.Folders");
                s_GetPackagesPathsMethod = type.GetMethod("GetPackagesPaths", BindingFlags.Public | BindingFlags.Static);
            }
            return (string[])s_GetPackagesPathsMethod.Invoke(null, null);
        }

        internal static string GetQuickSearchVersion()
        {
            string version = null;
            try
            {
                var filePath = File.ReadAllText($"{packageFolderName}/package.json");
                if (JsonDeserialize(filePath) is Dictionary<string, object> manifest && manifest.ContainsKey("version"))
                {
                    version = manifest["version"] as string;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return version ?? "unknown";
        }

        internal static string GetQuickSearchDocVersion()
        {
            var version = GetQuickSearchVersion();
            var docVersion = version.Split('.');
            if (docVersion.Length > 2)
            {
                return $"@{docVersion[0]}.{docVersion[1]}";
            }
            else
            {
                return "@latest";
            }
        }

        internal static string GetNextWord(string src, ref int index)
        {
            // Skip potential white space BEFORE the actual word we are extracting
            for (; index < src.Length; ++index)
            {
                if (!char.IsWhiteSpace(src[index]))
                {
                    break;
                }
            }

            var startIndex = index;
            for (; index < src.Length; ++index)
            {
                if (char.IsWhiteSpace(src[index]))
                {
                    break;
                }
            }

            return src.Substring(startIndex, index - startIndex);
        }

        public static string GetPackagePath(string relativePath)
        {
            return Path.Combine(packageFolderName, relativePath).Replace("\\", "/");
        }

        public static T LoadPackageAsset<T>(string relativePath) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(GetPackagePath(relativePath));
        }

        public static int LevenshteinDistance<T>(IEnumerable<T> lhs, IEnumerable<T> rhs) where T : System.IEquatable<T>
        {
            if (lhs == null) throw new System.ArgumentNullException("lhs");
            if (rhs == null) throw new System.ArgumentNullException("rhs");

            IList<T> first = lhs as IList<T> ?? new List<T>(lhs);
            IList<T> second = rhs as IList<T> ?? new List<T>(rhs);

            int n = first.Count, m = second.Count;
            if (n == 0) return m;
            if (m == 0) return n;

            int curRow = 0, nextRow = 1;
            int[][] rows = { new int[m + 1], new int[m + 1] };
            for (int j = 0; j <= m; ++j)
                rows[curRow][j] = j;

            for (int i = 1; i <= n; ++i)
            {
                rows[nextRow][0] = i;

                for (int j = 1; j <= m; ++j)
                {
                    int dist1 = rows[curRow][j] + 1;
                    int dist2 = rows[nextRow][j - 1] + 1;
                    int dist3 = rows[curRow][j - 1] +
                        (first[i - 1].Equals(second[j - 1]) ? 0 : 1);

                    rows[nextRow][j] = System.Math.Min(dist1, System.Math.Min(dist2, dist3));
                }
                if (curRow == 0)
                {
                    curRow = 1;
                    nextRow = 0;
                }
                else
                {
                    curRow = 0;
                    nextRow = 1;
                }
            }
            return rows[curRow][m];
        }

        public static int LevenshteinDistance(string lhs, string rhs, bool caseSensitive = true)
        {
            if (!caseSensitive)
            {
                lhs = lhs.ToLower();
                rhs = rhs.ToLower();
            }
            char[] first = lhs.ToCharArray();
            char[] second = rhs.ToCharArray();
            return LevenshteinDistance(first, second);
        }

        internal static Texture2D GetThumbnailForGameObject(GameObject go)
        {
            var thumbnail = PrefabUtility.GetIconForGameObject(go);
            if (thumbnail)
                return thumbnail;
            return EditorGUIUtility.ObjectContent(go, go.GetType()).image as Texture2D;
        }

        private static MethodInfo s_FindTextureMethod;
        internal static Texture2D FindTextureForType(Type type)
        {
            if (s_FindTextureMethod == null)
            {
                var t = typeof(EditorGUIUtility);
                s_FindTextureMethod = t.GetMethod("FindTexture", BindingFlags.NonPublic| BindingFlags.Static);
            }
            return (Texture2D)s_FindTextureMethod.Invoke(null, new object[]{type});
        }

        private static MethodInfo s_GetIconForObject;
        internal static Texture2D GetIconForObject(UnityEngine.Object obj)
        {
            if (s_GetIconForObject == null)
            {
                var t = typeof(EditorGUIUtility);
                s_GetIconForObject = t.GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            }
            return (Texture2D)s_GetIconForObject.Invoke(null, new object[] { obj });
        }

        internal static void PingAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                if (!(asset is GameObject))
                    Resources.UnloadAsset(asset);
            }
        }

        #if UNITY_2020_1_OR_NEWER

        private static MethodInfo s_OpenPropertyEditorMethod;
        internal static EditorWindow OpenPropertyEditor(UnityEngine.Object obj)
        {
            if (s_OpenPropertyEditorMethod == null)
            {
                Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
                var type = assembly.GetTypes().First(t => t.FullName == "UnityEditor.PropertyEditor");
                s_OpenPropertyEditorMethod = type.GetMethod("OpenPropertyEditor", BindingFlags.NonPublic | BindingFlags.Static);
                if (s_OpenPropertyEditorMethod == null)
                    return null;
            }
            return (EditorWindow)s_OpenPropertyEditorMethod.Invoke(null, new object[] {obj, true});
        }

        // TODO: Fix issue if PingUIElement is called more than once before delayCall is called, locking the window with the new style
        internal static void PingUIElement(VisualElement element, [CanBeNull] EditorWindow window)
        {
            var s = element.style;
            var oldBorderTopColor = s.borderTopColor;
            var oldBorderBottomColor = s.borderBottomColor;
            var oldBorderLeftColor = s.borderLeftColor;
            var oldBorderRightColor = s.borderRightColor;
            var oldBorderTopWidth = s.borderTopWidth;
            var oldBorderBottomWidth = s.borderBottomWidth;
            var oldBorderLeftWidth = s.borderLeftWidth;
            var oldBorderRightWidth = s.borderRightWidth;

            s.borderTopWidth = s.borderBottomWidth = s.borderLeftWidth = s.borderRightWidth = new StyleFloat(2);
            s.borderTopColor = s.borderBottomColor = s.borderLeftColor = s.borderRightColor = new StyleColor(Color.cyan);

            element.Focus();

            DelayCall(1f, () =>
            {
                s.borderTopColor = oldBorderTopColor;
                s.borderBottomColor = oldBorderBottomColor;
                s.borderLeftColor = oldBorderLeftColor;
                s.borderRightColor = oldBorderRightColor;
                s.borderTopWidth = oldBorderTopWidth;
                s.borderBottomWidth = oldBorderBottomWidth;
                s.borderLeftWidth = oldBorderLeftWidth;
                s.borderRightWidth = oldBorderRightWidth;

                if (window)
                    window.Repaint();
            });
        }
        #endif

        internal static void DelayCall(float seconds, System.Action callback)
        {
            DelayCall(EditorApplication.timeSinceStartup, seconds, callback);
        }

        internal static void DelayCall(double timeStart, float seconds, System.Action callback)
        {
            var dt = EditorApplication.timeSinceStartup - timeStart;
            if (dt >= seconds)
                callback();
            else
                EditorApplication.delayCall += () => DelayCall(timeStart, seconds, callback);
        }

        public static T ConvertValue<T>(string value)
        {
            var type = typeof(T);
            var converter = TypeDescriptor.GetConverter(type);
            if (converter.IsValid(value))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
            }
            return (T)Activator.CreateInstance(type);
        }

        public static bool TryConvertValue<T>(string value, out T convertedValue)
        {
            var type = typeof(T);
            var converter = TypeDescriptor.GetConverter(type);
            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                convertedValue = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
                return true;
            }
            catch
            {
                convertedValue = default;
                return false;
            }
        }

        private static UnityEngine.Object[] s_LastDraggedObjects;
        internal static void StartDrag(UnityEngine.Object[] objects, string label = null)
        {
            s_LastDraggedObjects = objects;
            if (s_LastDraggedObjects == null)
                return;
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = s_LastDraggedObjects;
            DragAndDrop.StartDrag(label);
        }

        internal static void StartDrag(UnityEngine.Object[] objects, string[] paths, string label = null)
        {
            s_LastDraggedObjects = objects;
            if (paths == null || paths.Length == 0)
                return;
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = s_LastDraggedObjects;
            DragAndDrop.paths = paths;
            DragAndDrop.StartDrag(label);
        }

        private static MethodInfo s_GetFieldInfoFromProperty;
        internal static FieldInfo GetFieldInfoFromProperty(SerializedProperty property, out Type requiredType)
        {
            requiredType = null;
            if (s_GetFieldInfoFromProperty == null)
            {
                Assembly assembly = typeof(UnityEditor.SerializedProperty).Assembly;
                var type = assembly.GetTypes().First(t => t.FullName == "UnityEditor.ScriptAttributeUtility");
                s_GetFieldInfoFromProperty = type.GetMethod("GetFieldInfoFromProperty", BindingFlags.NonPublic | BindingFlags.Static);
                if (s_GetFieldInfoFromProperty == null)
                    return null;
            }
            object[] parameters = new object[]{ property, null };
            var fi = (FieldInfo)s_GetFieldInfoFromProperty.Invoke(null, parameters);
            requiredType = parameters[1] as Type;
            return fi;
        }

        private static MethodInfo s_GetSourceAssetFileHash;
        internal static Hash128 GetSourceAssetFileHash(string guid)
        {
            if (s_GetSourceAssetFileHash == null)
            {
                var type = typeof(UnityEditor.AssetDatabase);
                s_GetSourceAssetFileHash = type.GetMethod("GetSourceAssetFileHash", BindingFlags.NonPublic | BindingFlags.Static);
                if (s_GetSourceAssetFileHash == null)
                    return default;
            }
            object[] parameters = new object[] { guid };
            return (Hash128)s_GetSourceAssetFileHash.Invoke(null, parameters);
        }

        public static void LogProperties(SerializedObject so, bool includeChildren = true)
        {
            so.Update();
            SerializedProperty propertyLogger = so.GetIterator();
            while (true)
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, so.targetObject, $"{propertyLogger.propertyPath} [{propertyLogger.type}]");
                if (!propertyLogger.Next(includeChildren))
                    break;
            }
        }

        public static Type GetTypeFromName(string typeName)
        {
            return TypeCache.GetTypesDerivedFrom<UnityEngine.Object>().FirstOrDefault(t => t.Name == typeName) ?? typeof(UnityEngine.Object);
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        /// <summary>
        /// Converts a search item into any valid UnityEngine.Object if possible.
        /// </summary>
        /// <param name="item">Item to be converted</param>
        /// <param name="filterType">The object should be converted in this type if possible.</param>
        /// <returns></returns>
        public static UnityEngine.Object ToObject(SearchItem item, Type filterType)
        {
            if (item == null || item.provider == null)
                return null;
            return item.provider.toObject?.Invoke(item, filterType);
        }

        /// <summary>
        /// Checks if the previously focused window used to open quick search is of a given type.
        /// </summary>
        /// <param name="focusWindowName">Class name of the window to be verified.</param>
        /// <returns>True if the class name matches the quick search opener window class name.</returns>
        public static bool IsFocusedWindowTypeName(string focusWindowName)
        {
            return EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().ToString().EndsWith("." + focusWindowName);
        }

        internal static string CleanString(string s)
        {
            var sb = s.ToCharArray();
            for (int c = 0; c < s.Length; ++c)
            {
                var ch = s[c];
                if (ch == '_' || ch == '.' || ch == '-' || ch == '/')
                    sb[c] = ' ';
            }
            return new string(sb).ToLowerInvariant();
        }

        public static string CleanPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static bool IsPathUnderProject(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = new FileInfo(path).FullName;
            }

            return CleanPath(path).StartsWith(Application.dataPath + "/");
        }

        public static string GetPathUnderProject(string path)
        {
            var cleanPath = CleanPath(path);
            if (!Path.IsPathRooted(cleanPath) || !path.StartsWith(Application.dataPath))
            {
                return cleanPath;
            }

            return cleanPath.Substring(Application.dataPath.Length - 6);
        }

        public static Texture2D GetSceneObjectPreview(GameObject obj, FetchPreviewOptions options, Texture2D defaultThumbnail)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr && sr.sprite && sr.sprite.texture)
                return sr.sprite.texture;

            #if PACKAGE_UGUI
            var uii = obj.GetComponent<UnityEngine.UI.Image>();
            if (uii && uii.mainTexture is Texture2D uiit)
                return uiit;
            #endif

            var preview = AssetPreview.GetAssetPreview(obj);
            if (preview)
                return preview;

            var assetPath = SearchUtils.GetHierarchyAssetPath(obj, true);
            if (String.IsNullOrEmpty(assetPath))
                return defaultThumbnail;
            return GetAssetPreviewFromPath(assetPath, options);
        }

        private static object[] s_SearchFilterArgs;
        private static MethodInfo s_FindAllAssetsMethod;
        private static MethodInfo s_SearchFieldStringToFilterMethod;
        public static IEnumerable<string> FindAssets(string searchQuery)
        {
            if (s_FindAllAssetsMethod == null)
            {
                var type = typeof(AssetDatabase);
                s_FindAllAssetsMethod = type.GetMethod("FindAllAssets", BindingFlags.NonPublic | BindingFlags.Static);
                Debug.Assert(s_FindAllAssetsMethod != null);

                var searchFilterType = type.Assembly.GetTypes().First(t => t.Name == "SearchFilter");
                s_SearchFilterArgs = new object[] { Activator.CreateInstance(searchFilterType) };

                s_SearchFieldStringToFilterMethod = searchFilterType.GetMethod("SearchFieldStringToFilter", BindingFlags.NonPublic | BindingFlags.Instance);
                Debug.Assert(s_SearchFieldStringToFilterMethod != null);
            }

            s_SearchFieldStringToFilterMethod.Invoke(s_SearchFilterArgs[0], new object[] { searchQuery });

            IEnumerable<HierarchyProperty> properties = null;
            try
            {
                properties = (IEnumerable<HierarchyProperty>)s_FindAllAssetsMethod.Invoke(null, s_SearchFilterArgs);
            }
            catch
            {
                // Ignore these errors.
            }
            return properties != null ? properties.Select(p => AssetDatabase.GUIDToAssetPath(p.guid)) : new string[0];
        }

        public static bool TryGetNumber(object value, out double number)
        {
            number = double.NaN;
            if (value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal)
            {
                number = Convert.ToDouble(value);
                return true;
            }

            return double.TryParse(Convert.ToString(value), out number);
        }

        public static bool IsRunningTests()
        {
            return !InternalEditorUtility.isHumanControllingUs || InternalEditorUtility.inBatchMode;
        }
    }
}
