using System.Formats.Tar;
using System.IO.Compression;

namespace SabakaConvert.Converters;

[Converter]
public class ArchiveConverter : IFileConverter
{
    private static readonly HashSet<string> Formats = 
    [
        with(StringComparer.OrdinalIgnoreCase),
        ".zip",
        ".tar",
        ".gz",
        ".tar.gz"
    ];
    
    public bool CanConvert(string fromExt, string toExt)
    {
        var from = NormalizeExtension(fromExt);
        var to = NormalizeExtension(toExt);
        
        return !from.Equals(to, StringComparison.OrdinalIgnoreCase)
            && Formats.Contains(from)
            && Formats.Contains(to);
    }

    public void Convert(string inputPath, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input archive not found.", inputPath);

        var fromExt = GetArchiveExtension(inputPath);
        var toExt = GetArchiveExtension(outputPath);

        if (!CanConvert(fromExt, toExt))
            throw new NotSupportedException($"Convertation from '{fromExt}' to '{toExt}' unsupported.");

        var tempDir = Path.Combine(Path.GetTempPath(), "SabakaConvert", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var extractDir = Path.Combine(tempDir, "extracted");
            Directory.CreateDirectory(extractDir);

            ExtractArchive(inputPath, fromExt, extractDir);
            
            var outputParent = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (!string.IsNullOrEmpty(outputParent))
                Directory.CreateDirectory(outputParent);

            if (File.Exists(outputPath))
                File.Delete(outputPath);

            CreateArchive(extractDir, toExt, outputPath);
        }
        finally
        {
            if (Directory.Exists(tempDir)) 
                Directory.Delete(tempDir, recursive: true);
        }
    }

    private static void ExtractArchive(string archivePath, string archiveExt, string destinationDir)
    {
        var fullDestDir = EnsureTrailingSeparator(Path.GetFullPath(destinationDir));

        switch (archiveExt.ToLowerInvariant())
        {
            case ".zip":
                ExtractZipSafely(archivePath, fullDestDir);
                break;
            
            case ".tar":
                using (var fs = File.OpenRead(archivePath))
                {
                    TarFile.ExtractToDirectory(fs, fullDestDir, overwriteFiles: true);
                }
                break;
            
            case ".tar.gz":
                using (var fs = File.OpenRead(archivePath))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    TarFile.ExtractToDirectory(gz, fullDestDir, overwriteFiles: true);
                }
                break;
            
            case ".gz":
                var rawName = Path.GetFileNameWithoutExtension(archivePath);
                if (string.IsNullOrWhiteSpace(rawName))
                    rawName = "decompressed.bin";
                
                var outFile = GetSafePath(fullDestDir, rawName);
                using (var fs = File.OpenRead(archivePath))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                using (var outStream = File.Create(outFile))
                {
                    gz.CopyTo(outStream);
                }
                break;
            
            default:
                throw new NotSupportedException($"Unpacking '{archiveExt}' not supported.");
        }
    }

    private static void CreateArchive(string sourceDir, string targetExt, string outputPath)
    {
        switch (targetExt.ToLowerInvariant())
        {
            case ".zip":
                ZipFile.CreateFromDirectory(sourceDir, outputPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                break;

            case ".tar":
                TarFile.CreateFromDirectory(sourceDir, outputPath, includeBaseDirectory: false);
                break;

            case ".tar.gz":
                using (var fs = File.Create(outputPath))
                using (var gz = new GZipStream(fs, CompressionLevel.Optimal))
                {
                    TarFile.CreateFromDirectory(sourceDir, gz, includeBaseDirectory: false);
                }
                break;

            case ".gz":
                var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
                if (files.Length == 1)
                {
                    using var inStream = File.OpenRead(files[0]);
                    using var outStream = File.Create(outputPath);
                    using var gz = new GZipStream(outStream, CompressionLevel.Optimal);
                    inStream.CopyTo(gz);
                }
                else
                {
                    var tempTar = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tar");
                    try
                    {
                        TarFile.CreateFromDirectory(sourceDir, tempTar, includeBaseDirectory: false);
                        using var inStream = File.OpenRead(tempTar);
                        using var outStream = File.Create(outputPath);
                        using var gz = new GZipStream(outStream, CompressionLevel.Optimal);
                        inStream.CopyTo(gz);
                    }
                    finally
                    {
                        if (File.Exists(tempTar))
                            File.Delete(tempTar);
                    }
                }
                break;
            
            default:
                throw new NotSupportedException($"Создание '{targetExt}' не поддерживается.");
            
        }
    }

    private static void ExtractZipSafely(string zipPath, string fullDestDir)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            var targetPath = GetSafePath(fullDestDir, entry.FullName);

            if (string.IsNullOrEmpty(entry.Name))
            {
                Directory.CreateDirectory(targetPath);
                continue;
            }

            var parentDir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(parentDir))
                Directory.CreateDirectory(parentDir);

            entry.ExtractToFile(targetPath, overwrite: true);
        }
    }
    
    private static string GetSafePath(string fullDestDir, string relativeEntryPath)
    {
        var normalizedEntry = relativeEntryPath.Replace('\\', '/').TrimStart('/');
        var combined = Path.GetFullPath(Path.Combine(fullDestDir, normalizedEntry));

        if (!combined.StartsWith(fullDestDir, StringComparison.Ordinal))
            throw new InvalidOperationException($"Unsafe path in archive: '{relativeEntryPath}'.");

        return combined;
    }
    
    private static string GetArchiveExtension(string path)
    {
        if (path.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase))
        {
            return ".tar.gz";
        }

        return NormalizeExtension(Path.GetExtension(path));
    }

    private static string NormalizeExtension(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext))
            return string.Empty;

        if (ext.Equals(".tgz", StringComparison.OrdinalIgnoreCase))
            return ".tar.gz";

        return ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}";
    }

    private static string EnsureTrailingSeparator(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;
}