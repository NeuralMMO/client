using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using Unity.Properties;
using UnityEditor;
using UnityEngine;


public class SharedComponentSerializeTests
{
    enum MyEnum
    {
        Zero = 0,
        Blah = 5
    }

    struct TestStruct : ISharedComponentData, IEquatable<TestStruct>
    {
        public int Value;
        public float3 Float3;
        public UnityEngine.Material[] MaterialArray;
        public List<UnityEngine.Material> MaterialList;
        public string StringValue;
        public MyEnum EnumValue;
        public UnityEngine.Material Mat;
        public UnityEngine.Object NullObj;

        public static void AreEqual(TestStruct expected, TestStruct value)
        {
            Assert.AreEqual(expected.Value, value.Value);
            Assert.AreEqual(expected.Float3, value.Float3);
            Assert.AreEqual(expected.StringValue, value.StringValue);
            Assert.AreEqual(expected.EnumValue, value.EnumValue);
            Assert.AreEqual(expected.Mat, value.Mat);
            Assert.AreEqual(expected.NullObj, value.NullObj);
            Assert.IsTrue(expected.MaterialArray.SequenceEqual(value.MaterialArray));
            Assert.IsTrue(expected.MaterialList.SequenceEqual(value.MaterialList));
        }

        public bool Equals(TestStruct other)
        {
            return Value == other.Value && Float3.Equals(other.Float3) && Equals(MaterialArray, other.MaterialArray)
                && Equals(MaterialList, other.MaterialList) && StringValue == other.StringValue
                && EnumValue == other.EnumValue && Equals(Mat, other.Mat) && Equals(NullObj, other.NullObj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Value;
                hashCode = (hashCode * 397) ^ Float3.GetHashCode();
                hashCode = (hashCode * 397) ^ (!ReferenceEquals(MaterialArray,  null) ? MaterialArray.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (!ReferenceEquals(MaterialList, null) ? MaterialList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StringValue != null ? StringValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)EnumValue;
                hashCode = (hashCode * 397) ^ (!ReferenceEquals(Mat, null) ? Mat.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (!ReferenceEquals(NullObj, null) ? NullObj.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    TestStruct ConfigureStruct()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.entities/Unity.Scenes.Hybrid.Tests/Test.mat");
        var srcData = new TestStruct();
        srcData.Value = 5;
        srcData.EnumValue = MyEnum.Blah;
        srcData.Float3 = new float3(1, 2, 3);
        srcData.StringValue = "boing string 漢漢";
        srcData.MaterialArray = new Material[] { material, null, material };
        srcData.MaterialList = new List<Material> { null, material, null, material };
        srcData.Mat = material;
        srcData.NullObj = null;
        return srcData;
    }

    [Test]
    unsafe public void ReadWriteBoxed()
    {
        var srcData = ConfigureStruct();

        // Write to stream
        var buffer = new UnsafeAppendBuffer(0, 16, Allocator.Persistent);
        var writer = new ManagedObjectBinaryWriter(&buffer);

        var boxedSrcData = (object)srcData;
        writer.WriteObject(boxedSrcData);

        var objectTable = writer.GetObjectTable();

        // Read from stream
        var readStream = buffer.AsReader();
        var reader = new ManagedObjectBinaryReader(&readStream, objectTable);

        var boxedRead = reader.ReadObject(typeof(TestStruct));

        // Check same
        TestStruct.AreEqual(srcData, (TestStruct)boxedRead);

        buffer.Dispose();
    }

#if !UNITY_DISABLE_MANAGED_COMPONENTS
    public class ComponentWithStringArray : IComponentData
    {
        public string[] StringArray;

        public static void AreEqual(ComponentWithStringArray expected, ComponentWithStringArray value)
        {
            Assert.AreEqual(expected.StringArray.Length, value.StringArray.Length);
            for (int i = 0; i < expected.StringArray.Length; ++i)
                Assert.AreEqual(expected.StringArray[i], value.StringArray[i]);
        }
    }

    /// <summary>
    /// Regression test for an issue where arrays of strings were not constructed properly when
    /// deserializing. Arrays have a special deserialization path, and strings also have a special code
    /// path since the type is immutable. This test exercises both special paths.
    /// </summary>
    [Test]
    unsafe public void ReadWriteBoxedWithStringArrayWithOneElement()
    {
        var srcData = new ComponentWithStringArray()
        {
            StringArray = new string[] { "One" }
        };

        // Write to stream
        var buffer = new UnsafeAppendBuffer(0, 16, Allocator.Persistent);
        var writer = new ManagedObjectBinaryWriter(&buffer);

        var boxedSrcData = (object)srcData;
        writer.WriteObject(boxedSrcData);

        var objectTable = writer.GetObjectTable();

        // Read from stream
        var readStream = buffer.AsReader();
        var reader = new ManagedObjectBinaryReader(&readStream, objectTable);

        var boxedRead = reader.ReadObject(typeof(ComponentWithStringArray));

        // Check same
        ComponentWithStringArray.AreEqual(srcData, (ComponentWithStringArray)boxedRead);

        buffer.Dispose();
    }

#endif
}
