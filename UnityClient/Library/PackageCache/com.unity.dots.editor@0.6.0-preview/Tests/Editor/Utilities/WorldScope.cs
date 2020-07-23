using System;

namespace Unity.Entities.Editor.Tests
{
    struct WorldScope : IDisposable
    {
        World[] m_CapturedWorlds;

        public static WorldScope CaptureAndResetExistingWorlds()
        {
            var scope = new WorldScope { m_CapturedWorlds = World.s_AllWorlds.ToArray() };
            World.s_AllWorlds.Clear();
            return scope;
        }

        public void Dispose()
        {
            while (World.s_AllWorlds.Count != 0)
            {
                // If not already disposed by a test
                if (World.s_AllWorlds[0].IsCreated)
                    World.s_AllWorlds[0].Dispose();
            }

            World.s_AllWorlds.Clear();
            World.s_AllWorlds.AddRange(m_CapturedWorlds);
        }
    }
}
