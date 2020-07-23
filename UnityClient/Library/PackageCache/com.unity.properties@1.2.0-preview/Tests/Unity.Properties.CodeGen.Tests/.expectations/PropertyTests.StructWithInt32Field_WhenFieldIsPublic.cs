using System.Reflection;
using Unity.Properties;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
private class Int32Field : Property<StructWithPrimitives, int>
{
	public override string Name => "Int32Field";

	public override bool IsReadOnly => false;

	public override int GetValue(ref StructWithPrimitives container)
	{
		return container.Int32Field;
	}

	public override void SetValue(ref StructWithPrimitives container, int value)
	{
		container.Int32Field = value;
	}
}
