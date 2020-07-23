namespace Unity.Burst.Editor
{
    internal partial class BurstDisassembler
    {
        private class WasmAsmTokenKindProvider : AsmTokenKindProvider
        {
            private static readonly string[] Registers = new[]
            {
                "memory.",     // add . to avoid parsing instruction portion as directive
                "local.",
                "global.",
                "i32.",
                "i64.",
                "f32.",
                "f64."
            };

            private static readonly string[] Qualifiers = new[]
            {
                "offset",
                "align",

                "eqz",
                "eq",
                "ne",
                "lt_s",
                "lt_u",
                "gt_s",
                "gt_u",
                "le_s",
                "le_u",
                "ge_s",
                "ge_u",
                "lt",
                "gt",
                "le",
                "ge",
            };

            private static readonly string[] Instructions = new[]
            {
                "if",
                "end",
                "block",
                "end_block",
                "loop",
                "unreachable",
                "nop",
                "br",
                "br_if",
                "br_table",
                "return",
                "call",
                "call_indirect",

                "drop",
                "select",
                "get",
                "set",
                "tee",

                "load",
                "load8_s",
                "load8_u",
                "load16_s",
                "load16_u",
                "load32_s",
                "load32_u",
                "store",
                "store8",
                "store16",
                "store32",
                "size",
                "grow",

                "const",
                "clz",
                "ctz",
                "popcnt",
                "add",
                "sub",
                "mul",
                "div_s",
                "div_u",
                "rem_s",
                "rem_u",
                "and",
                "or",
                "xor",
                "shl",
                "shr_s",
                "shr_u",
                "rotl",
                "rotr",
                "abs",
                "neg",
                "ceil",
                "floor",
                "trunc",
                "sqrt",
                "div",
                "min",
                "max",
                "copysign",

                "wrap_i64",
                "trunc_f32_s",
                "trunc_f32_u",
                "trunc_f64_s",
                "trunc_f64_u",
                "extend_i32_s",
                "extend_i64_u",
                "convert_f32_s",
                "convert_f32_u",
                "convert_f64_s",
                "convert_f64_u",
                "demote_f64",
                "promote_f32",
                "reinterpret_f32",
                "reinterpret_f64",
                "reinterpret_i32",
                "reinterpret_i64",
            };

            private static readonly string[] SimdInstructions = new string[]
            {
            };

            private WasmAsmTokenKindProvider() : base(Registers.Length + Qualifiers.Length + Instructions.Length + SimdInstructions.Length)
            {
                foreach (var register in Registers)
                {
                    AddTokenKind(register, AsmTokenKind.Register);
                }

                foreach (var instruction in Qualifiers)
                {
                    AddTokenKind(instruction, AsmTokenKind.Qualifier);
                }

                foreach (var instruction in Instructions)
                {
                    AddTokenKind(instruction, AsmTokenKind.Instruction);
                }

                foreach (var instruction in SimdInstructions)
                {
                    AddTokenKind(instruction, AsmTokenKind.InstructionSIMD);
                }
            }

            public override bool AcceptsCharAsIdentifierOrRegisterEnd(char c)
            {
                return c == '.';
            }


            public static readonly WasmAsmTokenKindProvider Instance = new WasmAsmTokenKindProvider();
        }
    }
}

