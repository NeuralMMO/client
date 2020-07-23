using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Properties.Internal;

namespace Unity.Entities
{
    readonly unsafe struct DynamicBufferContainer<TElement> : IList<TElement>
    {
        static DynamicBufferContainer()
        {
            PropertyBagStore.AddPropertyBag(new ListPropertyBag<DynamicBufferContainer<TElement>, TElement>());
        }

        readonly BufferHeader* m_Buffer;
        readonly bool m_IsReadOnly;

        public int Count => m_Buffer->Length;
        public bool IsReadOnly => true;

        public DynamicBufferContainer(BufferHeader* buffer, bool readOnly = true)
        {
            m_Buffer = buffer;
            m_IsReadOnly = readOnly;
        }

        public TElement this[int index]
        {
            get
            {
                CheckBounds(index);
                return UnsafeUtility.ReadArrayElement<TElement>(BufferHeader.GetElementPointer(m_Buffer), index);
            }
            set
            {
                // @FIXME
                //
                // In C# despite being `readonly` a list can have it's elements mutated, however for ECS data we have strict access writes.
                // For now we opt to silently skip until a proper fix is implemented.
                //
                // In order to properly fix this we need either:
                //
                // 1) A custom property bag for DynamicBufferContainer`1 which correctly sets IsReadOnly per element property.
                //    * While this is a more elegant solution we lose the built in machinery around ListPropertyBag`1. e.g. UI would not be quite right.
                //
                // 2) A fix directly in ListPropertyBag`1 to allow controlling IsReadOnly per element.
                //    * This is a best place to fix it but requires a package update of properties.
                //
                if (!m_IsReadOnly)
                {
                    CheckBounds(index);
                    UnsafeUtility.WriteArrayElement(BufferHeader.GetElementPointer(m_Buffer), index, value);
                }
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TElement item) => throw new InvalidOperationException();
        public void Clear() => throw new InvalidOperationException();
        public bool Contains(TElement item) => throw new InvalidOperationException();
        public void CopyTo(TElement[] array, int arrayIndex) => throw new InvalidOperationException();
        public bool Remove(TElement item) => throw new InvalidOperationException();
        public int IndexOf(TElement item) => throw new InvalidOperationException();
        public void Insert(int index, TElement item) => throw new InvalidOperationException();
        public void RemoveAt(int index) => throw new InvalidOperationException();

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckBounds(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if ((uint)index >= (uint)Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range in DynamicBufferContainer of '{Count}' Count.");
#endif
        }

        public override int GetHashCode()
        {
            return (int)math.hash(new uint2((uint)m_Buffer, (uint)Count));
        }
    }
}
