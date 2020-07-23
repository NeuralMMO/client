using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Unity.Properties.CodeGen.Tests
{
#pragma warning disable 0649
    public struct StructWithPrimitives
    {
        public int Int32Field;
        [CreateProperty] int HiddenInt32Field;
        [CreateProperty] public int Int32Property { get; set; }
        [CreateProperty] public int Int32PropertyReadOnly { get; }
        [HideInInspector] public int Int32FieldWithCustomAttribute;
        
        [DontCreateProperty]
        public int DontCreateAPropertyForMe;
    }

    public class ClassWithGenericParameter<T>
    {
        public T Value;

        public class Nested
        {
            public T Value;
        }
    }
    public class ClassWithCollections
    {
        public List<int> Int32List;
        public Dictionary<HashSet<List<float>>, List<List<string>>> Complex;
    }
    
    class TestBag : ContainerPropertyBag<StructWithPrimitives>
    {
        class Hidden : ReflectedMemberProperty<StructWithPrimitives, int>
        {
            public Hidden() : base(typeof(StructWithPrimitives).GetProperty("Int32Field", BindingFlags.Instance | BindingFlags.NonPublic))
            {
            }
        }

        public TestBag()
        {
            AddProperty(new Hidden());
        }
    }
    
#pragma warning restore 0649
}