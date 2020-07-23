using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Entities.Conversion;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Entities.Tests.Conversion
{
    class ConversionMappingSystemJournalingTests : ConversionTestFixtureBase
    {
        GameObjectConversionSettings m_Settings;
        IJournalDataDebug[] m_Events;

        [SetUp]
        public void SetUp()
        {
            m_Settings = MakeDefaultSettings();
            m_Settings.ConversionWorldPreDispose += world =>
            {
                m_Events = world
                    .GetExistingSystem<GameObjectConversionMappingSystem>()
                    .JournalData
                    .SelectJournalDataDebug()
                    .ToArray();
            };
        }

        [Test]
        public void SingleGameObject_RecordsCreatingDstEntity()
        {
            var go = CreateGameObject();

            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, m_Settings);

            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Any(entity));

            Assert.That(m_Events, Has.Length.EqualTo(1));
            Assert.That(m_Events.EventsOfType<Entity>(), Is.EqualTo(new[]
                { JournalDataDebug.Create(go.GetInstanceID(), entity) }));
        }

        [Test]
        public void ErrorDuringSelfConversion_RecordsError()
        {
            var go = CreateGameObject();
            go.AddComponent<JournalTestAuthoring>().ShouldError = true;

            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(go, m_Settings);

            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Any(entity));

            Assert.That(m_Events.EventsOfType<LogEventData>(), Is.EqualTo(new[]
            {
                JournalDataDebug.Create(go.GetInstanceID(), new LogEventData { Type = LogType.Error, Message = "JournalTestAuthoring.Convert error" })
            }));

            LogAssert.Expect(LogType.Error, "JournalTestAuthoring.Convert error");
        }

        [Test]
        public void DeclareReferencedPrefab_WithNonPrefab_LogsWarning()
        {
            var go = CreateGameObject();
            go.AddComponent<DeclareReferencesTestAuthoring>().Prefab = CreateGameObject();

            GameObjectConversionUtility.ConvertGameObjectHierarchy(go, m_Settings);
            LogAssert.Expect(LogType.Warning, new Regex(".* is not a Prefab"));
        }
    }
}
