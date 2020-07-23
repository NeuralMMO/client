using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Unity.Entities.Editor.Tests
{
    class DirectoryInfoExtensionsTests
    {
        DirectoryInfo m_RootTestDirectory;
        DirectoryInfo m_RootTestEditorDirectory;
        readonly List<DirectoryInfo> m_DirectoriesToDelete = new List<DirectoryInfo>();
        const string m_TestFileName = "NewFile";
        const string m_TestFileNameExtension = ".cs";
        string m_TestFileFullName;
        const string m_TestFileEditorDirectory = "TestFileEditorDirectory.cs";

        [OneTimeSetUp]
        public void SetUp()
        {
            m_TestFileFullName = m_TestFileName + m_TestFileNameExtension;
            string appPathFullName = new DirectoryInfo(Application.dataPath).Parent.FullName;
            m_RootTestEditorDirectory = new DirectoryInfo(appPathFullName + "/Assets/Editor");
            m_RootTestDirectory = new DirectoryInfo(appPathFullName + "/Assets/Editor/Extensions");
            m_RootTestDirectory.Create();
            StreamWriter sr = File.CreateText(m_RootTestDirectory.FullName + "/NewFile.cs");
            sr.Close();
            sr = File.CreateText(m_RootTestEditorDirectory.FullName + "/TestFileEditorDirectory.cs");
            sr.Close();
            m_DirectoriesToDelete.Add(m_RootTestDirectory);
            m_DirectoriesToDelete.Add(m_RootTestEditorDirectory);
            Assert.That(m_RootTestDirectory.Exists, Is.True);
            Assert.That(m_RootTestDirectory.GetFile(m_TestFileFullName).Exists, Is.True);
            Assert.That(m_RootTestEditorDirectory.GetFile(m_TestFileEditorDirectory).Exists, Is.True);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            foreach (var directoryInfo in m_DirectoriesToDelete)
            {
                if (null == directoryInfo || !directoryInfo.Exists)
                {
                    continue;
                }

                directoryInfo.Delete(true);
            }
        }

        [Test]
        public void CanCombineDirectoryInfoFromNames()
        {
            var directory = new DirectoryInfo(Application.dataPath).Parent.Combine("Assets", "Editor", "Extensions");
            Assert.That(directory.Exists, Is.True);
            Assert.That(directory, Is.EqualTo(m_RootTestDirectory));
        }

        [Test]
        public void CanGetFileOutOfADirectoryFromName()
        {
            // Testing on self with and without extension on the file
            Assert.That(m_RootTestDirectory.GetFile(m_TestFileFullName).Exists, Is.True);
            Assert.That(m_RootTestDirectory.GetFile(m_TestFileName).Exists, Is.False);

            // Wrong extensions
            Assert.That(m_RootTestDirectory.GetFile(m_TestFileName + ".wrongextension").Exists, Is.False);

            // File on the same level as the directory
            Assert.That(m_RootTestDirectory.GetFile(m_TestFileEditorDirectory).Exists, Is.False);

            // File that doesn't exist.
            Assert.That(m_RootTestDirectory.GetFile("Blerg").Exists, Is.False);
        }

        [Test]
        public void CanGetFileOutOfADirectoryFromFieldInfo()
        {
            var rootDirectory = m_RootTestDirectory.FullName;

            // Testing on self with and without extension on the file
            Assert.That(m_RootTestDirectory.GetFile(new FileInfo(Path.Combine(rootDirectory, m_TestFileFullName))).Exists, Is.True);
            Assert.That(m_RootTestDirectory.GetFile(new FileInfo(Path.Combine(rootDirectory, m_TestFileName))).Exists, Is.False);

            // Wrong extensions
            Assert.That(m_RootTestDirectory.GetFile(new FileInfo(Path.Combine(rootDirectory, m_TestFileName + ".wrongextension"))).Exists, Is.False);

            // File on the same level as the directory
            Assert.That(m_RootTestDirectory.GetFile(new FileInfo(Path.Combine(rootDirectory, m_TestFileEditorDirectory))).Exists, Is.False);

            // File that doesn't exist.
            Assert.That(m_RootTestDirectory.GetFile(new FileInfo(Path.Combine(rootDirectory, "Blerg"))).Exists, Is.False);
        }

        [Test]
        public void CanGetHyperlinkFromDirectory()
        {
            const string key = "MyKey";
            var value = m_RootTestDirectory.FullName;
            Assert.That(m_RootTestDirectory.ToHyperLink(key), Is.EqualTo($"<a {key}={value.DoubleQuoted()}>{value}</a>"));
            Assert.That(m_RootTestDirectory.ToHyperLink(string.Empty), Is.EqualTo($"<a>{value}</a>"));
            Assert.That(m_RootTestDirectory.ToHyperLink(null), Is.EqualTo($"<a>{value}</a>"));
        }

        [Test]
        public void CanCopyDirectoryContentToAnotherDirectory()
        {
            CopyToDirectory(false);
        }

        [Test]
        public void CanRecursivelyCopyDirectoryContentToAnotherDirectory()
        {
            CopyToDirectory(true);
        }

        void CopyToDirectory(bool recurse)
        {
            var subDirectory = m_RootTestDirectory.CreateSubdirectory("Test");
            m_DirectoriesToDelete.Add(subDirectory);
            var copyToDirectory = m_RootTestDirectory.Parent.CreateSubdirectory("ExtensionsTemp");
            m_DirectoriesToDelete.Add(copyToDirectory);
            m_RootTestDirectory.CopyTo(copyToDirectory, recurse);

            Assert.That(copyToDirectory.GetFile(m_TestFileFullName).Exists, Is.True);
            Assert.That(copyToDirectory.GetDirectories("Test").Length, Is.EqualTo(recurse ? 1 : 0));
        }
    }
}
