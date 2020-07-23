using System.Collections.Generic;
using System.Reflection;
using Unity.Properties;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
private class Int32List : Property<ClassWithCollections, List<int>>
{
	public override string Name => "Int32List";

	public override bool IsReadOnly => false;

	public override List<int> GetValue(ref ClassWithCollections container)
	{
		return container.Int32List;
	}

	public override void SetValue(ref ClassWithCollections container, List<int> value)
	{
		container.Int32List = value;
	}
}
