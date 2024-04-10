# File Uploads
Upload a file as POST endpoint and process it easiy
```csharp
 [HttpPost("upload")]
 public async Task<IActionResult> Post([FromForm] IFormFileCollection files)
 {
     foreach (var file in files)
     {
         var filename = file.FileName;
         var filesize = file.Length;
         var extension = Path.GetExtension(filename);

         using (var ms = new MemoryStream())
         {
             await file.CopyToAsync(ms);
             var fileBytes = ms.ToArray();
             System.IO.File.WriteAllBytes("PathToSaveFile", fileBytes);
         }
     }
     return Ok(true);
 }
```

# File Downloads
Download a file as octet stream or any other mime type
```csharp
    [HttpGet("download/{filename}")]
    public IActionResult Download(string filename)
    {
        var filePath = "PathToSaveFile"; // Path where the file was saved
        var mimeType = "application/octet-stream"; // Octet-stream MIME type

        // Check if the file exists
        if (System.IO.File.Exists(filePath))
        {
            // Return the file as a FileStreamResult
            var fileStream = new FileStream(filePath, FileMode.Open);
            return File(fileStream, mimeType, filename);
        }
        else
        {
            // If the file doesn't exist, return NotFound
            return NotFound();
        }
    }
```

# Compressed Image Downloading/Streaming
Image compression is controlled by
 - IMAGE_SIZE_REDUCTION_FACTOR
 - IMAGE_DPI_REDUCTION_FACTOR
 - IMAGE_BITDEPTH_REDUCTION_FACTOR

> There are modes below - DOWNLOAD IMAGE or STREAM IMAGE

```csharp
//Triggers Image Download

[HttpGet("download/{filename}")]
public async Task<IActionResult> DownloadAsync(string filename)
{
    const int IMAGE_SIZE_REDUCTION_FACTOR = 8;
    const int IMAGE_DPI_REDUCTION_FACTOR = 4;
    const PngBitDepth IMAGE_BITDEPTH_REDUCTION_FACTOR = PngBitDepth.Bit4;

    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedImages", $"{filename}.png");
    var mimeType = "image/png";

    if (System.IO.File.Exists(filePath))
    {
        // Use FileStream to directly stream the file to the response
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        // Resize the image and save it to the fileStream
        using (var image = SixLabors.ImageSharp.Image.Load(fileStream))
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var newWidth = originalWidth / IMAGE_SIZE_REDUCTION_FACTOR;
            var newHeight = originalHeight / IMAGE_SIZE_REDUCTION_FACTOR;

            image.Mutate(x => x.Resize(newWidth, newHeight));

            var encoder = new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression,
                ColorType = PngColorType.Palette,
                BitDepth = IMAGE_BITDEPTH_REDUCTION_FACTOR
            };

            // Reduce the DPI
            image.Metadata.HorizontalResolution = image.Metadata.HorizontalResolution / IMAGE_DPI_REDUCTION_FACTOR;
            image.Metadata.VerticalResolution = image.Metadata.VerticalResolution / IMAGE_DPI_REDUCTION_FACTOR;

            // Create a new MemoryStream to store the compressed image
            using (var outputStream = new MemoryStream())
            {
                await image.SaveAsync(outputStream, encoder);
                outputStream.Seek(0, SeekOrigin.Begin);

                // Return the file as a FileContentResult
                return new FileContentResult(outputStream.ToArray(), mimeType)
                {
                    FileDownloadName = $"{filename}.png"
                };
            }
        }
    }
    else
    {
        return NotFound();
    }
}

//Stream Image Into Browser

 [HttpGet("stream/{filename}")]
 public async Task<IActionResult> DownloadAsync(string filename)
 {
     const int IMAGE_SIZE_REDUCTION_FACTOR = 8;
     const int IMAGE_DPI_REDUCTION_FACTOR = 4;
     const PngBitDepth IMAGE_BITDEPTH_REDUCTION_FACTOR = PngBitDepth.Bit4;

     var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedImages", $"{filename}.png");
     var mimeType = "image/png";

     if (System.IO.File.Exists(filePath))
     {
         // Use FileStream to directly stream the file to the response
         var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

         // Resize the image and save it to the fileStream
         using (var image = SixLabors.ImageSharp.Image.Load(fileStream))
         {
             var originalWidth = image.Width;
             var originalHeight = image.Height;
             var newWidth = originalWidth / IMAGE_SIZE_REDUCTION_FACTOR;
             var newHeight = originalHeight / IMAGE_SIZE_REDUCTION_FACTOR;

             image.Mutate(x => x.Resize(newWidth, newHeight));

             var encoder = new PngEncoder
             {
                 CompressionLevel = PngCompressionLevel.BestCompression,
                 ColorType = PngColorType.Palette,
                 BitDepth = IMAGE_BITDEPTH_REDUCTION_FACTOR
             };

             // Reduce the DPI
             image.Metadata.HorizontalResolution = image.Metadata.HorizontalResolution / IMAGE_DPI_REDUCTION_FACTOR;
             image.Metadata.VerticalResolution = image.Metadata.VerticalResolution / IMAGE_DPI_REDUCTION_FACTOR;

             // Create a new MemoryStream to store the compressed image
             byte[] imageBytes;
             using (var outputStream = new MemoryStream())
             {
                 await image.SaveAsync(outputStream, encoder);
                 imageBytes = outputStream.ToArray();
             }

             // Return the file as a FileStreamResult to display in the browser
             var resultStream = new MemoryStream(imageBytes);
             return new FileStreamResult(resultStream, mimeType);
         }
     }
     else
     {
         return NotFound();
     }
 }
```

## Api.js Uploading With Progress
Using Api.js to upload with Axios and reporting progress
```jsx
const [selectedFile, setSelectedFile] = useState(null);

const onFileChange = (event) => {
    setSelectedFile(event.target.files[0]);
};

const onUploadClick = async () => {
        if (selectedFile) {
            await Api.UPLOAD(
                '/ebooks/upload',
                selectedFile,
                {
                    onUpload: (progress) => {
                        //Upload in progress
                        console.log(`Upload progress: ${progress}%`);
                    },
                    onSuccessfulUpload: (data) => {
                        //File uploaded
                    },
                    onUploadFailure: (error) => {
                        //Upload failed
                        console.error('File upload failed:', error);
                    },
                },
                null
            );
        }
};

<input type="file" className="form-control" onChange={onFileChange} />
<a className="btn btn-primary" onClick={onUploadClick}>Upload</a>
```
