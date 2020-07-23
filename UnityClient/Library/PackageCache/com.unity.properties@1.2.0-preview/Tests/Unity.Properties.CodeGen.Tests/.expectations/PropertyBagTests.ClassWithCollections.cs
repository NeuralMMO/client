using System.Collections.Generic;
using System.Reflection;
using Unity.Properties.CodeGen.Tests;

[assembly: AssemblyVersion("0.0.0.0")]
namespace Unity.Properties.Generated
{
	internal class Unity_Properties_CodeGen_Tests_ClassWithCollections_PropertyBag : ContainerPropertyBag<ClassWithCollections>
	{
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

		private class Complex : Property<ClassWithCollections, Dictionary<HashSet<List<float>>, List<List<string>>>>
		{
			public override string Name => "Complex";

			public override bool IsReadOnly => false;

			public override Dictionary<HashSet<List<float>>, List<List<string>>> GetValue(ref ClassWithCollections container)
			{
				return container.Complex;
			}

			public override void SetValue(ref ClassWithCollections container, Dictionary<HashSet<List<float>>, List<List<string>>> value)
			{
				container.Complex = value;
			}
		}

		public Unity_Properties_CodeGen_Tests_ClassWithCollections_PropertyBag()
		{
			PropertyBag.RegisterList<ClassWithCollections, List<int>, int>();
			AddProperty(new Int32List());
			PropertyBag.RegisterDictionary<ClassWithCollections, Dictionary<HashSet<List<float>>, List<List<string>>>, HashSet<List<float>>, List<List<string>>>();
			PropertyBag.RegisterSet<Dictionary<HashSet<List<float>>, List<List<string>>>, HashSet<List<float>>, List<float>>();
			PropertyBag.RegisterList<HashSet<List<float>>, List<float>, float>();
			PropertyBag.RegisterList<Dictionary<HashSet<List<float>>, List<List<string>>>, List<List<string>>, List<string>>();
			PropertyBag.RegisterList<List<List<string>>, List<string>, string>();
			AddProperty(new Complex());
		}
	}
}
