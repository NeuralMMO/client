using System.Reflection;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
namespace Unity.Properties.Generated
{
	internal class Unity_Properties_CodeGen_Tests_ClassWithGenericParameter_1_System_Int32__PropertyBag : ContainerPropertyBag<ClassWithGenericParameter<int>>
	{
		private class Value : Property<ClassWithGenericParameter<int>, int>
		{
			public override string Name => "Value";

			public override bool IsReadOnly => false;

			public override int GetValue(ref ClassWithGenericParameter<int> container)
			{
				return container.Value;
			}

			public override void SetValue(ref ClassWithGenericParameter<int> container, int value)
			{
				container.Value = value;
			}
		}

		public Unity_Properties_CodeGen_Tests_ClassWithGenericParameter_1_System_Int32__PropertyBag()
		{
			AddProperty(new Value());
		}
	}
}
