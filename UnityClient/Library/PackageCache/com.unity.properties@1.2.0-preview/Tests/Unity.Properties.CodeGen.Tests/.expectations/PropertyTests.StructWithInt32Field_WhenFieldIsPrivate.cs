using System.Reflection;
using Unity.Properties;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
private class HiddenInt32Field : ReflectedMemberProperty<StructWithPrimitives, int>
{
	public HiddenInt32Field()
		: base(typeof(StructWithPrimitives).GetField("HiddenInt32Field", BindingFlags.Instance | BindingFlags.NonPublic))
	{
	}
}
