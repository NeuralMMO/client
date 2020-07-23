using Burst.Compiler.IL.Tests.Helpers;
using Unity.Mathematics;

namespace Burst.Compiler.IL.Tests
{
    internal partial class VectorsBools
    {
        // ---------------------------------------------------
        // ! operator
        // ---------------------------------------------------

        [TestCompiler(DataRange.Standard)]
        public static int Bool4Not(ref bool4 a)
        {
            return Vectors.ConvertToInt(!a);
        }

        [TestCompiler(DataRange.Standard)]
        public static int Bool3Not(ref bool3 a)
        {
            return Vectors.ConvertToInt(!a);
        }

        [TestCompiler(DataRange.Standard)]
        public static int Bool2Not(ref bool2 a)
        {
            return Vectors.ConvertToInt(!a);
        }
    }
}