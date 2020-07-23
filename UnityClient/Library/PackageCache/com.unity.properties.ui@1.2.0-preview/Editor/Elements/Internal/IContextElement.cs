namespace Unity.Properties.UI.Internal
{
    interface IContextElement
    {
        PropertyPath Path { get; }
        
        void SetContext(PropertyElement root, PropertyPath path);
        void OnContextReady();
    }
}