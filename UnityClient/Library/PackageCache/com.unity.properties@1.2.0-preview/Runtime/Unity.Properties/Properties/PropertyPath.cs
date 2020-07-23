#if !NET_DOTS
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties.Internal;

namespace Unity.Properties
{
    /// <summary>
    /// A <see cref="PropertyPath"/> is used to store a reference to a single property within a tree.
    /// </summary>
    /// <remarks>
    /// The path is stored as an array of parts and can be easily queried for algorithms.
    /// </remarks>
    public class PropertyPath : IEquatable<PropertyPath>
    {
        /// <summary>
        /// A <see cref="PartType"/> specifies a type for a <see cref="Part"/>.
        /// </summary>
        public enum PartType
        {
            /// <summary>
            /// Represents a named part of the path.
            /// </summary>
            Name,
            
            /// <summary>
            /// Represents an indexed part of the path.
            /// </summary>
            Index,
            
            /// <summary>
            /// Represents a keyed part of the path.
            /// </summary>
            Key
        }

        /// <summary>
        /// A <see cref="Part"/> represents a single element of the path.
        /// </summary>
        /// <remarks>
        /// <see cref="PartType.Name"/>  -> ".{name}"
        /// <see cref="PartType.Index"/> -> "[{index}]"
        /// <see cref="PartType.Key"/>   -> "[{key}]"
        /// </remarks>
        public struct Part : IEquatable<Part>
        {
            [CreateProperty] PartType m_Type;
            [CreateProperty] string m_Name;
            [CreateProperty] int m_Index;
            [CreateProperty] object m_Key;

            /// <summary>
            /// Returns true if the part is <see cref="PartType.Name"/>.
            /// </summary>
            public bool IsName => Type == PartType.Name;

            /// <summary>
            /// Returns true if the part is <see cref="PartType.Index"/>.
            /// </summary>
            public bool IsIndex => Type == PartType.Index;
            
            /// <summary>
            /// Returns true if the part is <see cref="PartType.Key"/>.
            /// </summary>
            public bool IsKey => Type == PartType.Key;

            /// <summary>
            /// The <see cref="PartType"/> for this path. This determines how algorithms will resolve the path.
            /// </summary>
            public PartType Type => m_Type;
            
            /// <summary>
            /// The Name of the part. This will only be set when using <see cref="PartType.Name"/>
            /// </summary>
            public string Name
            {
                get
                {
                    CheckType(PartType.Name);
                    return m_Name;
                }
            }

            /// <summary>
            /// The Index of the part. This will only be set when using <see cref="PartType.Index"/>
            /// </summary>
            public int Index
            {
                get
                {
                    CheckType(PartType.Index);
                    return m_Index;
                }
            }
            
            /// <summary>
            /// The Key of the part. This will only be set when using <see cref="PartType.Key"/>
            /// </summary>
            public object Key
            {
                get
                {
                    CheckType(PartType.Key);
                    return m_Key;
                }
            }

            /// <summary>
            /// Initializes a new <see cref="Part"/> with the specified name.
            /// </summary>
            /// <param name="name">The name of the part.</param>
            public Part(string name)
            {
                m_Type = PartType.Name;
                m_Name = name;
                m_Index = -1;
                m_Key = null;
            }

            /// <summary>
            /// Initializes a new <see cref="Part"/> with the specified index.
            /// </summary>
            /// <param name="index">The index of the part.</param>
            public Part(int index)
            {
                m_Type = PartType.Index;
                m_Name = string.Empty;
                m_Index = index;
                m_Key = null;
            }

            /// <summary>
            /// Initializes a new <see cref="Part"/> with the specified key.
            /// </summary>
            /// <param name="key">The key of the part.</param>
            public Part(object key)
            {
                m_Type = PartType.Key;
                m_Name = string.Empty;
                m_Index = -1;
                m_Key = key;
            }

            void CheckType(PartType type)
            {
                if (type != Type) throw new InvalidOperationException();
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                switch (Type)
                {
                    case PartType.Name:
                        return m_Name;
                    case PartType.Index:
                        return "[" + m_Index + "]";
                    case PartType.Key:
                        return "[\"" + m_Key + "\"]";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /// <inheritdoc/>
            public bool Equals(Part other)
            {
                return m_Type == other.m_Type && m_Name == other.m_Name && m_Index == other.m_Index && Equals(m_Key, other.m_Key);
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                return obj is Part other && Equals(other);
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (int) m_Type;
                    hashCode = (hashCode * 397) ^ (m_Name != null ? m_Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ m_Index;
                    hashCode = (hashCode * 397) ^ (m_Key != null ? m_Key.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        /// <summary>
        /// Internal pooling for <see cref="PropertyPath"/>.
        /// </summary>
        internal static readonly Pool<PropertyPath> Pool = new Pool<PropertyPath>(() => new PropertyPath(), p => p.Clear());
        
        [CreateProperty] readonly List<Part> m_Parts = new List<Part>(32);
        
        /// <summary>
        /// Gets the number of parts contained in the <see cref="PropertyPath"/>.
        /// </summary>
        public int PartsCount => m_Parts.Count;
        
        /// <summary>
        /// Gets if there is any part contained in the <see cref="PropertyPath"/>.
        /// </summary>
        public bool Empty => m_Parts.Count == 0;
        
        /// <summary>
        /// Gets the <see cref="Part"/> at the given index.
        /// </summary>
        public Part this[int index] => m_Parts[index];
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPath"/> class that is empty.
        /// </summary>
        public PropertyPath() : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyPath"/> based on the given property string.
        /// </summary>
        /// <param name="path">The string path to initialize this instance with.</param>
        public PropertyPath(string path)
        {
            ConstructFromPath(path);   
        }

        /// <summary>
        /// Appends all parts of the given <see cref="PropertyPath"/> to the end of the this <see cref="PropertyPath"/>.
        /// </summary>
        /// <param name="path">The <see cref="PropertyPath"/> to append.</param>
        public void PushPath(PropertyPath path)
        {
            for (var i = 0; i < path.PartsCount; ++i)
            {
                PushPart(path[i]);
            }
        }

        /// <summary>
        /// Appends the given <see cref="Part"/> to this property path.
        /// </summary>
        /// <param name="part">The part to add.</param>
        public void PushPart(Part part) => m_Parts.Add(part);

        /// <summary>
        /// Appends the given name to the <see cref="PropertyPath"/>. This will use <see cref="PartType.Name"/>.
        /// </summary>
        /// <param name="name">The part name to add.</param>
        public void PushName(string name) => m_Parts.Add(new Part(name));
        
        /// <summary>
        /// Appends the given index to the <see cref="PropertyPath"/>. This will use <see cref="PartType.Index"/>.
        /// </summary>
        /// <param name="index">The index to add.</param>
        public void PushIndex(int index) => m_Parts.Add(new Part(index));
        
        /// <summary>
        /// Appends the given key to the <see cref="PropertyPath"/>. This will use <see cref="PartType.Key"/>.
        /// </summary>
        /// <param name="key">The key to add.</param>
        public void PushKey(object key) => m_Parts.Add(new Part(key));

        /// <summary>
        /// Appends the given property to the <see cref="PropertyPath"/>. This will use the correct <see cref="PartType"/> based on the property interfaces.
        /// </summary>
        /// <param name="property">The property to add.</param>
        public void PushProperty(IProperty property)
        {
            switch (property)
            {
                case IListElementProperty indexable:
                    PushIndex(indexable.Index);
                    break;
                case IDictionaryElementProperty keyable:
                    PushKey(keyable.ObjectKey);
                    break;
                default:
                    PushName(property.Name);
                    break;
            }
        }

        /// <summary>
        /// Removes the last <see cref="Part"/> from the <see cref="PropertyPath"/>.
        /// </summary>
        public void Pop() => m_Parts.RemoveAt(m_Parts.Count - 1);

        /// <summary>
        /// Clears all parts from the <see cref="PropertyPath"/>.
        /// </summary>
        public void Clear() => m_Parts.Clear();

        /// <inheritdoc/>
        public override string ToString()
        {
            if (m_Parts.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(32);

            foreach (var part in m_Parts)
            {
                switch (part.Type)
                {
                    case PartType.Name:
                        if (builder.Length > 0)
                            builder.Append('.');
                        builder.Append(part);
                    break;
                    
                    case PartType.Index:
                        builder.Append(part);
                        break;
                    case PartType.Key:
                        builder.Append(part);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return builder.ToString();
        }
        
        /// <summary>
        /// Appends the specified path.
        /// </summary>
        /// <param name="path">The string path to append.</param>
        public void AppendPath(string path)
        {
            ConstructFromPath(path);    
        }

        void ConstructFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            var parts = path.TrimStart('[').Split('.', '[');
            
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    throw new ArgumentException($"Invalid {nameof(PropertyPath)}: Part is empty.");
                }
                
                if (part.EndsWith("]"))
                {
                    var content = part.Substring(0, part.Length - 1);

                    if (string.IsNullOrWhiteSpace(part))
                    {
                        throw new ArgumentException($"Invalid {nameof(PropertyPath)}: Index or key is empty.");
                    }
                    
                    if (content.StartsWith("\""))
                    {
                        if (content.EndsWith("\""))
                        {
                            var key = content.Substring(1, content.Length - 2);
                            
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                throw new ArgumentException($"Invalid {nameof(PropertyPath)}: Key is null or empty.");
                            }
                            
                            PushKey((object) key);
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid {nameof(PropertyPath)}: No matching end quote for key.");
                        }
                    }
                    else if (int.TryParse(content, out var index))
                    {
                        if (index < 0)
                        {
                            throw new ArgumentException($"Invalid {nameof(PropertyPath)}: Negative indices are not supported.");    
                        }
                        
                        PushIndex(index);
                    }
                    else
                    {
                        throw new ArgumentException($"Indices in {nameof(PropertyPath)} must be a numeric value.");
                    }
                }
                else
                {
                    PushName(part);
                }
            }
        }

        /// <inheritdoc/>
        public bool Equals(PropertyPath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (PartsCount != other.PartsCount)
            {
                return false;
            }

            for (var i = 0; i < PartsCount; ++i)
            {
                if (!this[i].Equals(other[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyPath) obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashcode = 19;
            foreach (var part in m_Parts)
            {
                hashcode = hashcode * 31 + part.GetHashCode();
            }

            return hashcode;
        }
    }
}
#endif