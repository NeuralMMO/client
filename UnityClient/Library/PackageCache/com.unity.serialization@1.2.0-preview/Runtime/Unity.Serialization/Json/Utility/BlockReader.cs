using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Unity.Serialization.Json
{
    interface IUnsafeStreamBlockReader : IDisposable
    {
        /// <summary>
        /// Resets the reader for re-use.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Returns the next block in the stream.
        /// </summary>
        UnsafeBuffer<char> GetNextBlock();
    }
    
    /// <summary>
    /// Helper struct to store a managed array of chars.
    /// </summary>
    unsafe struct ManagedCharBuffer
    {
        readonly char[] m_ManagedBuffer;
        readonly GCHandle m_GCHandle;
        readonly char* m_PinnedPtr;
        
        public char* GetUnsafePtr() => m_PinnedPtr;
        public char[] GetManagedArray() => m_ManagedBuffer;
        
        public ManagedCharBuffer(int length)
        {
            m_ManagedBuffer = new char[length];
            m_GCHandle = GCHandle.Alloc(m_ManagedBuffer, GCHandleType.Pinned);
            m_PinnedPtr = (char*) m_GCHandle.AddrOfPinnedObject().ToPointer();
        }

        public void Dispose()
        {
            m_GCHandle.Free();
        }
    }

    abstract class BlockStreamReader : IUnsafeStreamBlockReader
    {
        protected readonly StreamReader Reader;

        protected BlockStreamReader(Stream input, int bufferSize, bool leaveInputOpen)
        {
            Reader = new StreamReader(input, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize, leaveInputOpen);
        }

        public virtual void Dispose()
        {
            Reader.Dispose();
        }

        public virtual void Reset()
        {
            Reader.DiscardBufferedData();
        }

        public abstract UnsafeBuffer<char> GetNextBlock();
    }

    /// <summary>
    /// An asynchronous block reader that tries to read one block ahead in the stream.
    ///
    /// * The first call to `GetNextBlock` reads synchronously and queues the second block.
    /// * Subsequent calls wait on the running task and start the next task before returning.
    ///
    /// </summary>
    unsafe class AsyncBlockReader : BlockStreamReader
    {
        readonly List<ManagedCharBuffer> m_Buffers;
        int m_NextBufferIndex;
        Task<int> m_ReadBlockTask;

        public AsyncBlockReader(Stream input, int bufferSize, bool leaveInputOpen) 
            : base (input, bufferSize, leaveInputOpen)
        {
            var charBufferSize = bufferSize / sizeof(char);
            
            m_Buffers = new List<ManagedCharBuffer>
            {
                new ManagedCharBuffer(charBufferSize), 
                new ManagedCharBuffer(charBufferSize)
            };
            m_NextBufferIndex = 0;
            m_ReadBlockTask = default;
        }
        
        public override UnsafeBuffer<char> GetNextBlock()
        {
            var buffer = m_Buffers[m_NextBufferIndex++];
            var chars = buffer.GetManagedArray();

            if (m_NextBufferIndex >= m_Buffers.Count)
            {
                m_NextBufferIndex = 0;
            }

            int count;

            if (null == m_ReadBlockTask)
            {
                // If we have no task running read the block synchronously
                count = Reader.ReadBlock(chars, index: 0, count: chars.Length);
            }
            else
            {
                // Wait on the current queued task
                if (!m_ReadBlockTask.IsCompleted)
                {
                    m_ReadBlockTask.Wait();
                }

                count = m_ReadBlockTask.Result;
            }

            if (count == chars.Length)
            {
                // Queue up the next block if we still have some bytes to read
                var nextChars = m_Buffers[m_NextBufferIndex].GetManagedArray();
                m_ReadBlockTask = Reader.ReadBlockAsync(nextChars, index: 0, count: nextChars.Length);
            }
            else
            {
                m_ReadBlockTask?.Dispose();
                m_ReadBlockTask = null;
            }
            
            return new UnsafeBuffer<char>(buffer.GetUnsafePtr(), count);
        }
        
        public override void Reset()
        {
            base.Reset();
            
            m_NextBufferIndex = 0;
            m_ReadBlockTask?.Dispose();
            m_ReadBlockTask = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            m_ReadBlockTask?.Dispose();
            m_ReadBlockTask = null;

            foreach (var buffer in m_Buffers)
            {
                buffer.Dispose();
            }
        }
    }

    /// <summary>
    /// A synchronous block reader. This is a simple wrapper over `StreamReader.ReadBlock`.
    /// </summary>
    unsafe class SyncBlockReader : BlockStreamReader
    {
        ManagedCharBuffer m_Buffer;

        public SyncBlockReader(Stream input, int bufferSize, bool leaveInputOpen) 
            : base (input, bufferSize, leaveInputOpen)
        {
            var charBufferSize = bufferSize / sizeof(char);
            m_Buffer = new ManagedCharBuffer(charBufferSize);
        }

        public override UnsafeBuffer<char> GetNextBlock()
        {
            var count = Reader.ReadBlock(m_Buffer.GetManagedArray(), 0, m_Buffer.GetManagedArray().Length);
            return new UnsafeBuffer<char>(m_Buffer.GetUnsafePtr(), count);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            m_Buffer.Dispose();
        }
    }
}