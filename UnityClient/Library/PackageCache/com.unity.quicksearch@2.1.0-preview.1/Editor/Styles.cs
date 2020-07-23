using System;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    internal static class Styles
    {
        static Styles()
        {
            if (!isDarkTheme)
            {
                selectedItemLabel.normal.textColor = Color.white;
                selectedItemDescription.normal.textColor = Color.white;
            }

            statusWheel = new GUIContent[12];
            for (int i = 0; i < 12; i++)
                statusWheel[i] = EditorGUIUtility.IconContent("WaitSpin" + i.ToString("00"));
        }

        private const int itemRowPadding = 4;
        public const float actionButtonSize = 24f;
        public const float itemPreviewSize = 32f;
        public const float itemRowSpacing = 30.0f;
        public const float descriptionPadding = 2f;
        private const int actionButtonMargin = (int)((itemRowHeight - actionButtonSize) / 2f);
        public const float itemRowHeight = itemPreviewSize + itemRowPadding * 2f;
        public const float statusOffset = 20;
        public const int kSearchBoxBtnSize = 24;
        public const int kSearchFieldWidthOffset = 120;

        private static bool isDarkTheme => EditorGUIUtility.isProSkin;

        private static readonly RectOffset marginNone = new RectOffset(0, 0, 0, 0);
        private static readonly RectOffset paddingNone = new RectOffset(0, 0, 0, 0);
        private static readonly RectOffset defaultPadding = new RectOffset(itemRowPadding, itemRowPadding, itemRowPadding, itemRowPadding);

        private static readonly Color darkColor1 = new Color(61 / 255f, 61 / 255f, 61 / 255f);
        private static readonly Color darkColor2 = new Color(71 / 255f, 106 / 255f, 155 / 255f);
        private static readonly Color darkColor3 = new Color(68 / 255f, 68 / 255f, 71 / 255f);
        private static readonly Color darkColor4 = new Color(111 / 255f, 111 / 255f, 111 / 255f);
        private static readonly Color darkColor5 = new Color(71 / 255f, 71 / 255f, 71 / 255f);
        private static readonly Color darkColor6 = new Color(63 / 255f, 63 / 255f, 63 / 255f);
        private static readonly Color darkColor7 = new Color(71 / 255f, 71 / 255f, 71 / 255f);

        private static readonly Color lightColor1 = new Color(171 / 255f, 171 / 255f, 171 / 255f);
        private static readonly Color lightColor2 = new Color(71 / 255f, 106 / 255f, 155 / 255f);
        private static readonly Color lightColor3 = new Color(168 / 255f, 168 / 255f, 171 / 255f);
        private static readonly Color lightColor4 = new Color(111 / 255f, 111 / 255f, 111 / 255f);
        private static readonly Color lightColor5 = new Color(181 / 255f, 181 / 255f, 181 / 255f);
        private static readonly Color lightColor6 = new Color(214 / 255f, 214 / 255f, 214 / 255f);
        private static readonly Color lightColor7 = new Color(230 / 255f, 230 / 255f, 230 / 255f);

        public static readonly Color sliderColor = isDarkTheme ? new Color(71 / 255f, 71 / 255f, 71 / 255f) : new Color(230 / 255f, 230 / 255f, 230 / 255f);

        public static readonly string highlightedTextColorFormat = isDarkTheme ? "<color=#F6B93F>{0}</color>" : "<b>{0}</b>";

        private static readonly Color textAutoCompleteBgColorDark = new Color(37 / 255.0f, 37 / 255.0f, 38 / 255.0f);
        private static readonly Color textAutoCompleteBgColorLight = new Color(165 / 255.0f, 165 / 255.0f, 165 / 255.0f);
        public static readonly Color textAutoCompleteBgColor = isDarkTheme ? textAutoCompleteBgColorDark : textAutoCompleteBgColorLight;
        private static readonly Color textAutoCompleteSelectedColorDark = new Color(7 / 255.0f, 54 / 255.0f, 85 / 255.0f);
        private static readonly Color textAutoCompleteSelectedColorLight = new Color(58 / 255.0f, 114 / 255.0f, 176 / 255.0f);
        public static readonly Color textAutoCompleteSelectedColor = isDarkTheme ? textAutoCompleteSelectedColorDark : textAutoCompleteSelectedColorLight;

        private static readonly Texture2D buttonPressedBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor4 : lightColor4);
        private static readonly Texture2D buttonHoveredBackgroundImage = GenerateSolidColorTexture(isDarkTheme ? darkColor5 : lightColor5);

        private static readonly Texture2D searchFieldBg = GenerateSolidColorTexture(isDarkTheme ? darkColor6 : lightColor6);
        private static readonly Texture2D searchFieldFocusBg = GenerateSolidColorTexture(isDarkTheme ? darkColor7 : lightColor7);

        public static readonly GUIStyle panelBorder = new GUIStyle("grey_border")
        {
            name = "quick-search-border",
            padding = new RectOffset(1, 1, 1, 1),
            margin = new RectOffset(0, 0, 0, 0)
        };
        public static readonly GUIContent filterButtonContent = new GUIContent("", Icons.filter, "Open search filter window (Alt + Left)");
        public static readonly GUIContent moreActionsContent = new GUIContent("", Icons.more, "Open actions menu (Alt + Right)");

        public static readonly GUIStyle scrollbar = new GUIStyle("VerticalScrollbar");
        public static readonly float scrollbarWidth = scrollbar.fixedWidth + scrollbar.margin.horizontal;

        public static readonly GUIStyle itemBackground1 = new GUIStyle
        {
            name = "quick-search-item-background1",
            fixedHeight = 0,

            margin = marginNone,
            padding = defaultPadding
        };

        public static readonly GUIStyle itemBackground2 = new GUIStyle(itemBackground1) { name = "quick-search-item-background2" };
        public static readonly GUIStyle selectedItemBackground = new GUIStyle(itemBackground1) { name = "quick-search-item-selected-background" };

        public static readonly GUIStyle selectedGridItemBackground = new GUIStyle(selectedItemBackground)
        {
            name = "quick-search-selected-item-grid-background",
            fixedHeight = 0,
        };

        public static readonly GUIStyle itemGridBackground1 = new GUIStyle(itemBackground1) { fixedHeight = 0, };
        public static readonly GUIStyle itemGridBackground2 = new GUIStyle(itemBackground2) { fixedHeight = 0 };

        public static readonly GUIStyle preview = new GUIStyle
        {
            name = "quick-search-item-preview",
            fixedWidth = 0,
            fixedHeight = 0,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageOnly,
            margin = new RectOffset(2, 2, 2, 2),
            padding = paddingNone
        };

        public static readonly Vector2 previewSize = new Vector2(256, 256);
        public static readonly GUIStyle largePreview = new GUIStyle
        {
            name = "quick-search-item-large-preview",
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageOnly,
            margin = new RectOffset(2, 2, 2, 2),
            padding = paddingNone
        };

        public static readonly GUIStyle itemLabel = new GUIStyle(EditorStyles.label)
        {
            name = "quick-search-item-label",
            richText = true,
            wordWrap = false,
            margin = new RectOffset(4, 4, 6, 2),
            padding = paddingNone
        };

        public static readonly GUIStyle itemLabelCompact = new GUIStyle(itemLabel)
        {
            margin = new RectOffset(4, 4, 2, 2)
        };

        public static readonly GUIStyle itemLabelGrid = new GUIStyle(itemLabel)
        {
            fontSize = itemLabel.fontSize - 1,
            wordWrap = true,
            alignment = TextAnchor.UpperCenter,
            margin = marginNone,
            padding = new RectOffset(1, 1, 1, 1)
        };

        public static readonly GUIStyle selectedItemLabel = new GUIStyle(itemLabel)
        {
            name = "quick-search-item-selected-label",
            margin = new RectOffset(4, 4, 6, 2),
            padding = paddingNone
        };

        public static readonly GUIStyle selectedItemLabelCompact = new GUIStyle(selectedItemLabel)
        {
            margin = new RectOffset(4, 4, 2, 2)
        };

        public static readonly GUIStyle autoCompleteItemLabel = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            name = "quick-search-auto-complete-item-label",
            fixedHeight = EditorStyles.toolbarButton.fixedHeight,
            padding = new RectOffset(4, 4, 0, 1)
        };

        public static readonly GUIStyle autoCompleteSelectedItemLabel = new GUIStyle(autoCompleteItemLabel)
        {
            name = "quick-search-auto-complete-item-selected-label"
        };

        public static readonly GUIStyle autoCompleteTooltip = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(2, 6, 0, 2)
        };

        public static readonly GUIStyle noResult = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            name = "quick-search-no-result",
            fontSize = 20,
            fixedHeight = 0,
            fixedWidth = 0,
            wordWrap = true,
            richText = true,
            alignment = TextAnchor.MiddleCenter,
            margin = marginNone,
            padding = paddingNone
        };

        public static readonly GUIStyle itemDescription = new GUIStyle(EditorStyles.label)
        {
            name = "quick-search-item-description",
            richText = true,
            wordWrap = false,
            margin = new RectOffset(4, 4, 1, 4),
            padding = paddingNone,

            fontSize = Math.Max(9, itemLabel.fontSize - 2),
            fontStyle = FontStyle.Italic
        };

        public static readonly GUIStyle previewDescription = new GUIStyle(itemDescription)
        {
            wordWrap = true,
            padding = new RectOffset(4, 4, 4, 4),
            fontSize = Math.Max(11, itemLabel.fontSize + 2),
            fontStyle = FontStyle.Normal
        };

        public static readonly GUIStyle statusLabel = new GUIStyle(itemDescription)
        {
            name = "quick-search-status-label",
            margin = new RectOffset(4, 4, 2, 2)
        };

        public static readonly GUIStyle itemIconSizeSlider = new GUIStyle("HorizontalSlider")
        {
            margin = new RectOffset(3, 2, -1, 1)
        };

        public static readonly GUIStyle itemIconSizeSliderThumb = new GUIStyle("HorizontalSliderThumb")
        {
            margin = new RectOffset(0, 0, 5, 0),
            fixedHeight = 8f,
            fixedWidth = 8f
        };

        public static readonly GUIStyle versionLabel = new GUIStyle(statusLabel)
        {
            name = "quick-search-version-label",
            normal = new GUIStyleState { textColor = new Color(79 / 255f, 128 / 255f, 248 / 255f) },
        };

        public static readonly GUIStyle selectedItemDescription = new GUIStyle(itemDescription)
        {
            name = "quick-search-item-selected-description"
        };

        public static readonly GUIStyle actionButton = new GUIStyle("IconButton")
        {
            name = "quick-search-action-button",

            fixedWidth = actionButtonSize,
            fixedHeight = actionButtonSize,

            imagePosition = ImagePosition.ImageOnly,

            margin = new RectOffset(4, 4, actionButtonMargin, actionButtonMargin),
            padding = paddingNone,

            active = new GUIStyleState { background = buttonPressedBackgroundImage, scaledBackgrounds = new[] { buttonPressedBackgroundImage } },
            hover = new GUIStyleState { background = buttonHoveredBackgroundImage, scaledBackgrounds = new[] { buttonHoveredBackgroundImage } }
        };

        public static readonly GUIStyle actionButtonHovered = new GUIStyle(actionButton)
        {
            name = "quick-search-action-button-hovered"
        };

        private const float k_ToolbarHeight = 40.0f;

        private static readonly GUIStyleState clear = new GUIStyleState()
        {
            background = null,
            scaledBackgrounds = new Texture2D[] { null },
            textColor = isDarkTheme ? new Color(210 / 255f, 210 / 255f, 210 / 255f) : Color.black
        };

        private static readonly GUIStyleState searchFieldBgNormal = new GUIStyleState() { background = searchFieldBg, scaledBackgrounds = new Texture2D[] { null } };
        private static readonly GUIStyleState searchFieldBgFocus = new GUIStyleState() { background = searchFieldFocusBg, scaledBackgrounds = new Texture2D[] { null } };

        public static readonly GUIStyle toolbar = new GUIStyle("Toolbar")
        {
            name = "quick-search-bar",
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0),
            fixedHeight = k_ToolbarHeight,

            normal = searchFieldBgNormal,
            focused = searchFieldBgFocus,
            hover = searchFieldBgFocus,
            active = searchFieldBgFocus,
            onNormal = clear,
            onHover = searchFieldBgFocus,
            onFocused = searchFieldBgFocus,
            onActive = searchFieldBgFocus,
        };

        public static readonly GUIStyle searchField = new GUIStyle("ToolbarSeachTextFieldPopup")
        {
            name = "quick-search-search-field",
            fontSize = 28,
            alignment = TextAnchor.MiddleLeft,
            margin = new RectOffset(10, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
            border = new RectOffset(0, 0, 0, 0),
            fixedHeight = 0,
            normal = clear,
            focused = clear,
            hover = clear,
            active = clear,
            onNormal = clear,
            onHover = clear,
            onFocused = clear,
            onActive = clear,
        };

        public static readonly GUIStyle placeholderTextStyle = new GUIStyle(searchField)
        {
            fontSize = 20,
            fontStyle = FontStyle.Italic,
            padding = new RectOffset(6, 0, 0, 0)
        };

        public static readonly GUIStyle searchFieldBtn = new GUIStyle()
        {
            name = "quick-search-search-field-clear",
            fixedHeight = 0,
            fixedWidth = 0,
            margin = new RectOffset(0, 5, 8, 0),
            padding = new RectOffset(0, 0, 0, 0),
            normal = clear,
            focused = clear,
            hover = clear,
            active = clear,
            onNormal = clear,
            onHover = clear,
            onFocused = clear,
            onActive = clear,
            alignment = TextAnchor.MiddleCenter
        };

        public static readonly GUIStyle filterButton = new GUIStyle(EditorStyles.whiteLargeLabel)
        {
            name = "quick-search-filter-button",
            margin = new RectOffset(-4, 0, 0, 0),
            padding = new RectOffset(0, 0, 1, 0),
            normal = clear,
            focused = clear,
            hover = clear,
            active = clear,
            onNormal = clear,
            onHover = clear,
            onFocused = clear,
            onActive = clear
        };

        public static readonly GUIContent createSearchQueryContent = new GUIContent(Icons.favorite, "Create New Search Query (Ctrl + S)");

        public static readonly GUIContent prefButtonContent = new GUIContent(Icons.settings, "Open quick search preferences...");
        public static readonly GUIStyle prefButton = new GUIStyle("IconButton")
        {
            fixedWidth = 16,
            fixedHeight = 16,
            margin = new RectOffset(0, 2, 0, 2),
            padding = new RectOffset(0, 0, 0, 0)
        };

        public static readonly GUIStyle searchInProgressButton = new GUIStyle(prefButton)
        {
            imagePosition = ImagePosition.ImageOnly,
            alignment = TextAnchor.MiddleLeft,
            contentOffset = new Vector2(-1, 0),
            padding = new RectOffset(2, 2, 2, 2),
            richText = false,
            stretchHeight = false,
            stretchWidth = false
        };

        public static readonly GUIStyle inspectorFoldout = new GUIStyle("FoldoutHeader")
        {
            clipping = TextClipping.Clip,
            fixedWidth = 235
        };

        public static readonly GUILayoutOption[] searchInProgressLayoutOptions = new[] { GUILayout.MaxWidth(searchInProgressButton.fixedWidth) };
        public static readonly GUIContent emptyContent = new GUIContent("", "No content");

        public static readonly GUIContent[] statusWheel;

        public static readonly GUIStyle debugToolbar = new GUIStyle(EditorStyles.toolbar)
        {
            name = "quick-search-debug-toolbar"
        };

        public static readonly GUIStyle debugToolbarButton = new GUIStyle(EditorStyles.toolbarButton)
        {
            name = "quick-search-debug-toolbarbutton",
            richText = true
        };

        public static readonly GUIStyle debugLabel = new GUIStyle(EditorStyles.label)
        {
            margin = new RectOffset(3, 3, 3, 3),
            richText = true
        };

        private static Texture2D GenerateSolidColorTexture(Color fillColor)
        {
            Texture2D texture = new Texture2D(1, 1);
            var fillColorArray = texture.GetPixels();

            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = fillColor;

            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixels(fillColorArray);
            texture.Apply();

            return texture;
        }

        public static readonly GUIStyle statusBarBackground = new GUIStyle()
        {
            name = "quick-search-status-bar-background"
        };
    }
}
