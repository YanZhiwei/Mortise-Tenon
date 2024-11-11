using System.Security.Cryptography;

namespace Tenon.Helper.Internal;

public sealed class FileHelper
{
    public string RemoveFileNameInvalidChars(string filename)
    {
        return string.IsNullOrWhiteSpace(filename)
            ? null
            : string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }

    public string ReplaceFileNameInvalidChars(string filename, string separator = "_")
    {
        return string.IsNullOrWhiteSpace(filename)
            ? null
            : string.Join(separator, filename.Split(Path.GetInvalidFileNameChars()));
    }

    public static string GetFileMd5(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File does not exist.", filePath);

        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            const int bufferSize = 8192; 
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                md5.TransformBlock(buffer, 0, bytesRead, null, 0);
            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }
    }

    public static string GetStreamMd5(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream), "Stream cannot be null.");

        return ComputeMd5Hash(stream);
    }

    private static string ComputeMd5Hash(Stream stream)
    {
        using (var md5 = MD5.Create())
        {
            const int bufferSize = 8192;
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                md5.TransformBlock(buffer, 0, bytesRead, null, 0);

            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }
    }
}