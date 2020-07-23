using System;

namespace Unity.Burst
{
    /// <summary>
    /// Can be used to specify that a parameter to a function, a field of a struct, a struct, or a function return will not alias. (Advanced - see User Manual for a description of Aliasing).
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.ReturnValue)]
    public class NoAliasAttribute : Attribute
    {
    }
}
