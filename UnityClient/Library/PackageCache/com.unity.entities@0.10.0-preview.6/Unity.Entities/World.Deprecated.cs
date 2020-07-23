using System;
using System.Reflection;

namespace Unity.Entities
{
    public partial class World
    {
#if !UNITY_DOTSPLAYER
        [Obsolete("To construct systems with constructor parameters please use World.AddSystem(new MySystem(myParams)); instead. (RemovedAfter 2020-06-17)")]
        public T CreateSystem<T>(params object[] constructorArguments) where T : ComponentSystemBase
        {
            return (T)CreateSystem(typeof(T), constructorArguments);
        }

        [Obsolete("To construct systems with constructor parameters please use World.AddSystem(new MySystem(myParams)); instead. (RemovedAfter 2020-06-17)")]
        public ComponentSystemBase CreateSystem(Type type, params object[] constructorArguments)
        {
            CheckGetOrCreateSystem();

            if (!typeof(ComponentSystemBase).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {type} must be derived from ComponentSystem or JobComponentSystem.");
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS

            if (constructorArguments != null && constructorArguments.Length != 0)
            {
                var constructors =
                    type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (constructors.Length == 1 && constructors[0].IsPrivate)
                    throw new MissingMethodException(
                        $"Constructing {type} failed because the constructor was private, it must be public.");
            }

            m_AllowGetSystem = false;
#endif
            ComponentSystemBase system;
            try
            {
                system = Activator.CreateInstance(type, constructorArguments) as ComponentSystemBase;
            }
            catch (MissingMethodException)
            {
                throw new MissingMethodException($"Constructing {type} failed because CreateSystem " +
                    $"parameters did not match its constructor.  [Job]ComponentSystem {type} must " +
                    "be mentioned in a link.xml file, or annotated with a [Preserve] attribute to " +
                    "prevent its constructor from being stripped.  See " +
                    "https://docs.unity3d.com/Manual/ManagedCodeStripping.html for more information.");
            }
            finally
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                m_AllowGetSystem = true;
#endif
            }
            return AddSystem(system);
        }

#endif
    }
}
