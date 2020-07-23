using System.IO;

namespace Unity.Serialization.Json.Adapters
{
    partial class JsonAdapter :
        IJsonAdapter<DirectoryInfo>,
        IJsonAdapter<FileInfo>
    {
        void IJsonAdapter<DirectoryInfo>.Serialize(JsonStringBuffer writer, DirectoryInfo value)
        {
            if (null == value) 
                writer.Write("null");
            else 
                writer.WriteEncodedJsonString(value.GetRelativePath());
        }

        DirectoryInfo IJsonAdapter<DirectoryInfo>.Deserialize(SerializedValueView view)
        {
            return view.AsStringView().Equals("null") ? null : new DirectoryInfo(view.ToString());
        }

        void IJsonAdapter<FileInfo>.Serialize(JsonStringBuffer writer, FileInfo value)
        {
            if (null == value) 
                writer.Write("null");
            else 
                writer.WriteEncodedJsonString(value.GetRelativePath());
        }

        FileInfo IJsonAdapter<FileInfo>.Deserialize(SerializedValueView view)
        {
            return view.AsStringView().Equals("null") ? null : new FileInfo(view.ToString());
        }
    }

    static class DirectoryInfoExtensions
    {
        internal static string GetRelativePath(this DirectoryInfo directoryInfo)
        {
            var relativePath = new DirectoryInfo(".").FullName.ToForwardSlash();
            var path = directoryInfo.FullName.ToForwardSlash();
            return path.StartsWith(relativePath) ? path.Substring(relativePath.Length).TrimStart('/') : path;
        }
    }

    static class FileInfoExtensions
    {
        internal static string GetRelativePath(this FileInfo fileInfo)
        {
            var relativePath = new DirectoryInfo(".").FullName.ToForwardSlash();
            var path = fileInfo.FullName.ToForwardSlash();
            return path.StartsWith(relativePath) ? path.Substring(relativePath.Length).TrimStart('/') : path;
        }
    }
    
    static class StringExtensions
    {
        internal static string ToForwardSlash(this string value)
        {
            return value.Replace('\\', '/');
        }
    }
}
