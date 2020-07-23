namespace Unity.Burst.CompilerServices
{
#if UNITY_BURST_EXPERIMENTAL_LOOP_INTRINSICS
    public static class Loop
    {
        /// <summary>
        /// Must be called from inside a loop.
        /// Will cause a compiler error in Burst-compiled code if the loop is not auto-vectorized.
        /// </summary>
        public static void ExpectVectorized() { }

        /// <summary>
        /// Must be called from inside a loop.
        /// Will cause a compiler error in Burst-compiled code if the loop is auto-vectorized.
        /// </summary>
        public static void ExpectNotVectorized() { }
    }
#endif
}
