using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.Tools
{
    /// <summary>
    /// This save load method saves and loads files as encrypted binary files
    /// </summary>
    public class MMSaveLoadManagerMethodBinaryEncrypted : MMSaveLoadManagerEncrypter, IMMSaveLoadManagerMethod
    {
        /// <summary>
        /// Saves the specified object to disk at the specified location after encrypting it 
        /// </summary>
        /// <param name="objectToSave"></param>
        /// <param name="saveFile"></param>
        public void Save(object objectToSave, FileStream saveFile)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, objectToSave);
            memoryStream.Position = 0;
            Encrypt(memoryStream, saveFile, Key);
            saveFile.Flush();
            memoryStream.Close();
            saveFile.Close();
        }

        /// <summary>
        /// Loads the specified file from disk, decrypts it, and deserializes it
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                Decrypt(saveFile, memoryStream, Key);
            }
            catch (CryptographicException ce)
            {
                Debug.LogError("[MMSaveLoadManager] Encryption key error: " + ce.Message);
                return null;
            }
            memoryStream.Position = 0;
            savedObject = formatter.Deserialize(memoryStream);
            memoryStream.Close();
            saveFile.Close();
            return savedObject;
        }
    }
}
