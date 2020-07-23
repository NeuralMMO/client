using System;
using System.Reflection;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
namespace Unity.Properties.Generated
{
	internal class Unity_Properties_CodeGen_Tests_StructWithPrimitives_PropertyBag : ContainerPropertyBag<StructWithPrimitives>
	{
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

		private class HiddenInt32Field : ReflectedMemberProperty<StructWithPrimitives, int>
		{
			public HiddenInt32Field()
				: base(typeof(StructWithPrimitives).GetField("HiddenInt32Field", BindingFlags.Instance | BindingFlags.NonPublic))
			{
			}
		}

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

		public Unity_Properties_CodeGen_Tests_StructWithPrimitives_PropertyBag()
		{
			AddProperty(new Int32Field());
			AddProperty(new HiddenInt32Field());
			AddProperty(new Int32FieldWithCustomAttribute());
			AddProperty(new Int32Property());
			AddProperty(new Int32PropertyReadOnly());
		}
	}
}
