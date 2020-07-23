using System.Reflection;
using Unity.Properties;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
private class Int32Property : Property<StructWithPrimitives, int>
{
	public override string Name => "Int32Property";

	public override bool IsReadOnly => false;

	public override int GetValue(ref StructWithPrimitives container)
	{
		return container.Int32Property;
	}

	public override void SetValue(ref StructWithPrimitives container, int value)
	{
		container.Int32Property = value;
	}
}
