using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Rendering
{
    internal enum OperationType : int
    {
        Upload = 0,
        Matrix = 1,
        Matrix_Inverse = 2,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Operation
    {
        public uint type;
        public uint srcOffset;
        public uint dstOffset;
        public uint dstOffsetExtra;
        public uint size;
        public uint count;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ThreadedSparseUploaderData
    {
        // TODO: safety handle?
        // TODO: we want these to be NativeArrays, but then we can't pass it as a pointer in ThreadedSparseUploader
        // TODO: block allocate instead of doing atomics on the integers here?
        [NativeDisableUnsafePtrRestriction] public byte* m_DataPtr;
        [NativeDisableUnsafePtrRestriction] public Operation* m_OperationsPtr;
        public int m_CurrDataOffset;
        public int m_CurrOperation;
        public int m_MaxDataOffset;
        public int m_MaxOperations;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ThreadedSparseUploader
    {
        // TODO: safety handle?
        [NativeDisableUnsafePtrRestriction] internal ThreadedSparseUploaderData* m_Data;

        public void AddUpload(void* src, int size, int offsetInBytes, int repeatCount = 1)
        {
            var dataOffset = Interlocked.Add(ref m_Data->m_CurrDataOffset, size);

            if (dataOffset > m_Data->m_MaxDataOffset)
                return; // TODO: message?
            dataOffset -= size; // since Interlocked.Add returns value after addition

            var operationIndex = Interlocked.Increment(ref m_Data->m_CurrOperation);

            if (operationIndex > m_Data->m_MaxOperations)
                return; // TODO: message?
            operationIndex -= 1; // since Interlocked.Increment returns value after incrementing

            if (repeatCount <= 0)
                repeatCount = 1;

            var dst = m_Data->m_DataPtr;
            // TODO: Vectorized memcpy
            UnsafeUtility.MemCpy(dst + dataOffset, src, size);
            m_Data->m_OperationsPtr[operationIndex] = new Operation
            {
                type = (uint)OperationType.Upload,
                srcOffset = (uint)dataOffset,
                dstOffset = (uint)offsetInBytes,
                dstOffsetExtra = 0,
                size = (uint)size,
                count = (uint)repeatCount
            };
        }

        public void AddUpload<T>(T val, int offsetInBytes, int repeatCount = 1) where T : unmanaged
        {
            var size = UnsafeUtility.SizeOf<T>();
            AddUpload(&val, size, offsetInBytes, repeatCount);
        }

        public void AddUpload<T>(NativeArray<T> array, int offsetInBytes, int repeatCount = 1) where T : struct
        {
            var size = UnsafeUtility.SizeOf<T>() * array.Length;
            AddUpload(array.GetUnsafeReadOnlyPtr(), size, offsetInBytes, repeatCount);
        }

        // Expects an array of float4x4
        public void AddMatrixUpload(void* src, int numMatrices, int offset, int offsetInverse)
        {
            var size = numMatrices * sizeof(float4x3);
            var dataOffset = Interlocked.Add(ref m_Data->m_CurrDataOffset, size);

            if (dataOffset > m_Data->m_MaxDataOffset)
                return; // TODO: message?
            dataOffset -= size; // since Interlocked.Add returns value after addition

            var operationIndex = Interlocked.Increment(ref m_Data->m_CurrOperation);

            if (operationIndex > m_Data->m_MaxOperations)
                return; // TODO: message?
            operationIndex -= 1; // since Interlocked.Increment returns value after incrementing

            var dst = m_Data->m_DataPtr + dataOffset;
            var ptr = (byte*)src;
            for (int i = 0; i < numMatrices; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    UnsafeUtility.MemCpy(dst, ptr, 12);
                    dst += 12;
                    ptr += 16;
                }
            }

            m_Data->m_OperationsPtr[operationIndex] = new Operation
            {
                type = (offsetInverse == -1) ? (uint)OperationType.Matrix : (uint)OperationType.Matrix_Inverse,
                srcOffset = (uint)dataOffset,
                dstOffset = (uint)offset,
                dstOffsetExtra = (uint)offsetInverse,
                size = (uint)size,
                count = 1,
            };
        }
    }

    public unsafe struct SparseUploader : IDisposable
    {
        const int k_NumBufferedFrames = 3;
        int m_CurrFrame;

        ComputeBuffer m_DestinationBuffer;

        ComputeBuffer[] m_DataBuffer;
        NativeArray<byte> m_DataArray;

        ComputeBuffer[] m_OperationsBuffer;
        NativeArray<Operation> m_OperationsArray;

        ThreadedSparseUploaderData* m_ThreadData;

        ComputeShader m_SparseUploaderShader;
        int m_KernelIndex;

        public SparseUploader(ComputeBuffer dst)
        {
            m_CurrFrame = 0;

            m_DestinationBuffer = dst;

            m_DataBuffer = new ComputeBuffer[k_NumBufferedFrames];
            m_OperationsBuffer = new ComputeBuffer[k_NumBufferedFrames];

            m_DataArray = new NativeArray<byte>();
            m_OperationsArray = new NativeArray<Operation>();

            m_ThreadData = (ThreadedSparseUploaderData*)UnsafeUtility.Malloc(sizeof(ThreadedSparseUploaderData),
                UnsafeUtility.AlignOf<ThreadedSparseUploaderData>(), Allocator.Persistent);
            m_ThreadData->m_DataPtr = null;
            m_ThreadData->m_OperationsPtr = null;
            m_ThreadData->m_CurrDataOffset = 0;
            m_ThreadData->m_CurrOperation = 0;
            m_ThreadData->m_MaxDataOffset = 0;
            m_ThreadData->m_MaxOperations = 0;

            m_SparseUploaderShader = Resources.Load<ComputeShader>("SparseUploader");
            m_KernelIndex = m_SparseUploaderShader.FindKernel("CopyKernel");
        }

        public void Dispose()
        {
            for (int i = 0; i < k_NumBufferedFrames; ++i)
            {
                if (m_DataBuffer[i] != null)
                    m_DataBuffer[i].Dispose();

                if (m_OperationsBuffer[i] != null)
                    m_OperationsBuffer[i].Dispose();
            }

            UnsafeUtility.Free(m_ThreadData, Allocator.Persistent);
        }

        public unsafe ThreadedSparseUploader Begin(int maxDataSizeInBytes, int maxOperationCount)
        {
            if (m_DataBuffer[m_CurrFrame] == null || m_DataBuffer[m_CurrFrame].count < maxDataSizeInBytes / 4)
            {
                if (m_DataBuffer[m_CurrFrame] != null)
                    m_DataBuffer[m_CurrFrame].Dispose();

                m_DataBuffer[m_CurrFrame] = new ComputeBuffer((maxDataSizeInBytes + 3) / 4,
                    4,
                    ComputeBufferType.Raw,
                    ComputeBufferMode.SubUpdates);
            }

            if (m_OperationsBuffer[m_CurrFrame] == null ||
                m_OperationsBuffer[m_CurrFrame].count < maxOperationCount)
            {
                if (m_OperationsBuffer[m_CurrFrame] != null)
                    m_OperationsBuffer[m_CurrFrame].Dispose();

                m_OperationsBuffer[m_CurrFrame] = new ComputeBuffer(maxOperationCount,
                    UnsafeUtility.SizeOf<Operation>(),
                    ComputeBufferType.Default,
                    ComputeBufferMode.SubUpdates);
            }

#if UNITY_2020_1_OR_NEWER
            m_DataArray = m_DataBuffer[m_CurrFrame].BeginWrite<byte>(0, maxDataSizeInBytes);
            m_OperationsArray =
                m_OperationsBuffer[m_CurrFrame].BeginWrite<Operation>(0, maxOperationCount);
#endif

            m_ThreadData->m_DataPtr = (byte*)m_DataArray.GetUnsafePtr();
            m_ThreadData->m_OperationsPtr = (Operation*)m_OperationsArray.GetUnsafePtr();
            m_ThreadData->m_CurrDataOffset = 0;
            m_ThreadData->m_CurrOperation = 0;
            m_ThreadData->m_MaxDataOffset = maxDataSizeInBytes;
            m_ThreadData->m_MaxOperations = maxOperationCount;

            // TODO: set safety handle on thread data
            return new ThreadedSparseUploader
            {
                m_Data = m_ThreadData
            };
        }

        public void EndAndCommit(ThreadedSparseUploader tsu)
        {
            // TODO: release safety handle of thread data
            m_ThreadData->m_DataPtr = null;
            m_ThreadData->m_OperationsPtr = null;
            int writtenData = math.min(m_ThreadData->m_CurrDataOffset, m_ThreadData->m_MaxDataOffset);
            int writtenOps  = math.min(m_ThreadData->m_CurrOperation, m_ThreadData->m_MaxOperations);

#if UNITY_2020_1_OR_NEWER
            m_DataBuffer[m_CurrFrame].EndWrite<byte>(writtenData);
            m_OperationsBuffer[m_CurrFrame].EndWrite<Operation>(writtenOps);
#endif

            // Uncomment this to display how much data the uploader will copy.
            //Debug.Log($"SPARSE UPLOADER: uploaded {m_ThreadData->m_CurrDataOffset / (1024.0 * 1024.0)} MiB in {m_ThreadData->m_CurrOperation} operations");

            if (m_ThreadData->m_CurrOperation > 0)
            {
                m_SparseUploaderShader.SetBuffer(m_KernelIndex, "operations", m_OperationsBuffer[m_CurrFrame]);
                m_SparseUploaderShader.SetBuffer(m_KernelIndex, "src", m_DataBuffer[m_CurrFrame]);
                m_SparseUploaderShader.SetBuffer(m_KernelIndex, "dst", m_DestinationBuffer);
                m_SparseUploaderShader.Dispatch(m_KernelIndex, m_ThreadData->m_CurrOperation, 1, 1);
            }

            m_CurrFrame += 1;
            if (m_CurrFrame >= k_NumBufferedFrames)
                m_CurrFrame = 0;
            m_ThreadData->m_CurrDataOffset = 0;
            m_ThreadData->m_CurrOperation = 0;
            m_ThreadData->m_MaxDataOffset = 0;
            m_ThreadData->m_MaxOperations = 0;
        }
    }
}
