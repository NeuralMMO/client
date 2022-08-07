using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MoreMountains.Tools
{
    public class MMSaveLoadManagerMethodJsonEncrypted : MMSaveLoadManagerEncrypter, IMMSaveLoadManagerMethod
    {
        /// <summary>
        /// Saves the specified object at the specified location to disk, converts it to json and encrypts it
        /// </summary>
        /// <param name="objectToSave"></param>
        /// <param name="saveFile"></param>
        public void Save(object objectToSave, FileStream saveFile)
        {
            string json = JsonUtility.ToJson(objectToSave);
            // if you prefer using NewtonSoft's JSON lib uncomment the line below and commment the line above
            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(objectToSave);
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter streamWriter = new StreamWriter(memoryStream))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                memoryStream.Position = 0;
                Encrypt(memoryStream, saveFile, Key);
            }
            saveFile.Close();
        }

        /// <summary>
        /// Loads the specified file, decrypts it and decodes it
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject = null;
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader streamReader = new StreamReader(memoryStream))
            {
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
                savedObject = JsonUtility.FromJson(streamReader.ReadToEnd(), objectType);
                // if you prefer using NewtonSoft's JSON lib uncomment the line below and commment the line above
                //savedObject = Newtonsoft.Json.JsonConvert.DeserializeObject(sr.ReadToEnd(), objectType); 
            }
            saveFile.Close();
            return savedObject;
        }

    }
}
