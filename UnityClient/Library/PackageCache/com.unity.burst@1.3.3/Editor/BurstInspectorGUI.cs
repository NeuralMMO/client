using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst.LowLevel;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Unity.Burst.Editor
{
    internal enum DisassemblyKind
    {
        Asm = 0,
        IL = 1,
        UnoptimizedIR = 2,
        OptimizedIR = 3,
        IRPassAnalysis = 4
    }

    internal enum AssemblyKind
    {
        RawNoDebugInformation,
        RawWithDebugInformation,
        EnhancedMinimalDebugInformation,
        EnhancedFullDebugInformation,
        ColouredMinimalDebugInformation,
        ColouredFullDebugInformation,
    }

    internal class BurstInspectorGUI : EditorWindow
    {
        private const string FontSizeIndexPref = "BurstInspectorFontSizeIndex";

        private static readonly string[] DisassemblyKindNames =
        {
            "Assembly",
            ".NET IL",
            "LLVM IR (Unoptimized)",
            "LLVM IR (Optimized)",
            "LLVM IR Optimisation Diagnostics"
        };

        private static readonly string[] AssemblyOptions =
        {
            "Plain (No debug information)",
            "Plain (With debug information)",
            "Enhanced (Minimal debug information)",
            "Enhanced (Full debug information)",
            "Coloured (Minimal debug information)",
            "Coloured (Full debug information)"
        };

        private static readonly string[] DisasmOptions =
        {
            "\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDump, NativeDumpFlags.Asm),
            "\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDump, NativeDumpFlags.IL),
            "\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDump, NativeDumpFlags.IR),
            "\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDump, NativeDumpFlags.IROptimized),
            "\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDump, NativeDumpFlags.IRPassAnalysis)
        };

        private static readonly SplitterState TreeViewSplitterState = new SplitterState(new float[] { 30, 70 }, new int[] { 128, 128 }, null);

        private static readonly string[] TargetCpuNames = Enum.GetNames(typeof(TargetCpu));

        private static readonly int[] FontSizes =
        {
            8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20
        };

        private static string[] _fontSizesText;

        [NonSerialized]
        private readonly BurstDisassembler _burstDisassembler;

        [SerializeField] private TargetCpu _targetCpu = TargetCpu.Auto;

        [SerializeField] private DisassemblyKind _disasmKind = DisassemblyKind.Asm;

        [NonSerialized]
        private GUIStyle _fixedFontStyle;

        [NonSerialized]
        private int _fontSizeIndex = -1;

        [NonSerialized]
        private int _previousTargetIndex = -1;

        [SerializeField] private bool _safetyChecks;
        [SerializeField] private bool _enhancedDisassembly = true;
        [SerializeField] private int _assemblyKind = -1;

        private int _assemblyKindPrior = -1;
        private bool _sameTargetButDifferentAssemblyKind = false;
        private Vector2 _scrollPos;
        private SearchField _searchField;

        [SerializeField] private string _selectedItem;

        [NonSerialized]
        private List<BurstCompileTarget> _targets;

        [NonSerialized]
        private LongTextArea _textArea;

        [NonSerialized]
        private Font _font;

        [NonSerialized]
        private BurstMethodTreeView _treeView;

        [NonSerialized]
        private bool _initialized;

        [NonSerialized]
        private bool _requiresRepaint;

        private int FontSize => FontSizes[_fontSizeIndex];

        public BurstInspectorGUI()
        {
            _burstDisassembler = new BurstDisassembler();
            _assemblyKindPrior = _assemblyKind;
        }

        public void OnEnable()
        {
            if (_treeView == null) _treeView = new BurstMethodTreeView(new TreeViewState());
            _safetyChecks = BurstCompiler.Options.EnableBurstSafetyChecks;

            var assemblyList = BurstReflection.GetAssemblyList(AssembliesType.Editor, onlyAssembliesThatPossiblyContainJobs: true);

            Task.Run(
                () =>
                {
                    // Do this stuff asynchronously.
                    var result = BurstReflection.FindExecuteMethods(assemblyList);
                    _targets = result.CompileTargets;
                    _targets.Sort((left, right) => string.Compare(left.GetDisplayName(), right.GetDisplayName(), StringComparison.Ordinal));
                    return result;
                })
                .ContinueWith(t =>
                {
                    // Do this stuff on the main (UI) thread.
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        foreach (var logMessage in t.Result.LogMessages)
                        {
                            switch (logMessage.LogType)
                            {
                                case BurstReflection.LogType.Warning:
                                    Debug.LogWarning(logMessage.Message);
                                    break;
                                case BurstReflection.LogType.Exception:
                                    Debug.LogException(logMessage.Exception);
                                    break;
                                default:
                                    throw new InvalidOperationException();
                            }
                        }

                        _treeView.Targets = _targets;
                        _treeView.Reload();

                        if (_selectedItem != null)
                        {
                            _treeView.TrySelectByDisplayName(_selectedItem);
                        }

                        _requiresRepaint = true;
                        _initialized = true;
                    }
                    else if (t.Exception != null)
                    {
                        Debug.LogError($"Could not load Inspector: {t.Exception}");
                    }
                });
        }

        private void CleanupFont()
        {
            if (_font != null)
            {
                DestroyImmediate(_font, true);
                _font = null;
            }
        }

        public void OnDisable()
        {
            CleanupFont();
        }

        public void Update()
        {
            // Need to do this because if we call Repaint from anywhere else,
            // it doesn't do anything if this window is not currently focused.
            if (_requiresRepaint)
            {
                Repaint();
                _requiresRepaint = false;
            }
        }

        private void FlowToNewLine(ref float remainingWidth, float resetWidth, GUIStyle style, GUIContent content)
        {
            float spaceRemainingBeforeNewLine = EditorStyles.toggle.CalcSize(new GUIContent("WWWW")).x;

            remainingWidth -= style.CalcSize(content).x;
            if (remainingWidth <= spaceRemainingBeforeNewLine)
            {
                remainingWidth = resetWidth;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
        }

        private bool IsRaw(AssemblyKind kind)
        {
            return kind == AssemblyKind.RawNoDebugInformation || kind == AssemblyKind.RawWithDebugInformation;
        }

        private bool IsEnhanced(AssemblyKind kind)
        {
            return !IsRaw(kind);
        }

        private bool IsColoured(AssemblyKind kind)
        {
            return kind == AssemblyKind.ColouredMinimalDebugInformation || kind == AssemblyKind.ColouredFullDebugInformation;
        }

        private void RenderButtonBars(float width, BurstCompileTarget target, out bool doCopy, out int fontIndex)
        {
            float remainingWidth = width;

            var contentDisasm = new GUIContent("Enhanced Disassembly");
            var contentSafety = new GUIContent("Safety Checks");
            var contentCodeGenOptions = new GUIContent("Auto");
            var contentLabelFontSize = new GUIContent("Font Size");
            var contentFontSize = new GUIContent("99");
            var contentCopyToClip = new GUIContent("Copy to Clipboard");

            GUILayout.BeginHorizontal();

            _assemblyKind = EditorGUILayout.Popup(_assemblyKind, AssemblyOptions, EditorStyles.popup);
            FlowToNewLine(ref remainingWidth, width, EditorStyles.popup, contentDisasm);

            _safetyChecks = GUILayout.Toggle(_safetyChecks, contentSafety, EditorStyles.toggle);
            FlowToNewLine(ref remainingWidth, width, EditorStyles.toggle, contentSafety);

            EditorGUI.BeginDisabledGroup(!target.HasRequiredBurstCompileAttributes);

            _targetCpu = (TargetCpu)EditorGUILayout.Popup((int)_targetCpu, TargetCpuNames, EditorStyles.popup);
            FlowToNewLine(ref remainingWidth, width, EditorStyles.popup, contentCodeGenOptions);

            GUILayout.Label("Font Size", EditorStyles.label);
            fontIndex = EditorGUILayout.Popup(_fontSizeIndex, _fontSizesText, EditorStyles.popup);
            FlowToNewLine(ref remainingWidth, width, EditorStyles.label,contentLabelFontSize);
            FlowToNewLine(ref remainingWidth, width, EditorStyles.popup,contentFontSize);

            doCopy = GUILayout.Button(contentCopyToClip, EditorStyles.miniButton);
            FlowToNewLine(ref remainingWidth, width, EditorStyles.miniButton,contentCopyToClip);
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            _disasmKind = (DisassemblyKind) GUILayout.Toolbar((int) _disasmKind, DisassemblyKindNames, GUILayout.ExpandWidth(true), GUILayout.MinWidth(5*10));
        }

        public void OnGUI()
        {
            if (!_initialized)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Loading...");
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                return;
            }

            // Make sure that editor options are synchronized
            BurstEditorOptions.EnsureSynchronized();

            if (_fontSizesText == null)
            {
                _fontSizesText = new string[FontSizes.Length];
                for (var i = 0; i < FontSizes.Length; ++i) _fontSizesText[i] = FontSizes[i].ToString();
            }

            if (_fontSizeIndex == -1)
            {
                _fontSizeIndex = EditorPrefs.GetInt(FontSizeIndexPref, 5);
                _fontSizeIndex = Math.Max(0, _fontSizeIndex);
                _fontSizeIndex = Math.Min(_fontSizeIndex, FontSizes.Length - 1);
            }

            if (_fixedFontStyle == null)
            {
                _fixedFontStyle = new GUIStyle(GUI.skin.label);
                string fontName;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                  fontName = "Consolas";
                else
                  fontName = "Courier";

                CleanupFont();

                _font = Font.CreateDynamicFontFromOSFont(fontName, FontSize);
                _fixedFontStyle.font = _font;
                _fixedFontStyle.fontSize = FontSize;
            }

            if (_searchField == null) _searchField = new SearchField();

            if (_textArea == null) _textArea = new LongTextArea();

            GUILayout.BeginHorizontal();

            // SplitterGUILayout.BeginHorizontalSplit is internal in Unity but we don't have much choice
            SplitterGUILayout.BeginHorizontalSplit(TreeViewSplitterState);

            GUILayout.BeginVertical(GUILayout.Width(position.width / 3));

            GUILayout.Label("Compile Targets", EditorStyles.boldLabel);

            var newFilter = _searchField.OnGUI(_treeView.Filter);

            if (newFilter != _treeView.Filter)
            {
                _treeView.Filter = newFilter;
                _treeView.Reload();
            }

            _treeView.OnGUI(GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)));

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            var selection = _treeView.GetSelection();
            if (selection.Count == 1)
            {
                var targetIndex = selection[0];
                var target = _targets[targetIndex - 1];
                var targetOptions = target.Options;

                // Stash selected item name to handle domain reloads more gracefully
                _selectedItem = target.GetDisplayName();

                if (_assemblyKind == -1)
                {
                    if (_enhancedDisassembly)
                    {
                        _assemblyKind = (int)AssemblyKind.ColouredMinimalDebugInformation;
                    }
                    else
                    {
                        _assemblyKind = (int)AssemblyKind.RawNoDebugInformation;
                    }
                }
                
                // Refresh if any options are changed
                bool doCopy;
                int fontSize;
                // -14 to add a little bit of space for the vertical scrollbar to display correctly
                RenderButtonBars((position.width*2)/3 - 14, target, out doCopy, out fontSize);

                // We are currently formatting only Asm output
                var isTextFormatted = IsEnhanced((AssemblyKind)_assemblyKind) && _disasmKind == DisassemblyKind.Asm;

                // Depending if we are formatted or not, we don't render the same text
                var textToRender = isTextFormatted ? target.FormattedDisassembly : target.RawDisassembly;

                // Only refresh if we are switching to a new selection that hasn't been disassembled yet
                // Or we are changing disassembly settings (safety checks / enhanced disassembly)
                var targetRefresh = textToRender == null
                                    || target.DisassemblyKind != _disasmKind
                                    || targetOptions.EnableBurstSafetyChecks != _safetyChecks
                                    || target.TargetCpu != _targetCpu
                                    || target.IsDarkMode != EditorGUIUtility.isProSkin;

                bool targetChanged = _previousTargetIndex != targetIndex;

                _previousTargetIndex = targetIndex;

                if (_assemblyKindPrior != _assemblyKind)
                {
                    targetRefresh = true;
                    _assemblyKindPrior = _assemblyKind;     // Needs to be refreshed, as we need to change disassembly options

                    // If the target did not changed but our assembly kind did, we need to remember this.
                    if (!targetChanged)
                    {
                        _sameTargetButDifferentAssemblyKind = true;
                    }
                }

                // If the previous target changed the assembly kind and we have a target change, we need to
                // refresh the assembly because we'll have cached the previous assembly kinds output rather
                // than the one requested.
                if (_sameTargetButDifferentAssemblyKind && targetChanged)
                {
                    targetRefresh = true;
                    _sameTargetButDifferentAssemblyKind = false;
                }

                if (targetRefresh)
                {
                    // TODO: refactor this code with a proper AppendOption to avoid these "\n"
                    var options = new StringBuilder();

                    target.TargetCpu = _targetCpu;
                    target.DisassemblyKind = _disasmKind;
                    targetOptions.EnableBurstSafetyChecks = _safetyChecks;
                    target.IsDarkMode = EditorGUIUtility.isProSkin;
                    targetOptions.EnableBurstCompileSynchronously = true;

                    string defaultOptions;
                    if (targetOptions.TryGetOptions(target.IsStaticMethod ? (MemberInfo)target.Method : target.JobType, true, out defaultOptions))
                    {
                        options.Append(defaultOptions);

                        options.AppendFormat("\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionTarget, TargetCpuNames[(int)_targetCpu]));

                        switch ((AssemblyKind)_assemblyKind)
                        {
                            case AssemblyKind.EnhancedMinimalDebugInformation:
                            case AssemblyKind.ColouredMinimalDebugInformation:
                                options.AppendFormat("\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDebug, "2"));
                                break;
                            case AssemblyKind.ColouredFullDebugInformation:
                            case AssemblyKind.EnhancedFullDebugInformation:
                            case AssemblyKind.RawWithDebugInformation:
                                options.AppendFormat("\n" + BurstCompilerOptions.GetOption(BurstCompilerOptions.OptionDebug, "1"));
                                break;
                            default:
                            case AssemblyKind.RawNoDebugInformation:
                                break;
                        }

                        var baseOptions = options.ToString().Trim('\n', ' ');

                        target.RawDisassembly = GetDisassembly(target.Method, baseOptions + DisasmOptions[(int)_disasmKind]);

                        if (isTextFormatted)
                        {
                            target.FormattedDisassembly = _burstDisassembler.Process(target.RawDisassembly, FetchAsmKind(_targetCpu), target.IsDarkMode, IsColoured((AssemblyKind)_assemblyKind));
                            textToRender = target.FormattedDisassembly;
                        }
                        else
                        {
                            target.FormattedDisassembly = null;
                            textToRender = target.RawDisassembly;
                        }
                    }
                }

                if (textToRender != null)
                {
                    _textArea.Text = textToRender;
                    if (targetChanged) _scrollPos = Vector2.zero;
                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, true, true);
                    _textArea.Render(_fixedFontStyle);
                    GUILayout.EndScrollView();
                }

                if (doCopy)
                {
                    // When copying to the clipboard, we copy the version the user sees
                    EditorGUIUtility.systemCopyBuffer = textToRender ?? string.Empty;
                }

                if (fontSize != _fontSizeIndex)
                {
                    _fontSizeIndex = fontSize;
                    EditorPrefs.SetInt(FontSizeIndexPref, fontSize);
                    _fixedFontStyle = null;
                }
            }

            GUILayout.EndVertical();

            SplitterGUILayout.EndHorizontalSplit();

            GUILayout.EndHorizontal();
        }

        private static string GetDisassembly(MethodInfo method, string options)
        {
            try
            {
                var result = BurstCompilerService.GetDisassembly(method, options);
                if (result.IndexOf('\t') >= 0)
                {
                    result = result.Replace("\t", "        ");
                }

                // Workaround to remove timings
                if (result.Contains("Burst timings"))
                {
                    var index = result.IndexOf("While compiling", StringComparison.Ordinal);
                    if (index > 0)
                    {
                        result = result.Substring(index);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                return "Failed to compile:\n" + e.Message;
            }
        }

        private static BurstDisassembler.AsmKind FetchAsmKind(TargetCpu cpu)
        {
            switch (cpu)
            {
                case TargetCpu.ARMV7A_NEON32:
                case TargetCpu.ARMV8A_AARCH64:
                case TargetCpu.ARMV8A_AARCH64_HALFFP:
                case TargetCpu.THUMB2_NEON32:
                    return BurstDisassembler.AsmKind.ARM;
                case TargetCpu.WASM32:
                    return BurstDisassembler.AsmKind.Wasm;
            }
            return BurstDisassembler.AsmKind.Intel;
        }
    }

    internal class BurstMethodTreeView : TreeView
    {
        public BurstMethodTreeView(TreeViewState state) : base(state)
        {
            showBorder = true;
        }

        public BurstMethodTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            showBorder = true;
        }

        public List<BurstCompileTarget> Targets { get; set; }
        public string Filter { get; set; }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            var allItems = new List<TreeViewItem>();

            if (Targets != null)
            {
                allItems.Capacity = Targets.Count;
                var id = 1;
                var filter = Filter;
                foreach (var t in Targets)
                {
                    var displayName = t.GetDisplayName();
                    if (string.IsNullOrEmpty(filter) || displayName.IndexOf(filter, 0, displayName.Length, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        allItems.Add(new TreeViewItem {id = id, depth = 0, displayName = displayName});

                    ++id;
                }
            }

            SetupParentsAndChildrenFromDepths(root, allItems);

            return root;
        }

        internal void TrySelectByDisplayName(string name)
        {
            var id = 1;
            foreach (var t in Targets)
                if (t.GetDisplayName() == name)
                {
                    SetSelection(new[] {id});
                    FrameItem(id);
                    break;
                }
                else
                {
                    ++id;
                }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var target = Targets[args.item.id - 1];
            var wasEnabled = GUI.enabled;
            GUI.enabled = target.HasRequiredBurstCompileAttributes;
            base.RowGUI(args);
            GUI.enabled = wasEnabled;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
    }
}
