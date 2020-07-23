using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

internal class NativeMultiHashMapTestsFixture
{
    protected const int hashMapSize = 10 * 1024;

    [BurstCompile(CompileSynchronously = true)]
    public struct MultiHashMapSimpleWriteJob : IJob
    {
        public NativeMultiHashMap<int, int>.ParallelWriter hashMap;

        public void Execute()
        {
            hashMap.Add(0, 0);
        }
    }

    // Burst error BC1005: The `try` construction is not supported
    // [BurstCompile(CompileSynchronously = true)]
    public struct MultiHashMapWriteParallelForJob : IJobParallelFor
    {
        public NativeMultiHashMap<int, int>.ParallelWriter hashMap;
        public NativeArray<int> status;

        public int keyMod;

        public void Execute(int i)
        {
            status[i] = 0;
            try
            {
                hashMap.Add(i % keyMod, i);
            }
            catch (System.InvalidOperationException)
            {
                status[i] = -2;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct MultiHashMapReadParallelForJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeMultiHashMap<int, int> hashMap;
        public NativeArray<int> values;

        public int keyMod;
        public void Execute(int i)
        {
            int iSquared;
            values[i] = -1;
            NativeMultiHashMapIterator<int> it;
            if (hashMap.TryGetFirstValue(i % keyMod, out iSquared, out it))
            {
                int count = 0;
                do
                {
                    if (iSquared % keyMod != i % keyMod)
                    {
                        values[i] = -2;
                        return;
                    }
                    ++count;
                }
                while (hashMap.TryGetNextValue(out iSquared, ref it));
                values[i] = count;
            }
        }
    }
}
