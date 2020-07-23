namespace Unity.Properties.Tests
{
    class TestVisitor : PropertyVisitor
    {
    }
    
    public static class TestVisitorExtensions
    {
        public static PropertyVisitor WithAdapter<TAdapter>(this PropertyVisitor visitor)
            where TAdapter : IPropertyVisitorAdapter, new()
        {
            visitor.AddAdapter(new TAdapter());
            return visitor;
        }
    }
}