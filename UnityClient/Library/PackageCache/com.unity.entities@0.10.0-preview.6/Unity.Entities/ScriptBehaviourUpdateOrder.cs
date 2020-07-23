using System;
#if !UNITY_DOTSPLAYER
using System.Collections.Generic;
using System.Linq;

#if  UNITY_2019_3_OR_NEWER
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#else
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Experimental.PlayerLoop;
#endif
#endif

namespace Unity.Entities
{
    // Updating before or after a system constrains the scheduler ordering of these systems within a ComponentSystemGroup.
    // Both the before & after system must be a members of the same ComponentSystemGroup.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UpdateBeforeAttribute : Attribute
    {
        public UpdateBeforeAttribute(Type systemType)
        {
            if (systemType == null)
                throw new ArgumentNullException(nameof(systemType));

            SystemType = systemType;
        }

        public Type SystemType { get; }
    }

    // Updating before or after a system constrains the scheduler ordering of these systems within a ComponentSystemGroup.
    // Both the before & after system must be a members of the same ComponentSystemGroup.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UpdateAfterAttribute : Attribute
    {
        public UpdateAfterAttribute(Type systemType)
        {
            if (systemType == null)
                throw new ArgumentNullException(nameof(systemType));

            SystemType = systemType;
        }

        public Type SystemType { get; }
    }

    // The specified Type must be a ComponentSystemGroup.
    // Updating in a group means this system will be automatically updated by the specified ComponentSystemGroup.
    // The system may order itself relative to other systems in the group with UpdateBegin and UpdateEnd,
    // There is nothing preventing systems from being in multiple groups, it can be added if there is a use-case for it
    [AttributeUsage(AttributeTargets.Class)]
    public class UpdateInGroupAttribute : Attribute
    {
        public UpdateInGroupAttribute(Type groupType)
        {
            if (groupType == null)
                throw new ArgumentNullException(nameof(groupType));

            GroupType = groupType;
        }

        public Type GroupType { get; }
    }

#if !UNITY_DOTSPLAYER
    public static class ScriptBehaviourUpdateOrder
    {
        private static void InsertManagerIntoSubsystemList<T>(PlayerLoopSystem[] subsystemList, int insertIndex, T mgr)
            where T : ComponentSystemBase
        {
            var del = new DummyDelegateWrapper(mgr);
            subsystemList[insertIndex].type = typeof(T);
            subsystemList[insertIndex].updateDelegate = del.TriggerUpdate;
        }

        /// <summary>
        /// Update the player loop with a world's root-level systems
        /// </summary>
        /// <param name="world">World with root-level systems that need insertion into the player loop</param>
        /// <param name="existingPlayerLoop">Optional parameter to preserve existing player loops (e.g. PlayerLoop.GetCurrentPlayerLoop())</param>
        public static void UpdatePlayerLoop(World world, PlayerLoopSystem? existingPlayerLoop = null)
        {
            var playerLoop = existingPlayerLoop ?? PlayerLoop.GetDefaultPlayerLoop();

            if (world != null)
            {
                // Insert the root-level systems into the appropriate PlayerLoopSystem subsystems:
                for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
                {
                    int subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;
                    if (playerLoop.subSystemList[i].type == typeof(Update))
                    {
                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; ++j)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        InsertManagerIntoSubsystemList(newSubsystemList,
                            subsystemListLength + 0, world.GetOrCreateSystem<SimulationSystemGroup>());
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;
                    }
                    else if (playerLoop.subSystemList[i].type == typeof(PreLateUpdate))
                    {
                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; ++j)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        InsertManagerIntoSubsystemList(newSubsystemList,
                            subsystemListLength + 0, world.GetOrCreateSystem<PresentationSystemGroup>());
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;
                    }
                    else if (playerLoop.subSystemList[i].type == typeof(Initialization))
                    {
                        var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                        for (var j = 0; j < subsystemListLength; ++j)
                            newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[j];
                        InsertManagerIntoSubsystemList(newSubsystemList,
                            subsystemListLength + 0, world.GetOrCreateSystem<InitializationSystemGroup>());
                        playerLoop.subSystemList[i].subSystemList = newSubsystemList;
                    }
                }
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static bool IsWorldInSubSystemList(World world, PlayerLoopSystem[] subSystemList)
        {
            foreach (var subSystem in subSystemList)
            {
                var type = subSystem.type;
                if (type == typeof(SimulationSystemGroup) || type == typeof(PresentationSystemGroup) || type == typeof(InitializationSystemGroup))
                {
                    var wrapper = subSystem.updateDelegate.Target as DummyDelegateWrapper;

                    if (wrapper.System.World == world)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsWorldInPlayerLoop(World world)
        {
            if (world == null)
                return false;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (playerLoop.subSystemList[i].type == typeof(Update))
                {
                    if (!IsWorldInSubSystemList(world, playerLoop.subSystemList[i].subSystemList))
                        return false;
                }
                else if (playerLoop.subSystemList[i].type == typeof(PreLateUpdate))
                {
                    if (!IsWorldInSubSystemList(world, playerLoop.subSystemList[i].subSystemList))
                        return false;
                }
                else if (playerLoop.subSystemList[i].type == typeof(Initialization))
                {
                    if (!IsWorldInSubSystemList(world, playerLoop.subSystemList[i].subSystemList))
                        return false;
                }
            }

            return true;
        }

        [Obsolete("Please use PlayerLoop.GetCurrentPlayerLoop(). (RemovedAfter 2020-05-12)")]
        public static PlayerLoopSystem CurrentPlayerLoop => PlayerLoop.GetCurrentPlayerLoop();

        [Obsolete("Please use PlayerLoop.SetPlayerLoop(). (RemovedAfter 2020-05-12)")]
        public static void SetPlayerLoop(PlayerLoopSystem playerLoop)
        {
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        // FIXME: HACK! - mono 4.6 has problems invoking virtual methods as delegates from native, so wrap the invocation in a non-virtual class
        internal class DummyDelegateWrapper
        {
            internal ComponentSystemBase System => m_System;
            private readonly ComponentSystemBase m_System;

            public DummyDelegateWrapper(ComponentSystemBase sys)
            {
                m_System = sys;
            }

            public void TriggerUpdate()
            {
                m_System.Update();
            }
        }
    }
#endif
}
