using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MoreMountains.Tools
{
    /// <summary>
    /// An interface to implement save and load using different methods (binary, json, etc)
    /// </summary>
    public interface IMMSaveLoadManagerMethod
    {
        void Save(object objectToSave, FileStream saveFile);
        object Load(System.Type objectType, FileStream saveFile);
    }

    /// <summary>
    /// The possible methods to save and load files to and from disk available in the MMSaveLoadManager
    /// </summary>
    public enum MMSaveLoadManagerMethods { Json, JsonEncrypted, Binary, BinaryEncrypted };

    /// <summary>
    /// This class implements methods to encrypt and decrypt streams
    /// </summary>
    public abstract class MMSaveLoadManagerEncrypter
    {
        /// <summary>
        /// The Key to use to save and load the file
        /// </summary>
        public string Key { get; set; } = "yourDefaultKey";

        protected string _saltText = "SaltTextGoesHere";

        /// <summary>
        /// Encrypts the specified input stream into the specified output stream using the key passed in parameters
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="sKey"></param>
        protected virtual void Encrypt(Stream inputStream, Stream outputStream, string sKey)
        {
            RijndaelManaged algorithm = new RijndaelManaged();
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sKey, Encoding.ASCII.GetBytes(_saltText));

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            var cryptostream = new CryptoStream(inputStream, algorithm.CreateEncryptor(), CryptoStreamMode.Read);
            cryptostream.CopyTo(outputStream);
        }

        /// <summary>
        /// Decrypts the input stream into the output stream using the key passed in parameters
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="sKey"></param>
        protected virtual void Decrypt(Stream inputStream, Stream outputStream, string sKey)
        {
            RijndaelManaged algorithm = new RijndaelManaged();
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sKey, Encoding.ASCII.GetBytes(_saltText));

            algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

            var cryptostream = new CryptoStream(inputStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read);
            cryptostream.CopyTo(outputStream);
        }
    }
}
