# SharpCompress Based MultiFormat Compression/Decompression

## Usage

```csharp
// Compress a directory to a ZIP file using the file extension
string inputDirectory = @"C:\Source\Folder";
string outputFilePath = @"C:\Compressed.zip";
ZipHelper.Compress(inputDirectory, outputFilePath);

// Compress a directory to a RAR file specifying the compression format
string inputDirectory = @"C:\Source\Folder";
string outputFilePath = @"C:\Compressed.rar";
ZipHelper.Compress(inputDirectory, outputFilePath, ZipHelper.CompressionFormat.Rar);

// Decompress a GZip file using the file extension
string inputFilePath = @"C:\Compressed.gz";
string outputDirectory = @"C:\Destination\Folder";
ZipHelper.Decompress(inputFilePath, outputDirectory);

// Decompress a TAR file specifying the compression format
string inputFilePath = @"C:\Compressed.tar";
string outputDirectory = @"C:\Destination\Folder";
ZipHelper.Decompress(inputFilePath, outputDirectory, ZipHelper.CompressionFormat.Tar);
```

## Helper Class

```csharp
using System;
using System.IO;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;

public static class ZipHelper
{
    public enum CompressionFormat
    {
        Zip,
        Rar,
        GZip,
        SevenZip,
        Tar
    }

    public static void Compress(string inputDirectory, string outputFilePath, CompressionFormat? format = null)
    {
        if (string.IsNullOrWhiteSpace(inputDirectory) || !Directory.Exists(inputDirectory))
            throw new ArgumentException("Invalid input directory path.");

        if (string.IsNullOrWhiteSpace(outputFilePath))
            throw new ArgumentException("Invalid output file path.");

        using (Stream stream = File.OpenWrite(outputFilePath))
        using (IWriter writer = GetWriter(format, stream, outputFilePath))
        {
            writer.CompressionLevel = CompressionLevel.BestCompression;
            writer.WriteAll(inputDirectory, "*", SearchOption.AllDirectories);
        }
    }

    public static void Decompress(string inputFilePath, string outputDirectory, CompressionFormat? format = null)
    {
        if (string.IsNullOrWhiteSpace(inputFilePath) || !File.Exists(inputFilePath))
            throw new ArgumentException("Invalid input file path.");

        if (string.IsNullOrWhiteSpace(outputDirectory))
            throw new ArgumentException("Invalid output directory path.");

        Directory.CreateDirectory(outputDirectory);

        using (Stream stream = File.OpenRead(inputFilePath))
        using (IReader reader = ReaderFactory.Open(stream, GetReaderOptions(format, inputFilePath)))
        {
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    var destFilePath = Path.Combine(outputDirectory, reader.Entry.FilePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                    reader.WriteEntryToFile(destFilePath);
                }
            }
        }
    }

    private static IWriter GetWriter(CompressionFormat? format, Stream stream, string outputFilePath)
    {
        if (format.HasValue)
            return format.Value switch
            {
                CompressionFormat.Zip => new ZipWriter(stream),
                CompressionFormat.Rar => new RarArchiveWriter(stream),
                CompressionFormat.GZip => new GZipWriter(stream),
                CompressionFormat.SevenZip => new SevenZipWriter(stream),
                CompressionFormat.Tar => new TarWriter(stream),
                _ => throw new ArgumentException($"Unsupported compression format: {format.Value}"),
            };

        string extension = Path.GetExtension(outputFilePath).TrimStart('.').ToLower();
        return extension switch
        {
            "zip" => new ZipWriter(stream),
            "rar" => new RarArchiveWriter(stream),
            "gz" or "gzip" => new GZipWriter(stream),
            "7z" => new SevenZipWriter(stream),
            "tar" => new TarWriter(stream),
            _ => throw new ArgumentException($"Unsupported compression format: {extension}"),
        };
    }

    private static ReaderOptions GetReaderOptions(CompressionFormat? format, string inputFilePath)
    {
        if (format.HasValue)
            return format.Value switch
            {
                CompressionFormat.Zip => ReaderOptions.Zip,
                CompressionFormat.Rar => ReaderOptions.Rar,
                CompressionFormat.GZip => ReaderOptions.GZip,
                CompressionFormat.SevenZip => ReaderOptions.SevenZip,
                CompressionFormat.Tar => ReaderOptions.Tar,
                _ => throw new ArgumentException($"Unsupported compression format: {format.Value}"),
            };

        string extension = Path.GetExtension(inputFilePath).TrimStart('.').ToLower();
        return extension switch
        {
            "zip" => ReaderOptions.Zip,
            "rar" => ReaderOptions.Rar,
            "gz" or "gzip" => ReaderOptions.GZip,
            "7z" => ReaderOptions.SevenZip,
            "tar" => ReaderOptions.Tar,
            _ => throw new ArgumentException($"Unsupported compression format: {extension}"),
        };
    }
}
```
