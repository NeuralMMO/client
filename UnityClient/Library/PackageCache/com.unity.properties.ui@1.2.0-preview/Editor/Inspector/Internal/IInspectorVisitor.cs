namespace Unity.Properties.UI.Internal
{
    interface IInspectorVisitor
    {
        InspectorVisitorContext VisitorContext { get; }
        void ClearPath();
        void RestorePath(PropertyPath path);
        
        void AddToPath(PropertyPath path);
        void RemoveFromPath(PropertyPath path);
        void AddToPath(IProperty property);
        void RemoveFromPath(IProperty property);
        PropertyPath GetCurrentPath();
    }
}