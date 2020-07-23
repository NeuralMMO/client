using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
namespace Unity.Entities
{
#if ENABLE_UNITY_COLLECTIONS_CHECKS
    unsafe static class SystemDependencySafetyUtility
    {
        public struct SafetyErrorDetails
        {
            internal int m_ProblematicTypeIndex;
            internal int m_ReaderIndex;
            internal AtomicSafetyHandle m_ProblematicHandle;
            internal bool IsWrite => m_ReaderIndex == -1;

            internal string FormatToString(Type systemType)
            {
                int type = m_ProblematicTypeIndex;
                AtomicSafetyHandle h = m_ProblematicHandle;

                if (!IsWrite)
                {
                    int i = m_ReaderIndex;
                    if (typeof(JobComponentSystem).IsAssignableFrom(systemType))
                        return
                            $"The system {systemType} reads {TypeManager.GetType(type)} via {AtomicSafetyHandle.GetReaderName(h, i)} but that type was not returned as a job dependency. To ensure correct behavior of other systems, the job or a dependency of it must be returned from the OnUpdate method.";
                    else
                        return
                            $"The system {systemType} reads {TypeManager.GetType(type)} via {AtomicSafetyHandle.GetReaderName(h, i)} but that type was not assigned to the Dependency property. To ensure correct behavior of other systems, the job or a dependency must be assigned to the Dependency property before returning from the OnUpdate method.";
                }
                else
                {
                    if (typeof(JobComponentSystem).IsAssignableFrom(systemType))
                        return $"The system {systemType} writes {TypeManager.GetType(type)} via {AtomicSafetyHandle.GetWriterName(h)} but that was not returned as a job dependency. To ensure correct behavior of other systems, the job or a dependency of it must be returned from the OnUpdate method.";
                    else
                        return $"The system {systemType} writes {TypeManager.GetType(type)} via {AtomicSafetyHandle.GetWriterName(h)} but that type was not assigned to the Dependency property. To ensure correct behavior of other systems, the job or a dependency must be assigned to the Dependency property before returning from the OnUpdate method.";
                }
            }

            internal string FormatToString(FixedString64 systemTypeName)
            {
                int type = m_ProblematicTypeIndex;
                AtomicSafetyHandle h = m_ProblematicHandle;

                if (!IsWrite)
                {
                    int i = m_ReaderIndex;
                    return $"The system {systemTypeName} reads {TypeManager.GetType(type)} via {AtomicSafetyHandle.GetReaderName(h, i)} but that type was not assigned to the Dependency property. To ensure correct behavior of other systems, the job or a dependency must be assigned to the Dependency property before returning from the OnUpdate method.";
                }
                else
                {
                    return $"The system {systemTypeName} writes {TypeManager.GetType(type)} via {AtomicSafetyHandle.GetWriterName(h)} but that type was not assigned to the Dependency property. To ensure correct behavior of other systems, the job or a dependency must be assigned to the Dependency property before returning from the OnUpdate method.";
                }
            }
        }

        internal static bool CheckSafetyAfterUpdate(ref UnsafeIntList readingSystems, ref UnsafeIntList writingSystems,
            ComponentDependencyManager* dependencyManager, out SafetyErrorDetails details)
        {
            details = default;

            // Check that all reading and writing jobs are a dependency of the output job, to
            // catch systems that forget to add one of their jobs to the dependency graph.
            //
            // Note that this check is not strictly needed as we would catch the mistake anyway later,
            // but checking it here means we can flag the system that has the mistake, rather than some
            // other (innocent) system that is doing things correctly.

            //@TODO: It is not ideal that we call m_SafetyManager.GetDependency,
            //       it can result in JobHandle.CombineDependencies calls.
            //       Which seems like debug code might have side-effects

            for (var index = 0; index < readingSystems.Length; index++)
            {
                var type = readingSystems.Ptr[index];
                if (CheckJobDependencies(ref details, type, dependencyManager))
                    return true;
            }

            for (var index = 0; index < writingSystems.Length; index++)
            {
                var type = writingSystems.Ptr[index];
                if (CheckJobDependencies(ref details, type, dependencyManager))
                    return true;
            }

// EmergencySyncAllJobs(ref readingSystems, ref writingSystems, dependencyManager);

            return false;
        }

        static bool CheckJobDependencies(ref SafetyErrorDetails details, int type, ComponentDependencyManager* dependencyManager)
        {
            var h = dependencyManager->Safety.GetSafetyHandle(type, true);

            var readerCount = AtomicSafetyHandle.GetReaderArray(h, 0, IntPtr.Zero);
            JobHandle* readers = stackalloc JobHandle[readerCount];

            AtomicSafetyHandle.GetReaderArray(h, readerCount, (IntPtr)readers);

            for (var i = 0; i < readerCount; ++i)
            {
                if (!dependencyManager->HasReaderOrWriterDependency(type, readers[i]))
                {
                    details.m_ProblematicTypeIndex = type;
                    details.m_ProblematicHandle = h;
                    details.m_ReaderIndex = i;
                    return true;
                }
            }

            if (!dependencyManager->HasReaderOrWriterDependency(type, AtomicSafetyHandle.GetWriter(h)))
            {
                details.m_ProblematicTypeIndex = type;
                details.m_ProblematicHandle = h;
                details.m_ReaderIndex = -1;
                return true;
            }

            return false;
        }

        internal static void EmergencySyncAllJobs(ref UnsafeIntList readingSystems, ref UnsafeIntList writingSystems, ComponentDependencyManager* dependencyManager)
        {
            for (int i = 0; i != readingSystems.Length; i++)
            {
                int type = readingSystems.Ptr[i];
                AtomicSafetyHandle.EnforceAllBufferJobsHaveCompleted(dependencyManager->Safety.GetSafetyHandle(type, true));
            }

            for (int i = 0; i != writingSystems.Length; i++)
            {
                int type = writingSystems.Ptr[i];
                AtomicSafetyHandle.EnforceAllBufferJobsHaveCompleted(dependencyManager->Safety.GetSafetyHandle(type, true));
            }
        }
    }
#else
    unsafe static class SystemDependencySafetyUtility
    {
        public struct SafetyErrorDetails
        {
        }
    }
#endif
}
