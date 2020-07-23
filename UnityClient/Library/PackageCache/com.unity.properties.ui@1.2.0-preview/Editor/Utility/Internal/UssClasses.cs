namespace Unity.Properties.UI.Internal
{
    static class UssClasses
    {
        const string Base = "unity-properties";

        public const string Variables = Base + "__variables";
        public const string Highlight = Base + "__highlight";

        public static class ListElement
        {
            public const string List = Base + "__list-element";
            public const string Size = List + "__size";

            public const string AddItemButton = List + "__add-item-button";
            public const string RemoveItemButton = List + "__remove-item-button";
            public const string ToggleInput = List + "__toggle-input";

            public const string ItemContainer = List + "__item-container";
            public const string ItemContent = List + "__item-content";
            public const string ItemFoldout = List + "__item-foldout";
            public const string ItemNoFoldout = List + "__item-no-foldout";
            public const string Item = List + "__item";

            public static string MakeListItem(int index) => ItemContainer + "-" + index;
        }

        public static class SetElement
        {
            public const string Set = Base + "__set-element";

            public const string RemoveItemButton = Set + "__remove-item-button";
            public const string ToggleInput = Set + "__toggle-input";

            public const string ItemContainer = Set + "__item-container";
            public const string ItemContent = Set + "__item-content";
            public const string ItemFoldout = Set + "__item-foldout";
            public const string ItemNoFoldout = Set + "__item-no-foldout";
            public const string Item = Set + "__item";
        }

        public static class PaginationElement
        {
            public const string Pagination = Base + "__pagination-element";
            public const string PaginationSize = Pagination + "__pagination-size";
            public const string PreviousPageButton = Pagination + "__previous-page-button";
            public const string NextPageButton = Pagination + "__next-page-button";
            public const string ElementsRange = Pagination + "__elements-range";
            public const string RangeInputRoot = Pagination + "__elements-range-input__root";
            public const string RangeInput = Pagination + "__elements-range-input";
        }

        public static class DictionaryElement
        {
            public const string Dictionary = Base + "__dictionary-element";
        }

        public static class NullableFoldoutElement
        {
            public const string NullableFoldout = Base + "__nullable-foldout";
        }

        public static class CircularReferenceElement
        {
            public const string CircularReference = Base + "__circular-reference";
            public const string Label = CircularReference + "__label";
            public const string Path = CircularReference + "__path";
            public const string Icon = CircularReference + "__icon";
        }
        
        public static class KeyValuePairElement
        {
            public const string KeyValuePair = Base + "__key-value-pair";
            public const string Key = KeyValuePair + "__key";
            public const string Value = KeyValuePair + "__value";
            public const string RemoveButton = KeyValuePair + "__remove-key-button";
        }

        public static class AddKeyDictionaryElement
        {
            const string AddKeyDictionary = Base + "__add-dictionary-key";
            public const string ShowContainerButton = AddKeyDictionary + "__show-container-button";
            public const string Container = AddKeyDictionary + "__container";
            public const string Key = AddKeyDictionary + "__key-element";
            public const string Cancel = AddKeyDictionary + "__cancel-button";
            public const string Add = AddKeyDictionary + "__add-button";
            public const string Error = AddKeyDictionary + "__error-info";
        }

        public static class Unity
        {
            public const string ToggleInput = "unity-toggle__input";
            public const string BaseFieldInput = "unity-base-field__input";
            public const string BasePopupFieldInput = "unity-base-popup-field__input";
            public const string BaseTextFieldInput = "unity-base-text-field__input";
        }
    }
}