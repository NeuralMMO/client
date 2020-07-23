namespace Unity.Build
{
    /// <summary>
    /// Struct to contain a boolean result with contextual information when false.
    /// </summary>
    public struct BoolResult
    {
        /// <summary>
        /// Value of this result.
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// Contextual information about this result.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Construct a result equal to <see langword="true"/>.
        /// </summary>
        /// <returns>A new result.</returns>
        public static BoolResult True() => new BoolResult { Result = true, Reason = null };

        /// <summary>
        /// Construct a result equal to <see langword="false"/>, with a reason.
        /// </summary>
        /// <param name="reason">The reason why it is <see langword="false"/>.</param>
        /// <returns>A new result.</returns>
        public static BoolResult False(string reason) => new BoolResult { Result = false, Reason = reason };

        /// <summary>
        /// Implicit conversion to <see cref="bool"/>.
        /// </summary>
        /// <param name="canBuild">The result.</param>
        public static implicit operator bool(BoolResult canBuild) => canBuild.Result;
    }
}
