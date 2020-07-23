#if UNITY_ANDROID
using UnityEngine.Networking;
#else
using System.IO;
#endif

namespace Unity.Entities.Hybrid
{
    internal static unsafe class FileUtilityHybrid
    {
        public static bool FileExists(string path)
        {
            // This shows an error from the Engine code (not exception) if the file doesn't exist.
            // So for now it is not suitable for checking a file exists. Bring this back when it is, or change to a new
            // api for checking file exists on VFS. So for now we have to have a horrible ifdef
            /*
            var readHandle = AsyncReadManager.Read(path, null, 0);
            readHandle.JobHandle.Complete();
            if (readHandle.Status == ReadStatus.Failed)
                return false;

            return true;
            */

#if UNITY_ANDROID
            var uwrFile = new UnityWebRequest(path);
            uwrFile.SendWebRequest();
            while (!uwrFile.isDone) {}

            return !uwrFile.isNetworkError && !uwrFile.isHttpError;
#else
            return File.Exists(path);
#endif
        }
    }
}
