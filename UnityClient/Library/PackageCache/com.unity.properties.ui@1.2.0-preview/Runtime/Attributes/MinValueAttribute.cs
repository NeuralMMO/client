namespace Unity.Properties.UI
{
    /// <summary>
    ///   Attribute used to make a numeric field or property restricted to a specific minimum value.
    /// </summary>
    public class MinValueAttribute : InspectorAttribute
    {
        /// <summary>
        ///   The minimum allowed value.
        /// </summary>
        public float Min { get; }

        /// <summary>
        ///   Attribute used to make a float or int variable in a script be restricted to a specific minimum value.
        /// </summary>
        /// <param name="min">The minimum allowed value.</param>
        public MinValueAttribute(float min)
        {
            Min = min;
        }
    }
}