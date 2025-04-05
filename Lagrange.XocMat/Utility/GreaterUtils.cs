using System.IO.Compression;

namespace Lagrange.XocMat.Utility;

public class GreaterUtils
{
    public static byte[] GenerateCompressed(List<(string fileName, byte[] buffer)> data)
    {
        using var ms = new MemoryStream();
        using var zip = new ZipArchive(ms, ZipArchiveMode.Create);
        foreach (var (filename, buffer) in data)
        {
            if (buffer is null || buffer.Length == 0)
                continue;
            var entry = zip.CreateEntry(filename, CompressionLevel.Fastest);
            using var stream = entry.Open();
            stream.Write(buffer);
            stream.Flush();
        }
        ms.Flush();
        zip.Dispose();
        return ms.ToArray();
    }

}
