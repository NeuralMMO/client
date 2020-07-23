using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// Step instructions for the high-level reader API.
    ///
    /// This is used as input to control the parser.
    /// </summary>
    [Flags]
    public enum NodeType
    {
        /// <summary>
        /// Continue reading until there are no more characters.
        /// </summary>
        None               = 0,

        /// <summary>
        /// Start of an object.
        /// </summary>
        BeginObject        = 1 << 0,

        /// <summary>
        /// Start of a new member.
        /// </summary>
        ObjectKey          = 1 << 1,

        /// <summary>
        /// End of an object.
        /// </summary>
        EndObject          = 1 << 2,

        /// <summary>
        /// Start of an array/collection.
        /// </summary>
        BeginArray         = 1 << 3,

        /// <summary>
        /// End of an array/collection.
        /// </summary>
        EndArray           = 1 << 4,

        /// <summary>
        /// End of a string.
        /// </summary>
        String             = 1 << 5,

        /// <summary>
        /// End of a primitive (number, boolean, nan, etc.).
        /// </summary>
        Primitive          = 1 << 6,
        
        /// <summary>
        /// End of a comment.
        /// </summary>
        Comment            = 1 << 7,

        /// <summary>
        /// Any node type.
        /// </summary>
        Any                = ~0
    }
    
    unsafe struct NodeParser : IDisposable
    {
        struct ParseJobOutput
        {
            public int TokenNextIndex;
            public int TokenParentIndex;
            public NodeType NodeType;
            public int NodeNextIndex;
        }

        [BurstCompile]
        struct ParseJob : IJob
        {
            [NativeDisableUnsafePtrRestriction] public ParseJobOutput* Output;

            [NativeDisableUnsafePtrRestriction] public Token* Tokens;
            public int TokensLength;
            public int TokenNextIndex;
            public int TokenParentIndex;

            public NodeType TargetNodeType;
            public int TargetParentIndex;
            public int TargetNodeCount;

            [NativeDisableUnsafePtrRestriction] public int* Nodes;
            int m_NodeNextIndex;

            void Break(NodeType type)
            {
                Output->TokenNextIndex = TokenNextIndex;
                Output->TokenParentIndex = TokenParentIndex;
                Output->NodeType = type;
                Output->NodeNextIndex = m_NodeNextIndex;
            }

            public void Execute()
            {
                for (; TokenNextIndex < TokensLength; TokenNextIndex++)
                {
                    var node = NodeType.None;

                    var token = Tokens[TokenNextIndex];

                    while (Tokens[TokenNextIndex].Parent < TokenParentIndex)
                    {
                        var index = TokenParentIndex;

                        node = PopToken();

                        if (Evaluate(node, index))
                        {
                            if (TokenParentIndex < TargetParentIndex)
                            {
                                TokenParentIndex = index;
                            }

                            Break(node == NodeType.None ? NodeType.Any : node);
                            return;
                        }
                    }

                    var nodeIndex = TokenNextIndex;

                    switch (token.Type)
                    {
                        case TokenType.Array:
                        case TokenType.Object:
                        {
                            node |= token.Type == TokenType.Array ? NodeType.BeginArray : NodeType.BeginObject;
                            TokenParentIndex = TokenNextIndex;
                        }
                        break;

                        case TokenType.Primitive:
                        case TokenType.String:
                        {
                            if (token.End != -1)
                            {
                                node |= token.Type == TokenType.Primitive ? NodeType.Primitive : NodeType.String;

                                while (token.Start == -1)
                                {
                                    nodeIndex = token.Parent;
                                    token = Tokens[nodeIndex];
                                }

                                if (token.Parent == -1 || Tokens[token.Parent].Type == TokenType.Object)
                                {
                                    node |= NodeType.ObjectKey;
                                    TokenParentIndex = TokenNextIndex;
                                }
                            }
                        }
                        break;

                        case TokenType.Comment:
                        {
                            if (token.End != -1)
                            {
                                node |= NodeType.Comment;

                                while (token.Start == -1)
                                {
                                    nodeIndex = token.Parent;
                                    token = Tokens[nodeIndex];
                                }
                            }
                        }
                        break;
                    }

                    if (Evaluate(node, nodeIndex))
                    {
                        TokenNextIndex++;
                        Break(node == NodeType.None ? NodeType.Any : node);
                        return;
                    }
                }

                while (TokenParentIndex >= 0)
                {
                    var index = TokenParentIndex;
                    var token = Tokens[index];

                    if (token.End == -1 && (token.Type == TokenType.Object || token.Type == TokenType.Array))
                    {
                        Break(NodeType.None);
                        return;
                    }

                    var node = PopToken();

                    if (Evaluate(node, index))
                    {
                        if (TokenParentIndex < TargetParentIndex)
                        {
                            TokenParentIndex = index;
                        }

                        Break(node == NodeType.None ? NodeType.Any : node);
                        return;
                    }
                }

                Break(NodeType.None);
            }

            /// <summary>
            /// Evaluate user instruction to determine if we should break the parsing.
            ///
            /// @TODO Cleanup; far too many checks happening here
            /// </summary>
            bool Evaluate(NodeType node, int index)
            {
                if (TokenParentIndex <= TargetParentIndex)
                {
                    if (node == NodeType.None || (node & TargetNodeType) == node && TokenParentIndex == TargetParentIndex)
                    {
                        Nodes[m_NodeNextIndex++] = index;

                        if (m_NodeNextIndex < TargetNodeCount)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                if (node == NodeType.None)
                {
                    return false;
                }

                if ((node & TargetNodeType) != NodeType.None && (TargetParentIndex == k_IgnoreParent || TargetParentIndex >= TokenParentIndex))
                {
                    Nodes[m_NodeNextIndex++] = index;

                    if (m_NodeNextIndex >= TargetNodeCount)
                    {
                        return true;
                    }
                }

                return false;
            }

            NodeType PopToken()
            {
                var node = NodeType.None;
                var token = Tokens[TokenParentIndex];

                switch (token.Type)
                {
                    case TokenType.Array:
                        node = NodeType.EndArray;
                        break;
                    case TokenType.Object:
                        node = NodeType.EndObject;
                        break;
                }

                var parentIndex = token.Parent;

                while (parentIndex >= 0)
                {
                    var parent = Tokens[parentIndex];

                    if (parent.Type != TokenType.Primitive && parent.Type != TokenType.String && parent.Type != TokenType.Comment)
                    {
                        break;
                    }

                    if (parent.Start != -1 || parent.Parent == -1)
                    {
                        break;
                    }

                    parentIndex = parent.Parent;
                }

                TokenParentIndex = parentIndex;
                return node;
            }
        }

        const int k_DefaultBatchSize = 64;

        /// <summary>
        /// One less than the minimum parent (i.e. -1)
        /// </summary>
        public const int k_IgnoreParent = -2;

        struct Data
        {
            public JsonTokenizer Tokenizer;
            public int* Nodes;
            public int NodeLength;
            public int TokenNextIndex;
            public int TokenParentIndex;
            public int NodeNextIndex;
            public NodeType NodeType;
        }

        readonly Allocator m_Label;
        Data* m_Data;

        public int* Nodes => m_Data->Nodes;
        public int NodeNextIndex => m_Data->NodeNextIndex;
        public NodeType NodeType => m_Data->NodeType;
        public int Node => m_Data->NodeNextIndex <= 0 ? -1 : m_Data->Nodes[m_Data->NodeNextIndex - 1];

        /// <summary>
        /// Number of tokens processed.
        /// </summary>
        public int TokenNextIndex => m_Data->TokenNextIndex;

        public int TokenParentIndex => m_Data->TokenParentIndex;

        public NodeParser(JsonTokenizer tokenizer, Allocator label) : this(tokenizer, k_DefaultBatchSize, label)
        {
        }

        public NodeParser(JsonTokenizer tokenizer, int batchSize, Allocator label)
        {
            m_Label = label;
            m_Data = (Data*) UnsafeUtility.Malloc(sizeof(Data), UnsafeUtility.AlignOf<Data>(), label);
            UnsafeUtility.MemClear(m_Data, sizeof(Data));
            
            m_Data->Tokenizer = tokenizer;
            m_Data->NodeType = NodeType.None;

            if (batchSize < 1)
            {
                throw new ArgumentException("batchSize < 1");
            }
            
            m_Data->Nodes = (int*) UnsafeUtility.Malloc(sizeof(int) * batchSize, 4, m_Label);
            m_Data->NodeLength = batchSize;
            m_Data->NodeNextIndex = 0;
            m_Data->TokenNextIndex = 0;
            m_Data->TokenParentIndex = -1;
        }

        /// <summary>
        /// Seeks the parser to the given token/parent combination.
        /// </summary>
        public void Seek(int index, int parent)
        {
            m_Data->TokenNextIndex = index;
            m_Data->TokenParentIndex = parent;
        }

        /// <summary>
        /// Reads the next node from the input stream and advances the position by one.
        /// </summary>
        public NodeType Step()
        {
            Step(NodeType.Any);
            return NodeType;
        }

        /// <summary>
        /// Reads until the given node type and advances the position.
        /// <param name="type">The node type to break at.</param>
        /// <param name="parent">The minimum parent to break at.</param>
        /// </summary>
        public void Step(NodeType type, int parent = k_IgnoreParent)
        {
            StepBatch(1, type, parent);
        }

        /// <summary>
        /// Reads until the given number of matching nodes have been read.
        /// </summary>
        /// <param name="count">The maximum number of elements of the given type/parent to read.</param>
        /// <param name="type">The node type to break at.</param>
        /// <param name="parent">The minimum parent to break at.</param>
        /// <returns>The number of batch elements that have been read.</returns>
        public int StepBatch(int count, NodeType type, int parent = k_IgnoreParent)
        {
            if (m_Data->NodeLength < count)
            {
                UnsafeUtility.Free(m_Data->Nodes, m_Label);
                
                m_Data->NodeLength = count;
                m_Data->Nodes = (int*) UnsafeUtility.Malloc(sizeof(int) * m_Data->NodeLength, 4, m_Label);
            }

            var output = new ParseJobOutput();

            new ParseJob
            {
                Output = &output,
                Tokens = m_Data->Tokenizer.Tokens,
                TokensLength = m_Data->Tokenizer.TokenNextIndex,
                TokenNextIndex = m_Data->TokenNextIndex,
                TokenParentIndex = m_Data->TokenParentIndex,
                TargetNodeType = type,
                TargetParentIndex = parent,
                TargetNodeCount = count,
                Nodes = m_Data->Nodes
            }
            .Run();

            m_Data->TokenNextIndex = output.TokenNextIndex;
            m_Data->TokenParentIndex = output.TokenParentIndex;
            m_Data->NodeNextIndex = output.NodeNextIndex;

            m_Data->NodeType = output.NodeType;

            return output.NodeNextIndex;
        }

        public void Dispose()
        {
            if (null == m_Data)
            {
                return;
            }
            UnsafeUtility.Free(m_Data->Nodes, m_Label);
            UnsafeUtility.Free(m_Data, m_Label);
            m_Data = null;
        }
    }
}