using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Networking.PlayerConnection;

namespace Unity.Scenes
{
    internal static class MessageEventArgsExtensions
    {
        internal static unsafe byte[] SerializeUnmanagedArray<T>(T[] value) where T : unmanaged
        {
            var bytes = new byte[UnsafeUtility.SizeOf<T>() * value.Length + sizeof(int)];
            fixed(byte* ptr = bytes)
            {
                var buf = new UnsafeAppendBuffer(ptr, bytes.Length);
                fixed(T* dataPtr = value)
                buf.AddArray<T>(dataPtr, value.Length);
                Assert.AreEqual(buf.Length, bytes.Length);
            }

            return bytes;
        }

        public static unsafe byte[] SerializeUnmanagedArray<T>(NativeArray<T> value) where T : unmanaged
        {
            var bytes = new byte[UnsafeUtility.SizeOf<T>() * value.Length + sizeof(int)];
            fixed(byte* ptr = bytes)
            {
                var buf = new UnsafeAppendBuffer(ptr, bytes.Length);
                buf.Add(value);
                Assert.AreEqual(buf.Length, bytes.Length);
            }

            return bytes;
        }

        unsafe static NativeArray<T> DeserializeUnmanagedArray<T>(byte[] buffer, Allocator allocator = Allocator.Temp) where T : unmanaged
        {
            fixed(byte* ptr = buffer)
            {
                var buf = new UnsafeAppendBuffer.Reader(ptr, buffer.Length);
                buf.ReadNext<T>(out var array, allocator);
                return array;
            }
        }

        public unsafe static byte[] SerializeUnmanaged<T>(ref T value) where T : unmanaged
        {
            var bytes = new byte[UnsafeUtility.SizeOf<T>()];
            fixed(byte* ptr = bytes)
            {
                UnsafeUtility.CopyStructureToPtr(ref value, ptr);
            }

            return bytes;
        }

        unsafe static T DeserializeUnmanaged<T>(byte[] buffer) where T : unmanaged
        {
            fixed(byte* ptr = buffer)
            {
                UnsafeUtility.CopyPtrToStructure<T>(ptr, out var value);
                return value;
            }
        }

        static public T Receive<T>(this MessageEventArgs args) where T : unmanaged
        {
            return DeserializeUnmanaged<T>(args.data);
        }

        static public NativeArray<T> ReceiveArray<T>(this MessageEventArgs args, Allocator allocator = Allocator.Temp) where T : unmanaged
        {
            return DeserializeUnmanagedArray<T>(args.data, allocator);
        }

        static public void Send<T>(this IEditorPlayerConnection connection, Guid msgGuid, T data) where T : unmanaged
        {
            connection.Send(msgGuid, SerializeUnmanaged(ref data));
        }

        static public void SendArray<T>(this IEditorPlayerConnection connection, Guid msgGuid, NativeArray<T> data) where T : unmanaged
        {
            connection.Send(msgGuid, SerializeUnmanagedArray(data));
        }
    }
}
