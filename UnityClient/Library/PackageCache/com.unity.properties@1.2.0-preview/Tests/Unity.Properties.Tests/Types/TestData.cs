using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Properties.Tests
{
    [Flags]
    enum EnumInt32Flags : int
    {
        None =   0,
        Value1 = 1,
        Value2 = 2,
        Value3 = 4,
        Value4 = 8
    }

    enum UnorderedEnumInt32 : int
    {
        None = 0,
        Value1 = 1,
        Value4 = 4,
        Value2 = 2,
        Value3 = 3
    }

    enum EnumUInt8 : byte
    {
        None = 0,
        Value1 = 1,
        Value2 = 2,
    }

    struct ClassWithNoFields
    {
        internal class PropertyBag : ContainerPropertyBag<ClassWithNoFields>
        {
        }
    }

    struct StructWithNoFields
    {
        internal class PropertyBag : ContainerPropertyBag<StructWithNoFields>
        {
        }
    }

    class ClassWithPrimitives
    {
        public bool BoolValue;
        public sbyte Int8Value;
        public short Int16Value;
        public int Int32Value;
        public long Int64Value;
        public byte UInt8Value;
        public ushort UInt16Value;
        public uint UInt32Value;
        public ulong UInt64Value;
        public float Float32Value;
        public double Float64Value;
        public char CharValue;
        public string StringValue;
        public EnumInt32Flags EnumInt32Flags;
        public UnorderedEnumInt32 EnumInt32Unordered;
        public EnumUInt8 EnumUInt8;

        internal class PropertyBag : ContainerPropertyBag<ClassWithPrimitives>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithPrimitives, bool>(
                                name: nameof(BoolValue), 
                                getter: (ref ClassWithPrimitives c) => c.BoolValue, 
                                setter: (ref ClassWithPrimitives c, bool v) => c.BoolValue = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, sbyte>(
                                name: nameof(Int8Value), 
                                getter: (ref ClassWithPrimitives c) => c.Int8Value, 
                                setter: (ref ClassWithPrimitives c, sbyte v) => c.Int8Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, short>(
                                name: nameof(Int16Value), 
                                getter: (ref ClassWithPrimitives c) => c.Int16Value, 
                                setter: (ref ClassWithPrimitives c, short v) => c.Int16Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, int>(
                                name: nameof(Int32Value), 
                                getter: (ref ClassWithPrimitives c) => c.Int32Value, 
                                setter: (ref ClassWithPrimitives c, int v) => c.Int32Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, long>(
                                name: nameof(Int64Value), 
                                getter: (ref ClassWithPrimitives c) => c.Int64Value, 
                                setter: (ref ClassWithPrimitives c, long v) => c.Int64Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, byte>(
                                name: nameof(UInt8Value), 
                                getter: (ref ClassWithPrimitives c) => c.UInt8Value, 
                                setter: (ref ClassWithPrimitives c, byte v) => c.UInt8Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, ushort>(
                                name: nameof(UInt16Value), 
                                getter: (ref ClassWithPrimitives c) => c.UInt16Value, 
                                setter: (ref ClassWithPrimitives c, ushort v) => c.UInt16Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, uint>(
                                name: nameof(UInt32Value), 
                                getter: (ref ClassWithPrimitives c) => c.UInt32Value, 
                                setter: (ref ClassWithPrimitives c, uint v) => c.UInt32Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, ulong>(
                                name: nameof(UInt64Value), 
                                getter: (ref ClassWithPrimitives c) => c.UInt64Value, 
                                setter: (ref ClassWithPrimitives c, ulong v) => c.UInt64Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, float>(
                                name: nameof(Float32Value), 
                                getter: (ref ClassWithPrimitives c) => c.Float32Value, 
                                setter: (ref ClassWithPrimitives c, float v) => c.Float32Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, double>(
                                name: nameof(Float64Value), 
                                getter: (ref ClassWithPrimitives c) => c.Float64Value, 
                                setter: (ref ClassWithPrimitives c, double v) => c.Float64Value = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, char>(
                                name: nameof(CharValue), 
                                getter: (ref ClassWithPrimitives c) => c.CharValue, 
                                setter: (ref ClassWithPrimitives c, char v) => c.CharValue = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, string>(
                                name: nameof(StringValue), 
                                getter: (ref ClassWithPrimitives c) => c.StringValue, 
                                setter: (ref ClassWithPrimitives c, string v) => c.StringValue = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, EnumInt32Flags>(
                                name: nameof(EnumInt32Flags), 
                                getter: (ref ClassWithPrimitives c) => c.EnumInt32Flags, 
                                setter: (ref ClassWithPrimitives c, EnumInt32Flags v) => c.EnumInt32Flags = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, UnorderedEnumInt32>(
                                name: nameof(EnumInt32Unordered), 
                                getter: (ref ClassWithPrimitives c) => c.EnumInt32Unordered, 
                                setter: (ref ClassWithPrimitives c, UnorderedEnumInt32 v) => c.EnumInt32Unordered = v));
                
                AddProperty(new DelegateProperty<ClassWithPrimitives, EnumUInt8>(
                                name: nameof(EnumUInt8), 
                                getter: (ref ClassWithPrimitives c) => c.EnumUInt8, 
                                setter: (ref ClassWithPrimitives c, EnumUInt8 v) => c.EnumUInt8 = v));
            }
        }
    }
    
    struct StructWithPrimitives
    {
        public bool BoolValue;
        public sbyte Int8Value;
        public short Int16Value;
        public int Int32Value;
        public long Int64Value;
        public byte UInt8Value;
        public ushort UInt16Value;
        public uint UInt32Value;
        public ulong UInt64Value;
        public float Float32Value;
        public double Float64Value;
        public char CharValue;
        public string StringValue;
        public EnumInt32Flags EnumInt32Flags;
        public UnorderedEnumInt32 EnumInt32Unordered;
        public EnumUInt8 EnumUInt8;

        internal class PropertyBag : ContainerPropertyBag<StructWithPrimitives>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<StructWithPrimitives, bool>(
                                name: nameof(BoolValue), 
                                getter: (ref StructWithPrimitives c) => c.BoolValue, 
                                setter: (ref StructWithPrimitives c, bool v) => c.BoolValue = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, sbyte>(
                                name: nameof(Int8Value), 
                                getter: (ref StructWithPrimitives c) => c.Int8Value, 
                                setter: (ref StructWithPrimitives c, sbyte v) => c.Int8Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, short>(
                                name: nameof(Int16Value), 
                                getter: (ref StructWithPrimitives c) => c.Int16Value, 
                                setter: (ref StructWithPrimitives c, short v) => c.Int16Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, int>(
                                name: nameof(Int32Value), 
                                getter: (ref StructWithPrimitives c) => c.Int32Value, 
                                setter: (ref StructWithPrimitives c, int v) => c.Int32Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, long>(
                                name: nameof(Int64Value), 
                                getter: (ref StructWithPrimitives c) => c.Int64Value, 
                                setter: (ref StructWithPrimitives c, long v) => c.Int64Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, byte>(
                                name: nameof(UInt8Value), 
                                getter: (ref StructWithPrimitives c) => c.UInt8Value, 
                                setter: (ref StructWithPrimitives c, byte v) => c.UInt8Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, ushort>(
                                name: nameof(UInt16Value), 
                                getter: (ref StructWithPrimitives c) => c.UInt16Value, 
                                setter: (ref StructWithPrimitives c, ushort v) => c.UInt16Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, uint>(
                                name: nameof(UInt32Value), 
                                getter: (ref StructWithPrimitives c) => c.UInt32Value, 
                                setter: (ref StructWithPrimitives c, uint v) => c.UInt32Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, ulong>(
                                name: nameof(UInt64Value), 
                                getter: (ref StructWithPrimitives c) => c.UInt64Value, 
                                setter: (ref StructWithPrimitives c, ulong v) => c.UInt64Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, float>(
                                name: nameof(Float32Value), 
                                getter: (ref StructWithPrimitives c) => c.Float32Value, 
                                setter: (ref StructWithPrimitives c, float v) => c.Float32Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, double>(
                                name: nameof(Float64Value), 
                                getter: (ref StructWithPrimitives c) => c.Float64Value, 
                                setter: (ref StructWithPrimitives c, double v) => c.Float64Value = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, char>(
                                name: nameof(CharValue), 
                                getter: (ref StructWithPrimitives c) => c.CharValue, 
                                setter: (ref StructWithPrimitives c, char v) => c.CharValue = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, string>(
                                name: nameof(StringValue), 
                                getter: (ref StructWithPrimitives c) => c.StringValue, 
                                setter: (ref StructWithPrimitives c, string v) => c.StringValue = v));

                AddProperty(new DelegateProperty<StructWithPrimitives, EnumInt32Flags>(
                                name: nameof(EnumInt32Flags), 
                                getter: (ref StructWithPrimitives c) => c.EnumInt32Flags, 
                                setter: (ref StructWithPrimitives c, EnumInt32Flags v) => c.EnumInt32Flags = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, UnorderedEnumInt32>(
                                name: nameof(EnumInt32Unordered), 
                                getter: (ref StructWithPrimitives c) => c.EnumInt32Unordered, 
                                setter: (ref StructWithPrimitives c, UnorderedEnumInt32 v) => c.EnumInt32Unordered = v));
                
                AddProperty(new DelegateProperty<StructWithPrimitives, EnumUInt8>(
                                name: nameof(EnumUInt8), 
                                getter: (ref StructWithPrimitives c) => c.EnumUInt8, 
                                setter: (ref StructWithPrimitives c, EnumUInt8 v) => c.EnumUInt8 = v));
            }
        }
    }

    class ClassWithNestedClass
    {
        public ClassWithPrimitives Container;

        internal class PropertyBag : ContainerPropertyBag<ClassWithNestedClass>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithNestedClass, ClassWithPrimitives>(
                                name: nameof(Container), 
                                getter: (ref ClassWithNestedClass c) => c.Container, 
                                setter: (ref ClassWithNestedClass c, ClassWithPrimitives v) => c.Container = v));
            }
        }
    }

    class ClassWithNestedStruct
    {
        public StructWithPrimitives Container;

        internal class PropertyBag : ContainerPropertyBag<ClassWithNestedStruct>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithNestedStruct, StructWithPrimitives>(
                                name: nameof(Container), 
                                getter: (ref ClassWithNestedStruct c) => c.Container, 
                                setter: (ref ClassWithNestedStruct c, StructWithPrimitives v) => c.Container = v));
            }
        }
    }

    struct StructWithNestedClass
    {
        public ClassWithPrimitives Container;

        internal class PropertyBag : ContainerPropertyBag<StructWithNestedClass>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<StructWithNestedClass, ClassWithPrimitives>(
                                name: nameof(Container), 
                                getter: (ref StructWithNestedClass c) => c.Container, 
                                setter: (ref StructWithNestedClass c, ClassWithPrimitives v) => c.Container = v));
            }
        }
    }
    
    struct StructWithNestedStruct
    {
        public StructWithPrimitives Container;

        internal class PropertyBag : ContainerPropertyBag<StructWithNestedStruct>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<StructWithNestedStruct, StructWithPrimitives>(
                                name: nameof(Container), 
                                getter: (ref StructWithNestedStruct c) => c.Container, 
                                setter: (ref StructWithNestedStruct c, StructWithPrimitives v) => c.Container = v));
            }
        }
    }

    class ClassWithNestedClassRecursive
    {
        public ClassWithNestedClassRecursive Container;
        
        internal class PropertyBag : ContainerPropertyBag<ClassWithNestedClassRecursive>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithNestedClassRecursive, ClassWithNestedClassRecursive>(
                                name: nameof(Container), 
                                getter: (ref ClassWithNestedClassRecursive c) => c.Container, 
                                setter: (ref ClassWithNestedClassRecursive c, ClassWithNestedClassRecursive v) => c.Container = v));
            }
        }
    }

    class ClassWithArrays
    {
        public int[] Int32Array;
        public ClassWithPrimitives[] ClassContainerArray;
        public StructWithPrimitives[] StructContainerArray;

        internal class PropertyBag : ContainerPropertyBag<ClassWithArrays>
        {
            public PropertyBag()
            {
                Properties.PropertyBag.RegisterList<ClassWithArrays, int[], int>();
                Properties.PropertyBag.RegisterList<ClassWithArrays, ClassWithPrimitives[], ClassWithPrimitives>();
                Properties.PropertyBag.RegisterList<ClassWithArrays, StructWithPrimitives[], StructWithPrimitives>();
                
                AddProperty(new DelegateProperty<ClassWithArrays, int[]>(
                                name: nameof(Int32Array), 
                                getter: (ref ClassWithArrays c) => c.Int32Array, 
                                setter: (ref ClassWithArrays c, int[] v) => c.Int32Array = v));
                
                AddProperty(new DelegateProperty<ClassWithArrays, ClassWithPrimitives[]>(
                                name: nameof(ClassContainerArray), 
                                getter: (ref ClassWithArrays c) => c.ClassContainerArray, 
                                setter: (ref ClassWithArrays c, ClassWithPrimitives[] v) => c.ClassContainerArray = v));
                
                AddProperty(new DelegateProperty<ClassWithArrays, StructWithPrimitives[]>(
                                name: nameof(StructContainerArray), 
                                getter: (ref ClassWithArrays c) => c.StructContainerArray, 
                                setter: (ref ClassWithArrays c, StructWithPrimitives[] v) => c.StructContainerArray = v));
            }
        }
    }

    class ClassWithLists
    {
        public List<int> Int32List;
        public List<ClassWithPrimitives> ClassContainerList;
        public List<StructWithPrimitives> StructContainerList;
        public List<List<int>> Int32ListList;
        
        internal class PropertyBag : ContainerPropertyBag<ClassWithLists>
        {
            public PropertyBag()
            {
                Properties.PropertyBag.RegisterList<ClassWithLists, List<int>, int>();
                Properties.PropertyBag.RegisterList<ClassWithLists, List<ClassWithPrimitives>, ClassWithPrimitives>();
                Properties.PropertyBag.RegisterList<ClassWithLists, List<StructWithPrimitives>, StructWithPrimitives>();
                Properties.PropertyBag.RegisterList<ClassWithLists, List<List<int>>, List<int>>();
                Properties.PropertyBag.RegisterList<List<List<int>>, List<int>, int>();
                
                AddProperty(new DelegateProperty<ClassWithLists, List<int>>(
                                name: nameof(Int32List), 
                                getter: (ref ClassWithLists c) => c.Int32List, 
                                setter: (ref ClassWithLists c, List<int> v) => c.Int32List = v));
                
                AddProperty(new DelegateProperty<ClassWithLists, List<ClassWithPrimitives>>(
                                name: nameof(ClassContainerList), 
                                getter: (ref ClassWithLists c) => c.ClassContainerList, 
                                setter: (ref ClassWithLists c, List<ClassWithPrimitives> v) => c.ClassContainerList = v));
                
                AddProperty(new DelegateProperty<ClassWithLists, List<StructWithPrimitives>>(
                                name: nameof(StructContainerList), 
                                getter: (ref ClassWithLists c) => c.StructContainerList, 
                                setter: (ref ClassWithLists c, List<StructWithPrimitives> v) => c.StructContainerList = v));
                
                AddProperty(new DelegateProperty<ClassWithLists, List<List<int>>>(
                                name: nameof(Int32ListList), 
                                getter: (ref ClassWithLists c) => c.Int32ListList, 
                                setter: (ref ClassWithLists c, List<List<int>> v) => c.Int32ListList = v));
            }
        }
    }

    class ClassWithDictionaries
    {
        public Dictionary<string, int> DictionaryStringInt32;

        internal class PropertyBag : ContainerPropertyBag<ClassWithDictionaries>
        {
            public PropertyBag()
            {
                Properties.PropertyBag.RegisterDictionary<ClassWithDictionaries, Dictionary<string, int>, string, int>();

                AddProperty(new DelegateProperty<ClassWithDictionaries, Dictionary<string, int>>(
                                name: nameof(DictionaryStringInt32),
                                getter: (ref ClassWithDictionaries c) => c.DictionaryStringInt32,
                                setter: (ref ClassWithDictionaries c, Dictionary<string, int> v) => c.DictionaryStringInt32 = v));
            }
        }
    }
    
    class ClassWithPropertyPath
    {
        public PropertyPath Path = new PropertyPath("Path.To.Array[2].Element");
    }

    interface IContainerInterface
    {
        
    }

    abstract class ClassAbstract : IContainerInterface
    {
        public int AbstractInt32Value;
    }

    class ClassDerivedA : ClassAbstract
    {
        public int DerivedAInt32Value;
        
        internal class PropertyBag : ContainerPropertyBag<ClassDerivedA>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassDerivedA, int>(
                    name: nameof(AbstractInt32Value), 
                    getter: (ref ClassDerivedA c) => c.AbstractInt32Value, 
                    setter: (ref ClassDerivedA c, int v) => c.AbstractInt32Value = v));
                
                AddProperty(new DelegateProperty<ClassDerivedA, int>(
                    name: nameof(DerivedAInt32Value), 
                    getter: (ref ClassDerivedA c) => c.DerivedAInt32Value, 
                    setter: (ref ClassDerivedA c, int v) => c.DerivedAInt32Value = v));
            }
        }
    }

    class ClassDerivedB : ClassAbstract
    {
        public int DerivedBInt32Value;
        
        internal class PropertyBag : ContainerPropertyBag<ClassDerivedB>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassDerivedB, int>(
                    name: nameof(AbstractInt32Value), 
                    getter: (ref ClassDerivedB c) => c.AbstractInt32Value, 
                    setter: (ref ClassDerivedB c, int v) => c.AbstractInt32Value = v));
                
                AddProperty(new DelegateProperty<ClassDerivedB, int>(
                    name: nameof(DerivedBInt32Value), 
                    getter: (ref ClassDerivedB c) => c.DerivedBInt32Value, 
                    setter: (ref ClassDerivedB c, int v) => c.DerivedBInt32Value = v));
            }
        }
    }

    class ClassDerivedA1 : ClassDerivedA
    {
        public int DerivedA1Int32Value;

        internal new class PropertyBag : ContainerPropertyBag<ClassDerivedA1>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassDerivedA1, int>(
                    name: nameof(AbstractInt32Value), 
                    getter: (ref ClassDerivedA1 c) => c.AbstractInt32Value, 
                    setter: (ref ClassDerivedA1 c, int v) => c.AbstractInt32Value = v));
                
                AddProperty(new DelegateProperty<ClassDerivedA1, int>(
                    name: nameof(DerivedAInt32Value), 
                    getter: (ref ClassDerivedA1 c) => c.DerivedAInt32Value, 
                    setter: (ref ClassDerivedA1 c, int v) => c.DerivedAInt32Value = v));
                
                AddProperty(new DelegateProperty<ClassDerivedA1, int>(
                    name: nameof(DerivedA1Int32Value), 
                    getter: (ref ClassDerivedA1 c) => c.DerivedA1Int32Value, 
                    setter: (ref ClassDerivedA1 c, int v) => c.DerivedA1Int32Value = v));
            }
        }
    }

    class ClassWithPolymorphicFields
    {
        public object ObjectValue;
        public IContainerInterface InterfaceValue;
        public ClassAbstract AbstractValue;

        internal class PropertyBag : ContainerPropertyBag<ClassWithPolymorphicFields>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithPolymorphicFields, object>(
                                name: nameof(ObjectValue), 
                                getter: (ref ClassWithPolymorphicFields c) => c.ObjectValue, 
                                setter: (ref ClassWithPolymorphicFields c, object v) => c.ObjectValue = v));
                
                AddProperty(new DelegateProperty<ClassWithPolymorphicFields, IContainerInterface>(
                                name: nameof(InterfaceValue), 
                                getter: (ref ClassWithPolymorphicFields c) => c.InterfaceValue, 
                                setter: (ref ClassWithPolymorphicFields c, IContainerInterface v) => c.InterfaceValue = v));
                
                AddProperty(new DelegateProperty<ClassWithPolymorphicFields, ClassAbstract>(
                                name: nameof(AbstractValue), 
                                getter: (ref ClassWithPolymorphicFields c) => c.AbstractValue, 
                                setter: (ref ClassWithPolymorphicFields c, ClassAbstract v) => c.AbstractValue = v));
            }
        }
    }

    class ClassWithNullable
    {
        public int? NullableInt32;
        public EnumUInt8? NullableEnumUInt8;
        
        internal class PropertyBag : ContainerPropertyBag<ClassWithNullable>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithNullable, int?>(
                                name: nameof(NullableInt32), 
                                getter: (ref ClassWithNullable c) => c.NullableInt32, 
                                setter: (ref ClassWithNullable c, int? v) => c.NullableInt32 = v));
                
                AddProperty(new DelegateProperty<ClassWithNullable, EnumUInt8?>(
                                name: nameof(NullableEnumUInt8), 
                                getter: (ref ClassWithNullable c) => c.NullableEnumUInt8, 
                                setter: (ref ClassWithNullable c, EnumUInt8? v) => c.NullableEnumUInt8 = v));
            }
        }
    }

    class ScriptableObjectWithPrimitives : ScriptableObject
    {
        public int Int32Value;

        internal class PropertyBag : ContainerPropertyBag<ScriptableObjectWithPrimitives>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ScriptableObjectWithPrimitives, int>(
                                name: nameof(Int32Value),
                                getter: (ref ScriptableObjectWithPrimitives c) => c.Int32Value,
                                setter: (ref ScriptableObjectWithPrimitives c, int v) => c.Int32Value = v));
            }
        }
    }
    
    class ClassWithUnityObjects
    {
        public UnityEngine.Object ObjectValue;
        public UnityEngine.Texture2D Texture2DValue;
        public ScriptableObjectWithPrimitives ScriptableObjectValue;
        
        internal class PropertyBag : ContainerPropertyBag<ClassWithUnityObjects>
        {
            public PropertyBag()
            {
                AddProperty(new DelegateProperty<ClassWithUnityObjects, UnityEngine.Object>(
                                name: nameof(ObjectValue),
                                getter: (ref ClassWithUnityObjects c) => c.ObjectValue,
                                setter: (ref ClassWithUnityObjects c, UnityEngine.Object v) => c.ObjectValue = v));
                AddProperty(new DelegateProperty<ClassWithUnityObjects, UnityEngine.Texture2D>(
                                name: nameof(Texture2DValue),
                                getter: (ref ClassWithUnityObjects c) => c.Texture2DValue,
                                setter: (ref ClassWithUnityObjects c, UnityEngine.Texture2D v) => c.Texture2DValue = v));
                AddProperty(new DelegateProperty<ClassWithUnityObjects, ScriptableObjectWithPrimitives>(
                                name: nameof(ScriptableObjectValue),
                                getter: (ref ClassWithUnityObjects c) => c.ScriptableObjectValue,
                                setter: (ref ClassWithUnityObjects c, ScriptableObjectWithPrimitives v) => c.ScriptableObjectValue = v));
            }
        }
    }
}