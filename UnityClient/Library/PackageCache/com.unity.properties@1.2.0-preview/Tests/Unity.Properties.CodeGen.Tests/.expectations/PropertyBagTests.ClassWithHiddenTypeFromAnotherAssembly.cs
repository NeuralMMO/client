using System.Reflection;
using Unity.Properties.CodeGen.Tests.Types;

[assembly: AssemblyVersion("0.0.0.0")]
namespace Unity.Properties.Generated
{
	internal class Unity_Properties_CodeGen_Tests_Types_ClassFromAnotherAssembly_PropertyBag : ContainerPropertyBag<ClassFromAnotherAssembly>
	{
		private class Int32Field : ReflectedMemberProperty<ClassFromAnotherAssembly, int>
		{
			public Int32Field()
				: base((IMemberInfo)new FieldMember(typeof(ClassFromAnotherAssembly).GetField("Int32Field", BindingFlags.Instance | BindingFlags.NonPublic)))
			{
			}
		}

		public Unity_Properties_CodeGen_Tests_Types_ClassFromAnotherAssembly_PropertyBag()
		{
			AddProperty(new Int32Field());
		}
	}
}
