namespace Unity.Properties.CodeGen.Tests.Types
{
    public struct StructWithPrimitiveFromAnotherAssembly
    {
        public int Int32Field;
    }
    
    public class ClassFromAnotherAssemblyWithHiddenField
    {
        [CreateProperty] int Int32Field;
    }
}