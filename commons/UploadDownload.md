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
```
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
