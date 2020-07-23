using System.Reflection;
using Unity.Properties.CodeGen.Tests.Types;

[assembly: AssemblyVersion("0.0.0.0")]
namespace Unity.Properties.Generated
{
	internal class Unity_Properties_CodeGen_Tests_Types_StructWithPrimitiveFromAnotherAssembly_PropertyBag : ContainerPropertyBag<StructWithPrimitiveFromAnotherAssembly>
	{
		private class Int32Field : Property<StructWithPrimitiveFromAnotherAssembly, int>
		{
			public override string Name => "Int32Field";

			public override bool IsReadOnly => false;

			public override int GetValue(ref StructWithPrimitiveFromAnotherAssembly container)
			{
				return container.Int32Field;
			}

			public override void SetValue(ref StructWithPrimitiveFromAnotherAssembly container, int value)
			{
				container.Int32Field = value;
			}
		}

		public Unity_Properties_CodeGen_Tests_Types_StructWithPrimitiveFromAnotherAssembly_PropertyBag()
		{
			AddProperty(new Int32Field());
		}
	}
}
