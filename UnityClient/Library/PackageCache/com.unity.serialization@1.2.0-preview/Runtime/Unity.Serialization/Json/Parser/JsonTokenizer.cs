using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// The tokenizer is the lowest level API for json deserialization.
    ///
    /// It's only job is to parse characters into an array of <see cref="Token"/> simple structs.
    ///
    /// e.g. {"foo": 10} becomes
    ///
    ///  [0] Type=[JsonType.Object]    Range=[0..11] Parent=[-1]
    ///  [1] Type=[JsonType.String]    Range=[2..5]  Parent=[0]
    ///  [2] Type=[JsonType.Primitive] Range=[8..10] Parent=[1]
    ///
    /// @NOTE No conversion or copying of data takes place here.
    ///
    /// Implementation based off of https://github.com/zserge/jsmn
    /// </summary>
    unsafe struct JsonTokenizer : IDisposable
    {
        /// <summary>
        /// Special start value to signify this is a partial token continuation.
        /// </summary>
        const int k_PartialTokenStart = -1;

        /// <summary>
        /// Special end value to signify there is another part to follow.
        /// </summary>
        const int k_PartialTokenEnd = -1;

        /// <summary>
        /// All input characters were consumed and all tokens were generated.
        /// </summary>
        const int k_ResultSuccess = 0;

        /// <summary>
        /// The token buffer could not fit all tokens.
        /// </summary>
        const int k_ResultTokenBufferOverflow = -1;

        /// <summary>
        /// The input data was invalid in some way.
        /// </summary>
        const int k_ResultInvalidInput = -2;

        /// <summary>
        /// All input characters were consumed and the writer is expecting more
        /// </summary>
        const int k_ResultEndOfStream = -3;

        /// <summary>
        /// The maximum depth limit has been exceeded.
        /// </summary>
        const int k_ResultStackOverflow = -4;

        /// <summary>
        /// Maximum depth limit for discarding completed tokens.
        /// </summary>
        const int k_DefaultDepthLimit = 128;

        /// <summary>
        /// Internal struct used to track the type of comment being parsed.
        /// </summary>
        enum CommentType
        {
            /// <summary>
            /// The comment type is not yet known. E.g. we have only encountered the first `/`.
            /// </summary>
            Unknown,
            
            /// <summary>
            /// Single line comment prefixed with `//` and ending in `\n`.
            /// </summary>
            SingleLine,
            
            /// <summary>
            /// Multi line comment prefixed with `/*` and ending with `*/`.
            /// </summary>
            MultiLine
        }
        
        struct TokenizeJobOutput
        {
            public Token* Tokens;
            public int Result;
            public int BufferPosition;
            public int TokensLength;
            public int TokenNextIndex;
            public int TokenParentIndex;
            public ushort PrevChar;
            public CommentType CommentType;
        }

        /// <summary>
        /// Transforms raw input characters to <see cref="Token"/> objects.
        ///
        /// Only structural validation is performed.
        /// </summary>
        [BurstCompile]
        struct TokenizeJob : IJob
        {
            [NativeDisableUnsafePtrRestriction] public TokenizeJobOutput* Output;

            [NativeDisableUnsafePtrRestriction] public ushort* CharBuffer;
            public int CharBufferLength;
            public int CharBufferPosition;
            public ushort PrevChar;

            [NativeDisableUnsafePtrRestriction] public Token* Tokens;
            public int TokensLength;
            public int TokensNextIndex;
            public int TokenParentIndex;
            public CommentType CommentType;
            
            public Allocator Label;

            void Break(int result)
            {
                Output->Tokens = Tokens;
                Output->Result = result;
                Output->BufferPosition = CharBufferPosition;
                Output->TokensLength = TokensLength;
                Output->TokenNextIndex = TokensNextIndex;
                Output->TokenParentIndex = TokenParentIndex;
                Output->PrevChar = PrevChar;
                Output->CommentType = CommentType;
            }

            void GrowTokenBuffer()
            {
                var newLength = TokensLength * 2;
                Tokens = NativeArrayUtility.Resize(Tokens, TokensLength, newLength, 4, Label);
                TokensLength = newLength;
            }

            public void Execute()
            {
                if (TokensNextIndex >= TokensLength)
                {
                    GrowTokenBuffer();
                }

                // Handle re-entry with a open token on the `stack`
                if (TokensNextIndex - 1 >= 0 && Tokens[TokensNextIndex - 1].End == -1)
                {
                    var token = Tokens[TokensNextIndex - 1];

                    switch (token.Type)
                    {
                        case TokenType.String:
                        {
                            var result = ParseString(TokensNextIndex - 1, k_PartialTokenStart);

                            if (result != k_ResultSuccess)
                            {
                                Break(result);
                                return;
                            }

                            CharBufferPosition++;
                        }
                            break;
                        
                        case TokenType.Primitive:
                        {
                            var result = ParsePrimitive(TokensNextIndex - 1, k_PartialTokenStart);

                            if (result != k_ResultSuccess)
                            {
                                Break(result);
                                return;
                            }

                            CharBufferPosition++;
                        }
                            break;
                        
                        case TokenType.Comment:
                        {
                            var result = ParseComment(TokensNextIndex - 1, k_PartialTokenStart);

                            if (result != k_ResultSuccess)
                            {
                                Break(result);
                                return;
                            }

                            CharBufferPosition++;
                        }
                            break;
                    }
                }

                for (; CharBufferPosition < CharBufferLength; CharBufferPosition++)
                {
                    var c = CharBuffer[CharBufferPosition];

                    switch (c)
                    {
                        case '{':
                        case '[':
                        {
                            if (TokensNextIndex >= TokensLength)
                            {
                                GrowTokenBuffer();
                            }

                            Tokens[TokensNextIndex++] = new Token
                            {
                                Type = c == '{' ? TokenType.Object : TokenType.Array,
                                Parent = TokenParentIndex,
                                Start = CharBufferPosition,
                                End = -1
                            };

                            TokenParentIndex = TokensNextIndex - 1;
                        }
                            break;

                        case '}':
                        case ']':
                        {
                            var type = c == '}' ? TokenType.Object : TokenType.Array;

                            if (TokensNextIndex < 1)
                            {
                                Break(k_ResultInvalidInput);
                                return;
                            }

                            var index = TokensNextIndex - 1;

                            for (;;)
                            {
                                var token = Tokens[index];

                                if (token.Start != k_PartialTokenStart && token.End == k_PartialTokenEnd && token.Type != TokenType.String && token.Type != TokenType.Primitive)
                                {
                                    if (token.Type != type)
                                    {
                                        Break(k_ResultInvalidInput);
                                        return;
                                    }

                                    TokenParentIndex = token.Parent;
                                    token.End = CharBufferPosition + 1;
                                    Tokens[index] = token;
                                    break;
                                }

                                if (token.Parent == -1)
                                {
                                    if (token.Type != type || TokenParentIndex == -1)
                                    {
                                        Break(k_ResultInvalidInput);
                                        return;
                                    }

                                    break;
                                }

                                index = token.Parent;
                            }

                            var parent = TokenParentIndex != -1 ? Tokens[TokenParentIndex] : default;

                            if (TokenParentIndex != -1 &&
                                parent.Type != TokenType.Object &&
                                parent.Type != TokenType.Array)
                            {
                                TokenParentIndex = Tokens[TokenParentIndex].Parent;
                            }
                        }
                            break;

                        case '/':
                        {
                            CharBufferPosition++;
                            
                            PrevChar = 0;
                            CommentType = CommentType.Unknown;
                                
                            var result = ParseComment(TokenParentIndex, CharBufferPosition + 1);
                            
                            if (result == k_ResultInvalidInput)
                            {
                                Break(result);
                                return;
                            }
                        }
                            break;
                        
                        case '\t':
                        case '\r':
                        case ' ':
                        case '\n':
                        case '\0':
                        case ':':
                        case '=':
                        case ',':
                        {
                        }
                            break;

                        default:
                        {
                            int result;

                            if (c == '"')
                            {
                                CharBufferPosition++;

                                PrevChar = 0;
                                
                                result = ParseString(TokenParentIndex, CharBufferPosition);
                                
                                if (result == k_ResultInvalidInput)
                                {
                                    Break(result);
                                    return;
                                }
                            }
                            else
                            {
                                var start = CharBufferPosition;

                                result = ParsePrimitive(TokenParentIndex, start);

                                if (result == k_ResultInvalidInput)
                                {
                                    Break(result);
                                    return;
                                }
                            }

                            if (TokenParentIndex == -1 || Tokens[TokenParentIndex].Type == TokenType.Object)
                            {
                                TokenParentIndex = TokensNextIndex - 1;
                            }
                            else if (TokenParentIndex != -1 &&
                                     Tokens[TokenParentIndex].Type != TokenType.Object &&
                                     Tokens[TokenParentIndex].Type != TokenType.Array)
                            {
                                TokenParentIndex = Tokens[TokenParentIndex].Parent;
                            }

                            if (result == k_ResultEndOfStream)
                            {
                                Break(result);
                                return;
                            }
                        }
                        break;
                    }
                }

                Break(k_ResultSuccess);
            }

            int ParseString(int parent, int start)
            {
                for (; CharBufferPosition < CharBufferLength; CharBufferPosition++)
                {
                    var c = CharBuffer[CharBufferPosition];

                    if (c == '"' && PrevChar != '\\')
                    {
                        if (TokensNextIndex >= TokensLength)
                        {
                            GrowTokenBuffer();
                        }

                        Tokens[TokensNextIndex++] = new Token
                        {
                            Type = TokenType.String,
                            Parent = parent,
                            Start = start,
                            End = CharBufferPosition
                        };

                        break;
                    }

                    PrevChar = c;
                }

                if (CharBufferPosition >= CharBufferLength)
                {
                    if (TokensNextIndex >= TokensLength)
                    {
                        GrowTokenBuffer();
                    }

                    Tokens[TokensNextIndex++] = new Token
                    {
                        Type = TokenType.String,
                        Parent = parent,
                        Start = start,
                        End = -1
                    };

                    return k_ResultEndOfStream;
                }

                return k_ResultSuccess;
            }

            int ParsePrimitive(int parent, int start)
            {
                for (; CharBufferPosition < CharBufferLength; CharBufferPosition++)
                {
                    var c = CharBuffer[CharBufferPosition];

                    if (c == ' ' ||
                        c == '\t' ||
                        c == '\r' ||
                        c == '\n' ||
                        c == '\0' ||
                        c == ',' ||
                        c == ']' ||
                        c == '}' ||
                        c == ':' ||
                        c == '=')
                    {
                        if (TokensNextIndex >= TokensLength)
                        {
                            GrowTokenBuffer();
                        }

                        Tokens[TokensNextIndex++] = new Token
                        {
                            Type = TokenType.Primitive,
                            Parent = parent,
                            Start = start,
                            End = CharBufferPosition
                        };

                        CharBufferPosition--;
                        break;
                    }

                    if (c < 32 || c >= 127)
                    {
                        return k_ResultInvalidInput;
                    }
                }

                if (CharBufferPosition >= CharBufferLength)
                {
                    if (TokensNextIndex >= TokensLength)
                    {
                        GrowTokenBuffer();
                    }

                    Tokens[TokensNextIndex++] = new Token
                    {
                        Type = TokenType.Primitive,
                        Parent = parent,
                        Start = start,
                        End = -1
                    };

                    return k_ResultEndOfStream;
                }

                return k_ResultSuccess;
            }

            int ParseComment(int parent, int start)
            {
                for (; CharBufferPosition < CharBufferLength; CharBufferPosition++)
                {
                    var c = CharBuffer[CharBufferPosition];

                    switch (CommentType)
                    {
                        case CommentType.Unknown:
                        {
                            switch ((char) c)
                            {
                                case '/':
                                    CommentType = CommentType.SingleLine;
                                    continue;
                                case '*':
                                    CommentType = CommentType.MultiLine;
                                    continue;
                                default:
                                    return k_ResultInvalidInput;
                            }
                        }

                        case CommentType.SingleLine:
                        {
                            switch ((char) c)
                            {
                                case '\n':
                                case '\0':
                                {
                                    if (TokensNextIndex >= TokensLength)
                                    {
                                        GrowTokenBuffer();
                                    }

                                    Tokens[TokensNextIndex++] = new Token
                                    {
                                        Type = TokenType.Comment,
                                        Parent = parent,
                                        Start = start,
                                        End = CharBufferPosition - 1
                                    };

                                    return k_ResultSuccess;
                                }
                            }
                        }
                            break;

                        case CommentType.MultiLine:
                        {
                            if (c == '/' && PrevChar == '*')
                            {
                                if (TokensNextIndex >= TokensLength)
                                {
                                    GrowTokenBuffer();
                                }

                                Tokens[TokensNextIndex++] = new Token
                                {
                                    Type = TokenType.Comment,
                                    Parent = parent,
                                    Start = start,
                                    End = CharBufferPosition - 1
                                };
                                
                                return k_ResultSuccess;
                            }
                        }
                            break;
                    }

                    PrevChar = c;
                }
                
                if (CharBufferPosition >= CharBufferLength)
                {
                    if (TokensNextIndex >= TokensLength)
                    {
                        GrowTokenBuffer();
                    }

                    Tokens[TokensNextIndex++] = new Token
                    {
                        Type = TokenType.Comment,
                        Parent = parent,
                        Start = start,
                        End = -1
                    };
                }
                
                return k_ResultEndOfStream;
            }
        }

        struct DiscardCompletedJobOutput
        {
            public int Result;
            public int ParentTokenIndex;
            public int NextTokenIndex;
        }

        /// <summary>
        /// Trims all completed sibling tokens.
        /// </summary>
        [BurstCompile]
        struct DiscardCompletedJob : IJob
        {
            [NativeDisableUnsafePtrRestriction] public DiscardCompletedJobOutput* Output;

            [NativeDisableUnsafePtrRestriction] public Token* JsonTokens;
            [NativeDisableUnsafePtrRestriction] public int* Remap;
            public int JsonTokenParentIndex;
            public int JsonTokenNextIndex;
            public int StackSize;

            public void Execute()
            {
                var stack = stackalloc int[StackSize];
                var sp = -1;

                var index = JsonTokenNextIndex - 1;

                for (;;)
                {
                    if (index == -1)
                    {
                        break;
                    }

                    var token = JsonTokens[index];

                    if (token.Start != k_PartialTokenStart)
                    {
                        // Support partial tokens
                        if (token.End == k_PartialTokenEnd && (token.Type == TokenType.Primitive || token.Type == TokenType.String))
                        {
                            var partToken = token;
                            var partIndex = index;
                            var partCount = 1;

                            while (partToken.End == -1 && partIndex < JsonTokenNextIndex - 1)
                            {
                                partCount++;
                                partToken = JsonTokens[++partIndex];
                            }

                            if (sp + partCount >= StackSize)
                            {
                                Output->Result = k_ResultTokenBufferOverflow;
                                return;
                            }

                            for (var i = partCount - 1; i >= 0; i--)
                            {
                                stack[++sp] = index + i;
                            }
                        }
                        else
                        {
                            if (sp + 1>= StackSize)
                            {
                                Output->Result = k_ResultTokenBufferOverflow;
                                return;
                            }

                            stack[++sp] = index;
                        }
                    }

                    index = token.Parent;
                }

                JsonTokenNextIndex = sp + 1;

                for (var i = 0; sp >= 0; i++, sp--)
                {
                    index = stack[sp];

                    if (JsonTokenParentIndex == index)
                    {
                        JsonTokenParentIndex = i;
                    }

                    var token = JsonTokens[index];

                    var parentIndex = i - 1;

                    if (token.Start != k_PartialTokenStart)
                    {
                        while (parentIndex != -1 && JsonTokens[parentIndex].Start == k_PartialTokenStart)
                        {
                            parentIndex--;
                        }
                    }

                    token.Parent = parentIndex;
                    JsonTokens[i] = token;
                    Remap[index] = i;
                }

                Output->NextTokenIndex = JsonTokenNextIndex;
                Output->ParentTokenIndex = JsonTokenParentIndex;
            }
        }

        const int k_DefaultBufferSize = 1024;

        struct Data
        {
            public int BufferSize;
            public Token* JsonTokens;
            public int* DiscardRemap;
            public int TokenNextIndex;
            public int TokenParentIndex;
            public ushort PrevChar;
            public CommentType CommentType;
            public JsonValidationType ValidationType;
            public JsonStandardValidator StandardValidator;
            public JsonSimpleValidator SimpleValidator;
        }

        readonly Allocator m_Label;
        Data* m_Data;

        /// <inheritdoc />
        public Token* Tokens => m_Data->JsonTokens;

        internal int* DiscardRemap => m_Data->DiscardRemap;

        /// <inheritdoc />
        public int TokenNextIndex => m_Data->TokenNextIndex;

        /// <inheritdoc />
        public int TokenParentIndex => m_Data->TokenParentIndex;

        public JsonTokenizer(Allocator label)
            : this (k_DefaultBufferSize, JsonValidationType.None, label)
        {
        }

        public JsonTokenizer(JsonValidationType validation, Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
            : this (k_DefaultBufferSize, validation, label)
        {
        }

        public JsonTokenizer(int bufferSize = k_DefaultBufferSize, JsonValidationType validation = JsonValidationType.None, Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
        {
            m_Label = label;
            m_Data = (Data*) UnsafeUtility.Malloc(sizeof(Data), UnsafeUtility.AlignOf<Data>(), label);
            UnsafeUtility.MemClear(m_Data, sizeof(Data));
            
            m_Data->BufferSize = bufferSize;
            m_Data->ValidationType = validation;
            
            if (bufferSize <= 0)
            {
                throw new ArgumentException($"Token buffer size {bufferSize} <= 0");
            }

            m_Data->JsonTokens = (Token*) UnsafeUtility.Malloc(bufferSize * sizeof(Token), 4, label);
            m_Data->DiscardRemap = (int*) UnsafeUtility.Malloc(bufferSize * sizeof(int), 4, label);

            switch (validation)
            {
                case JsonValidationType.Standard:
                    m_Data->StandardValidator = new JsonStandardValidator(label);
                    break;
                case JsonValidationType.Simple:
                    m_Data->SimpleValidator = new JsonSimpleValidator(label);
                    break;
            }

            m_Data->TokenNextIndex = 0;
            m_Data->TokenParentIndex = -1;
            m_Data->PrevChar = 0;
        }

        /// <summary>
        /// Initializes the tokenizer for re-use.
        /// </summary>
        public void Reset()
        {
            m_Data->TokenNextIndex = 0;
            m_Data->TokenParentIndex = -1;
            m_Data->PrevChar = 0;

            switch (m_Data->ValidationType)
            {
                case JsonValidationType.Standard:
                    m_Data->StandardValidator.Reset();
                    break;
                case JsonValidationType.Simple:
                    m_Data->SimpleValidator.Reset();
                    break;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Writes <see cref="T:Unity.Serialization.Token" /> objects to the internal buffer.
        /// </summary>s
        /// <param name="buffer">A character array containing the input json data to tokenize.</param>
        /// <param name="start">The index of ptr at which to begin reading.</param>
        /// <param name="count">The maximum number of characters to read.</param>
        /// <returns>The number of characters that have been read.</returns>
        public int Write(UnsafeBuffer<char> buffer, int start, int count)
        {
            if (start + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            var position = start;
            
            for (;;)
            {
                var output = new TokenizeJobOutput();

                var handle = new TokenizeJob
                {
                    Output = &output,
                    CharBuffer = (ushort*) buffer.Buffer,
                    CharBufferLength = start + count,
                    CharBufferPosition = position,
                    PrevChar = m_Data->PrevChar,
                    CommentType = m_Data->CommentType,
                    Tokens = m_Data->JsonTokens,
                    TokensLength = m_Data->BufferSize,
                    TokensNextIndex = m_Data->TokenNextIndex,
                    TokenParentIndex = m_Data->TokenParentIndex,
                    Label = m_Label
                }.Schedule();

                var validation = default(JobHandle);

                switch (m_Data->ValidationType)
                {
                    case JsonValidationType.Standard: 
                        validation = m_Data->StandardValidator.ScheduleValidation(buffer, position, count);
                        break;
                    case JsonValidationType.Simple: 
                        validation = m_Data->SimpleValidator.ScheduleValidation(buffer, position, count);
                        break;
                }
                
                JobHandle.CombineDependencies(handle, validation).Complete();

                var result = default(JsonValidationResult);
                
                switch (m_Data->ValidationType)
                {
                    case JsonValidationType.Standard: 
                        result = m_Data->StandardValidator.GetResult();
                        break;
                    case JsonValidationType.Simple: 
                        result = m_Data->SimpleValidator.GetResult();
                        break;
                }
                
                if (!result.IsValid() && result.ActualType != JsonType.EOF)
                {
                    throw new InvalidJsonException(result.ToString())
                    {
                        Line = result.LineCount,
                        Character = result.CharCount
                    };
                }
                
                position = output.BufferPosition;

                m_Data->JsonTokens = output.Tokens;
                m_Data->TokenNextIndex = output.TokenNextIndex;
                m_Data->TokenParentIndex = output.TokenParentIndex;
                m_Data->PrevChar = output.PrevChar;
                m_Data->CommentType = output.CommentType;

                if (output.TokensLength != m_Data->BufferSize)
                {
                    m_Data->DiscardRemap = NativeArrayUtility.Resize(m_Data->DiscardRemap, m_Data->BufferSize, output.TokensLength, 4, m_Label);
                    m_Data->BufferSize = output.TokensLength;
                }

                if (output.Result == k_ResultInvalidInput)
                {
                    // No validation pass was performed.
                    // The tokenizer has failed with something that was structurally invalid.
                    throw new InvalidJsonException($"Input json was structurally invalid. Try with {nameof(JsonValidationType)}=[Standard or Simple]")
                    {
                        Line = -1,
                        Character = -1
                    };
                }

                return position - start;
            }
        }

        /// <inheritdoc />
        public void DiscardCompleted()
        {
            DiscardCompleted(k_DefaultDepthLimit);
        }

        public void DiscardCompleted(int depth)
        {
            if (m_Data->TokenNextIndex == 0)
            {
                return;
            }
            
            var output = new DiscardCompletedJobOutput();

            new DiscardCompletedJob
            {
                Output = &output,
                JsonTokens = m_Data->JsonTokens,
                Remap = m_Data->DiscardRemap,
                JsonTokenParentIndex = m_Data->TokenParentIndex,
                JsonTokenNextIndex = m_Data->TokenNextIndex,
                StackSize = depth
            }.Run();

            if (output.Result == k_ResultStackOverflow)
            {
                throw new StackOverflowException($"Tokenization depth limit of {depth} exceeded.");
            }

            m_Data->TokenNextIndex = output.NextTokenIndex;
            m_Data->TokenParentIndex = output.ParentTokenIndex;
        }

        public void Dispose()
        {
            if (null == m_Data)
            {
                return;
            }
            
            UnsafeUtility.Free(m_Data->JsonTokens, m_Label);
            UnsafeUtility.Free(m_Data->DiscardRemap, m_Label);
            
            switch (m_Data->ValidationType)
            {
                case JsonValidationType.Standard: 
                    m_Data->StandardValidator.Dispose();
                    break;
                case JsonValidationType.Simple: 
                    m_Data->SimpleValidator.Dispose();
                    break;
            }
            
            UnsafeUtility.Free(m_Data, m_Label);
            m_Data = null;
        }
    }
}