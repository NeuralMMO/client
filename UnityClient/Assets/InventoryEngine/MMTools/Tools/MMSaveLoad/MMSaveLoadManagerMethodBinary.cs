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
    /// This save load method saves and loads files as binary files
    /// </summary>
    public class MMSaveLoadManagerMethodBinary : IMMSaveLoadManagerMethod
    {
        /// <summary>
        /// Saves the specified object to disk at the specified location after serializing it
        /// </summary>
        /// <param name="objectToSave"></param>
        /// <param name="saveFile"></param>
        public void Save(object objectToSave, FileStream saveFile)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(saveFile, objectToSave);
            saveFile.Close();
        }

        /// <summary>
        /// Loads the specified file from disk and deserializes it
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        public object Load(System.Type objectType, FileStream saveFile)
        {
            object savedObject;
            BinaryFormatter formatter = new BinaryFormatter();
            savedObject = formatter.Deserialize(saveFile);
            saveFile.Close();
            return savedObject;
        }
    }
}
