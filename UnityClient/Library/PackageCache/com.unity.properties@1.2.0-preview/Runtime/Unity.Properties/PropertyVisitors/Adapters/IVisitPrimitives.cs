namespace Unity.Properties.Adapters
{
    /// <summary>
    /// Implement this interface to intercept the visitation of any primitive type.
    /// </summary>
    public interface IVisitPrimitives :
        IVisit<sbyte>,
        IVisit<short>,
        IVisit<int>,
        IVisit<long>,
        IVisit<byte>,
        IVisit<ushort>,
        IVisit<uint>,
        IVisit<ulong>,
        IVisit<float>,
        IVisit<double>,
        IVisit<bool>,
        IVisit<char>
    {
    }
}