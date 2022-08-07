using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A test object to store data to test the MMSaveLoadManager class
    /// </summary>
    [System.Serializable]
    public class MMSaveLoadTestObject
    {
        public List<string> StringList;
    }

    /// <summary>
    /// A simple class used in the MMSaveLoadTestScene to test the MMSaveLoadManager class
    /// </summary>
    public class MMSaveLoadTester : MonoBehaviour
    {
        [Header("Saved object")]
        /// a test object containing a list of strings to save and load
        public MMSaveLoadTestObject TestObject;

        [Header("Save settings")]
        /// the chosen save method (json, encrypted json, binary, encrypted binary)
        public MMSaveLoadManagerMethods SaveLoadMethod = MMSaveLoadManagerMethods.Binary;
        /// the name of the file to save
        public string FileName = "TestObject";
        /// the name of the destination folder
        public string FolderName = "MMTest/";
        /// the extension to use
        public string SaveFileExtension = ".testObject";
        /// the key to use to encrypt the file (if needed)
        public string EncryptionKey = "ThisIsTheKey";

        /// Test button
        [MMInspectorButton("Save")]
        public bool TestSaveButton;
        /// Test button
        [MMInspectorButton("Load")]
        public bool TestLoadButton;
        /// Test button
        [MMInspectorButton("Reset")]
        public bool TestResetButton;

        protected IMMSaveLoadManagerMethod _saveLoadManagerMethod;

        /// <summary>
        /// Saves the contents of the TestObject into a file
        /// </summary>
        protected virtual void Save()
        {
            InitializeSaveLoadMethod();
            MMSaveLoadManager.Save(TestObject, FileName+SaveFileExtension, FolderName);
        }

        /// <summary>
        /// Loads the saved data
        /// </summary>
        protected virtual void Load()
        {
            InitializeSaveLoadMethod();
            TestObject = (MMSaveLoadTestObject)MMSaveLoadManager.Load(typeof(MMSaveLoadTestObject), FileName + SaveFileExtension, FolderName);
        }

        /// <summary>
        /// Resets all saves by deleting the whole folder
        /// </summary>
        protected virtual void Reset()
        {
            MMSaveLoadManager.DeleteSaveFolder(FolderName);
        }

        /// <summary>
        /// Creates a new MMSaveLoadManagerMethod and passes it to the MMSaveLoadManager
        /// </summary>
        protected virtual void InitializeSaveLoadMethod()
        {
            switch(SaveLoadMethod)
            {
                case MMSaveLoadManagerMethods.Binary:
                    _saveLoadManagerMethod = new MMSaveLoadManagerMethodBinary();
                    break;
                case MMSaveLoadManagerMethods.BinaryEncrypted:
                    _saveLoadManagerMethod = new MMSaveLoadManagerMethodBinaryEncrypted();
                    (_saveLoadManagerMethod as MMSaveLoadManagerEncrypter).Key = EncryptionKey;
                    break;
                case MMSaveLoadManagerMethods.Json:
                    _saveLoadManagerMethod = new MMSaveLoadManagerMethodJson();
                    break;
                case MMSaveLoadManagerMethods.JsonEncrypted:
                    _saveLoadManagerMethod = new MMSaveLoadManagerMethodJsonEncrypted();
                    (_saveLoadManagerMethod as MMSaveLoadManagerEncrypter).Key = EncryptionKey;
                    break;
            }
            MMSaveLoadManager.saveLoadMethod = _saveLoadManagerMethod;
        }
    }
}
