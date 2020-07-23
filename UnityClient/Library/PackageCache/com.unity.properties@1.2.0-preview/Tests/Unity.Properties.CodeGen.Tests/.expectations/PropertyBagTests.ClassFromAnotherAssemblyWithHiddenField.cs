using System.Reflection;
using Unity.Properties.CodeGen.Tests.Types;

[assembly: AssemblyVersion("0.0.0.0")]
namespace Unity.Properties.Generated
{
	internal class Unity_Properties_CodeGen_Tests_Types_ClassFromAnotherAssemblyWithHiddenField_PropertyBag : ContainerPropertyBag<ClassFromAnotherAssemblyWithHiddenField>
	{
		private class Int32Field : ReflectedMemberProperty<ClassFromAnotherAssemblyWithHiddenField, int>
		{
			public Int32Field()
				: base(typeof(ClassFromAnotherAssemblyWithHiddenField).GetField("Int32Field", BindingFlags.Instance | BindingFlags.NonPublic))
			{
			}
		}

		public Unity_Properties_CodeGen_Tests_Types_ClassFromAnotherAssemblyWithHiddenField_PropertyBag()
		{
			AddProperty(new Int32Field());
		}
	}
}
