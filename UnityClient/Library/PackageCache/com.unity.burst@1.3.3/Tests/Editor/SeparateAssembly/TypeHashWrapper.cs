using Unity.Burst;

public static class TypeHashWrapper
{
    public static int GetIntHash()
    {
        return BurstRuntime.GetHashCode32<int>();
    }

    public static int GetGenericHash<T>()
    {
        return BurstRuntime.GetHashCode32<SomeStruct<T>>();
    }

    public struct SomeStruct<T> { }
}