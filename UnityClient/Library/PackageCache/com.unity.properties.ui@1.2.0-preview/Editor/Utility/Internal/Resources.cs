using UnityEditor;

namespace Unity.Properties.UI.Internal
{
    static class Resources
    {
        public const string BasePath = "Packages/com.unity.properties.ui/";
        public const string ResourcesPath = BasePath + "Editor Default Resources/";
        public const string Uxml = ResourcesPath + "uxml/";
        public const string Uss = ResourcesPath + "uss/";
        public const string Icons = ResourcesPath + "icons/";

        const string k_ProSuffix = "_dark";
        const string k_PersonalSuffix = "_light";

        public static string SkinSuffix => EditorGUIUtility.isProSkin ? k_ProSuffix : k_PersonalSuffix;

        public static string UxmlFromName(string name)
        {
            return Uxml + name + ".uxml";
        }

        public static string UssFromName(string name)
        {
            return Uss + name + ".uss";
        }

        public static class Templates
        {
            public static readonly UITemplate Common = new UITemplate("common");
            public static readonly UITemplate NullElement = new UITemplate("null-element");
            public static readonly UITemplate NullableFoldout = new UITemplate("nullable-foldout");
            public static readonly UITemplate NullStringField = new UITemplate("null-string-field");
            public static readonly UITemplate KeyValuePairElement = new UITemplate("key-value-pair");
            public static readonly UITemplate ListElement = new UITemplate("list-element");
            public static readonly UITemplate ListElementDefaultStyling = new UITemplate("list-element-default");
            public static readonly UITemplate PaginationElement = new UITemplate("pagination-element");
            public static readonly UITemplate DictionaryElement = new UITemplate("dictionary-element");
            public static readonly UITemplate AddCollectionItem = new UITemplate("add-collection-item");
            public static readonly UITemplate SetElement = new UITemplate("set-element");
            public static readonly UITemplate SetElementDefaultStyling = new UITemplate("set-element-default");
            public static readonly UITemplate CircularReference = new UITemplate("circular-reference");
            public static readonly UITemplate LazyLoadReference = new UITemplate("lazy-load-reference");
        }
    }
}