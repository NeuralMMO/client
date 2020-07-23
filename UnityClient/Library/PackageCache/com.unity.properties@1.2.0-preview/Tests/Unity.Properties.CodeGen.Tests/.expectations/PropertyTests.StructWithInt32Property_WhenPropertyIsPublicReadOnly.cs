using System;
using System.Reflection;
using Unity.Properties;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
private class Int32PropertyReadOnly : Property<StructWithPrimitives, int>
{
	public override string Name => "Int32PropertyReadOnly";

	public override bool IsReadOnly => true;

	public override int GetValue(ref StructWithPrimitives container)
	{
		return container.Int32PropertyReadOnly;
	}

	public override void SetValue(ref StructWithPrimitives container, int value)
	{
		throw new Exception("Property is ReadOnly");
	}
}
