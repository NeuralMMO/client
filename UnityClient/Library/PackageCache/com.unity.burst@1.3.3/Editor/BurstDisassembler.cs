using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.Burst.Editor
{
    /// <summary>
    /// Disassembler for Intel and ARM
    /// </summary>
    internal partial class BurstDisassembler
    {
        private readonly Dictionary<int, string> _fileName;
        private readonly Dictionary<int, string[]> _fileList;
        private readonly List<AsmToken> _tokens;

        private static readonly StringSlice FileDirective = new StringSlice(".file");
        private static readonly StringSlice LocDirective = new StringSlice(".loc");

        // This is used to aligned instructions and there operands so they look like this
        //
        // mulps   x,x,x
        // shufbps x,x,x
        //
        // instead of
        //
        // mulps x,x,x
        // shufbps x,x,x
        //
        // Notice if instruction name is longer than this no alignment will be done. 
        private const int InstructionAlignment = 10;

        // Colors used for the tokens
        // TODO: Make this configurable via some editor settings?
        private const string DarkColorLineDirective = "#FFFF00";
        private const string DarkColorDirective = "#CCCCCC";
        private const string DarkColorIdentifier = "#d4d4d4";
        private const string DarkColorQualifier = "#DCDCAA";
        private const string DarkColorInstruction = "#4EC9B0";
        private const string DarkColorInstructionSIMD = "#C586C0";
        private const string DarkColorRegister = "#d7ba7d";
        private const string DarkColorNumber = "#9cdcfe";
        private const string DarkColorString = "#ce9178";
        private const string DarkColorComment = "#6A9955";

        private const string LightColorLineDirective = "#888800";
        private const string LightColorDirective = "#444444";
        private const string LightColorIdentifier = "#1c1c1c";
        private const string LightColorQualifier = "#267f99";
        private const string LightColorInstruction = "#0451a5";
        private const string LightColorInstructionSIMD = "#0000ff";
        private const string LightColorRegister = "#811f3f";
        private const string LightColorNumber = "#007ACC";
        private const string LightColorString = "#a31515";
        private const string LightColorComment = "#008000";

        private string ColorLineDirective;
        private string ColorDirective;
        private string ColorIdentifier;
        private string ColorQualifier;
        private string ColorInstruction;
        private string ColorInstructionSIMD;
        private string ColorRegister;
        private string ColorNumber;
        private string ColorString;
        private string ColorComment;

        public BurstDisassembler()
        {
            _fileName= new Dictionary<int, string>();
            _fileList=new Dictionary<int, string[]>();
            _tokens = new List<AsmToken>();
        }

        public enum AsmKind
        {
            Intel,
            ARM,
            Wasm
        }

        public string Process(string input, AsmKind asmKind, bool useDarkSkin = true, bool useSyntaxColouring = true)
        {
            UseSkin(useDarkSkin);
#if BURST_INTERNAL
            return ProcessImpl(input, asmKind, useSyntaxColouring);
#else
            try
            {
                return ProcessImpl(input, asmKind, useSyntaxColouring);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Error while trying to disassemble the input: {ex}");
            }
            // in case of an error, return the input as-is at least
            return input;
#endif
        }

        private void UseSkin(bool useDarkSkin)
        {
            if (useDarkSkin)
            {
                ColorLineDirective      = DarkColorLineDirective;
                ColorDirective          = DarkColorDirective;
                ColorIdentifier         = DarkColorIdentifier;
                ColorQualifier          = DarkColorQualifier;
                ColorInstruction        = DarkColorInstruction;
                ColorInstructionSIMD    = DarkColorInstructionSIMD;
                ColorRegister           = DarkColorRegister;
                ColorNumber             = DarkColorNumber;
                ColorString             = DarkColorString;
                ColorComment            = DarkColorComment;
            }
            else
            {
                ColorLineDirective      = LightColorLineDirective;
                ColorDirective          = LightColorDirective;
                ColorIdentifier         = LightColorIdentifier;
                ColorQualifier          = LightColorQualifier;
                ColorInstruction        = LightColorInstruction;
                ColorInstructionSIMD    = LightColorInstructionSIMD;
                ColorRegister           = LightColorRegister;
                ColorNumber             = LightColorNumber;
                ColorString             = LightColorString;
                ColorComment            = LightColorComment;
            }
        }

        private void AlignInstruction(StringBuilder output, int instructionLength, AsmKind asmKind)
        {
            // Only support Intel for now
            if (instructionLength >= InstructionAlignment || asmKind != AsmKind.Intel)
                return;

            output.Append(' ', InstructionAlignment - instructionLength);
        }

        private string ProcessImpl(string input, AsmKind asmKind, bool colourize = true)
        {
            _fileList.Clear();
            _fileName.Clear();
            _tokens.Clear();

            AsmTokenKindProvider asmTokenProvider = default;

            switch (asmKind)
            {
                case AsmKind.Intel:
                    asmTokenProvider = (AsmTokenKindProvider)X86AsmTokenKindProvider.Instance;
                    break;
                case AsmKind.ARM:
                    asmTokenProvider = (AsmTokenKindProvider)ARM64AsmTokenKindProvider.Instance;
                    break;
                case AsmKind.Wasm:
                    asmTokenProvider = (AsmTokenKindProvider)WasmAsmTokenKindProvider.Instance;
                    break;
            }


            var tokenizer = new AsmTokenizer(input, asmKind, asmTokenProvider);

            // Adjust token size
            var pseudoTokenSizeMax = input.Length / 7;
            if (pseudoTokenSizeMax > _tokens.Capacity)
            {
                _tokens.Capacity = pseudoTokenSizeMax;
            }

            // Read all tokens
            while (tokenizer.TryGetNextToken(out var nextToken))
            {
                _tokens.Add(nextToken);
            }

            // Process all tokens
            var output = new StringBuilder();
            for (int i = 0; i < _tokens.Count; i++)
            {
                var token = _tokens[i];
                var slice = token.Slice(input);
                if (token.Kind == AsmTokenKind.Directive && i + 1 < _tokens.Count)
                {
                    if (slice == FileDirective)
                    {
                        // File is followed by an index and a string or just a string with an implied index = 0
                        i++;
                        int index = 0;

                        SkipSpaces(_tokens, ref i);

                        if (i < _tokens.Count && _tokens[i].Kind == AsmTokenKind.Number)
                        {
                            var numberAsStr = _tokens[i].ToString(input);
                            index = int.Parse(numberAsStr);
                            i++;
                        }

                        SkipSpaces(_tokens, ref i);

                        if (i < _tokens.Count && _tokens[i].Kind == AsmTokenKind.String)
                        {
                            var filename = _tokens[i].ToString(input).Trim('"');
                            string[] fileLines;

                            try
                            {
                                fileLines = System.IO.File.ReadAllLines(filename);
                            }
                            catch
                            {
                                fileLines = null;
                            }


                            _fileName.Add(index, filename);
                            _fileList.Add(index, fileLines);
                        }
                        continue;
                    }

                    if (slice == LocDirective)
                    {
                        // .loc {fileno} {lineno} [column] [options] -
                        int fileno = 0;
                        int colno = 0;
                        int lineno = 0;        // NB 0 indicates no information given
                        i++;
                        SkipSpaces(_tokens, ref i);
                        if (i < _tokens.Count && _tokens[i].Kind == AsmTokenKind.Number)
                        {
                            var numberAsStr = _tokens[i].ToString(input);
                            fileno = int.Parse(numberAsStr);
                            i++;
                        }
                        SkipSpaces(_tokens, ref i);
                        if (i < _tokens.Count && _tokens[i].Kind == AsmTokenKind.Number)
                        {
                            var numberAsStr = _tokens[i].ToString(input);
                            lineno = int.Parse(numberAsStr);
                            i++;
                        }
                        SkipSpaces(_tokens, ref i);
                        if (i < _tokens.Count && _tokens[i].Kind == AsmTokenKind.Number)
                        {
                            var numberAsStr = _tokens[i].ToString(input);
                            colno = int.Parse(numberAsStr);
                            i++;
                        }

                        // Skip until end of line
                        for (; i < _tokens.Count; i++)
                        {
                            var tokenToSkip = _tokens[i];
                            if (tokenToSkip.Kind == AsmTokenKind.NewLine)
                            {
                                break;
                            }
                        }

                        // If the file number is 0, skip the line
                        if (fileno == 0)
                        {
                        }
                        // If the line number is 0, then we can update the file tracking, but still not output a line
                        else if (lineno == 0)
                        {
                            if (colourize) output.Append("<color=").Append(ColorLineDirective).Append(">");
                            output.Append("=== ").Append(System.IO.Path.GetFileName(_fileName[fileno]));
                            if (colourize) output.Append("</color>");
                            output.Append("\n");
                        }
                        // We have a source line and number -- can we load file and extract this line?
                        else
                        {
                            if (_fileList.ContainsKey(fileno) && _fileList[fileno] != null && lineno - 1 < _fileList[fileno].Length)
                            {
                                if (colourize) output.Append("<color=").Append(ColorLineDirective).Append(">");
                                output.Append("=== ").Append(System.IO.Path.GetFileName(_fileName[fileno])).Append("(").Append(lineno).Append(", ").Append(colno + 1).Append(")").Append(_fileList[fileno][lineno - 1]);
                                if (colourize) output.Append("</color>");
                                output.Append("\n");
                            }
                            else
                            {
                                if (colourize) output.Append("<color=").Append(ColorLineDirective).Append($">");
                                output.Append("=== ").Append(System.IO.Path.GetFileName(_fileName[fileno])).Append("(").Append(lineno).Append(", ").Append(colno + 1).Append(")");
                                if (colourize) output.Append("</color>");
                                output.Append("\n");
                            }
                        }
                        continue;
                    }
                }

                if (colourize)
                {
                    switch (token.Kind)
                    {
                        case AsmTokenKind.Directive:
                            output.Append("<color=").Append(ColorDirective).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;
                        case AsmTokenKind.Identifier:
                            output.Append("<color=").Append(ColorIdentifier).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;
                        case AsmTokenKind.Qualifier:
                            output.Append("<color=").Append(ColorQualifier).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;
                        case AsmTokenKind.Instruction:
                            output.Append("<color=").Append(ColorInstruction).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            AlignInstruction(output, slice.Length, asmKind);
                            break;
                        case AsmTokenKind.InstructionSIMD:
                            output.Append("<color=").Append(ColorInstructionSIMD).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            AlignInstruction(output, slice.Length, asmKind);
                            break;
                        case AsmTokenKind.Register:
                            output.Append("<color=").Append(ColorRegister).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;
                        case AsmTokenKind.Number:
                            output.Append("<color=").Append(ColorNumber).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;
                        case AsmTokenKind.String:
                            output.Append("<color=").Append(ColorString).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;
                        case AsmTokenKind.Comment:
                            output.Append("<color=").Append(ColorComment).Append(">");
                            output.Append(input, slice.Position, slice.Length);
                            output.Append("</color>");
                            break;

                        default:
                            output.Append(input, slice.Position, slice.Length);
                            break;
                    }
                }
                else
                {
                    output.Append(input, slice.Position, slice.Length);
                }
            }

            return output.ToString();
        }

        private static void SkipSpaces(List<AsmToken> tokens, ref int i)
        {
            for(; i < tokens.Count; i++)
            {
                var kind = tokens[i].Kind;
                if (kind != AsmTokenKind.Misc)
                {
                    return;
                }
            }
        }
    }
}

