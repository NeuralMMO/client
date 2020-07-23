using System.Reflection;
using Unity.Properties;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
private class Int32FieldWithCustomAttribute : Property<StructWithPrimitives, int>
{
	public override string Name => "Int32FieldWithCustomAttribute";

	public override bool IsReadOnly => false;

	public Int32FieldWithCustomAttribute()
	{
		AddAttributes(typeof(StructWithPrimitives).GetField("Int32FieldWithCustomAttribute", BindingFlags.Instance | BindingFlags.Public).GetCustomAttributes());
	}

	public override int GetValue(ref StructWithPrimitives container)
	{
		return container.Int32FieldWithCustomAttribute;
	}

	public override void SetValue(ref StructWithPrimitives container, int value)
	{
		container.Int32FieldWithCustomAttribute = value;
	}
}
