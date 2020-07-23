namespace Unity.Entities.Editor
{
    static class Constants
    {
        public const string PackageName = "com.unity.dots.editor";
        public const string PackagePath = "Packages/" + PackageName;

        public const string EditorDefaultResourcesPath = PackagePath + "/Editor Default Resources/";

        public static class Conversion
        {
            public const string SelectedComponentSessionKey = "Conversion.Selected.{0}.{1}";
            public const string ShowAdditionalEntitySessionKey = "Conversion.ShowAdditional.{0}";
            public const string SelectedAdditionalEntitySessionKey = "Conversion.Additional.{0}";
        }

        public static class MenuItems
        {
            public const int WindowPriority = 3006;   
            public const string SystemScheduleWindow = "internal:Window/DOTS/Systems Schedule";
            public const string EntityHierarchyWindow = "internal:Window/DOTS/Entity Hierarchy";
        }

        public static class Settings
        {
            public const string InspectorSettings = "Inspector Settings";
            public const string AdvancedSettings = "Advanced Settings";
        }

        public static class State
        {
            public const string ViewDataKeyPrefix = "dots-editor__";
        }
    }
}
