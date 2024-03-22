# Decompress a ZIP file into temp folder using `SharpCompress`

```
public string Decompress(string ebookName)
{
    var fileName = Path.GetFileName(ebookName);
    var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    // Create the subfolder
    Directory.CreateDirectory(tempPath);

    // Open the EPUB file as a ZIP archive
    using (var archive = ZipArchive.Open(ebookName))
    {
        // Extract all files to the temporary folder
        foreach (var entry in archive.Entries)
        {
            if (!entry.IsDirectory)
            {
                entry.WriteToDirectory(tempPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
            }
        }
    }

    return tempPath;
}
```
