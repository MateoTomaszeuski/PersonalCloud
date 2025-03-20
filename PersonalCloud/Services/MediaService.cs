using System.IO.Compression;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Components.Forms;
using SkiaSharp;
namespace PersonalCloud.Services;


public class MediaService : IMediaService
{
    private readonly BlobContainerClient _mediaContainerClient;
    private readonly string _thumbnailContainerName = "thumbnails";
    private readonly BlobContainerClient _thumbnailContainerClient;
    public bool IsImage(string filePath) =>
           filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
           filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
           filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
           filePath.EndsWith(".heic", StringComparison.OrdinalIgnoreCase)
           ;

    public bool IsVideo(string filePath) =>
        filePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
        filePath.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
        filePath.EndsWith(".avi", StringComparison.OrdinalIgnoreCase);

    public MediaService()
    {
        // Ensure media, albums, and thumbnails directories exist
        string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;";
        string mediaContainerName = "media";

        _mediaContainerClient = new BlobContainerClient(connectionString, mediaContainerName);
        _mediaContainerClient.CreateIfNotExists();

        _thumbnailContainerClient = new BlobContainerClient(connectionString, _thumbnailContainerName);
        _thumbnailContainerClient.CreateIfNotExists();
    }

    public async Task UploadMedia(IBrowserFile file)
    {
        BlobClient blobClient = _mediaContainerClient.GetBlobClient(file.Name);

        try
        {
            await using Stream fileStream = file.OpenReadStream(long.MaxValue);
            await blobClient.UploadAsync(fileStream, true);
            Console.WriteLine($"Uploaded to Blob: {file.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading to blob {file.Name}: {ex.Message}");
        }
    }

    public async Task<IEnumerable<string>> GetAllMediaAsync()
    {
        var blobs = _mediaContainerClient.GetBlobsAsync();
        var mediaList = new List<string>();

        await foreach (var blobItem in blobs)
        {
            mediaList.Add(blobItem.Name);
        }

        return mediaList;
    }

    public async Task DeleteMedia(string fileName)
    {
        try
        {
            await _mediaContainerClient.DeleteBlobIfExistsAsync(fileName);
            Console.WriteLine($"Deleted Blob: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting blob {fileName}: {ex.Message}");
        }
    }

    public async Task<string> CreateZipFromFiles(List<string> fileNames, string zipName)
{
    var tempZipPath = Path.Combine(Path.GetTempPath(), zipName);
    using (var archive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
    {
        foreach (var fileName in fileNames)
        {
            var blobClient = _mediaContainerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                var tempFile = Path.GetTempFileName();
                await using (var tempFileStream = File.OpenWrite(tempFile))
                {
                    await blobClient.DownloadToAsync(tempFileStream);
                }
                archive.CreateEntryFromFile(tempFile, fileName);
                File.Delete(tempFile);
            }
        }
    }
    return tempZipPath;
}

    public async Task<string> GetThumbnailAsync(string fileName)
    {
        var thumbnailBlobName = Path.GetFileNameWithoutExtension(fileName) + "_thumb.jpg";
        var thumbnailBlobClient = _thumbnailContainerClient.GetBlobClient(thumbnailBlobName);

        if (!await thumbnailBlobClient.ExistsAsync())
        {
            await GenerateThumbnailAsync(fileName, thumbnailBlobClient);
        }

        return thumbnailBlobClient.Uri.ToString();
    }
    private async Task GenerateThumbnailAsync(string originalFileName, BlobClient thumbnailBlobClient)
    {
        var originalBlobClient = _mediaContainerClient.GetBlobClient(originalFileName);

        if (!await originalBlobClient.ExistsAsync())
        {
            Console.WriteLine($"Original blob not found: {originalFileName}");
            return;
        }

        var tempFile = Path.GetTempFileName();
        await using (var tempStream = File.OpenWrite(tempFile))
        {
            await originalBlobClient.DownloadToAsync(tempStream);
        }

        using var inputStream = File.OpenRead(tempFile);
        using var originalImage = SKBitmap.Decode(inputStream);

        if (originalImage == null)
        {
            Console.WriteLine($"Failed to decode image: {originalFileName}");
            File.Delete(tempFile);
            return;
        }

        int thumbnailSize = 150;
        float ratio = Math.Min((float)thumbnailSize / originalImage.Width, (float)thumbnailSize / originalImage.Height);
        int width = (int)(originalImage.Width * ratio);
        int height = (int)(originalImage.Height * ratio);

        using var resizedImage = originalImage.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        using var image = SKImage.FromBitmap(resizedImage);
        using var ms = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(ms);
        ms.Position = 0;

        await thumbnailBlobClient.UploadAsync(ms, true);
        File.Delete(tempFile);
        Console.WriteLine($"Generated and uploaded thumbnail: {thumbnailBlobClient.Name}");
    }
    public async Task UploadToBlob(Action<int> progressCallback)
    {
        string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;";

        string containerName = "media";
        string localFolder = "/app/publish/wwwroot/media"; // Matches Docker volume mount

        BlobContainerClient containerClient = new BlobContainerClient(connectionString, containerName);
        await containerClient.CreateIfNotExistsAsync();

        Console.WriteLine("made the containerclient");

        var files = Directory.GetFiles(localFolder);
        int totalFiles = files.Length;
        int uploadedFiles = 0;

        foreach (var filePath in Directory.GetFiles(localFolder))
        {
            string fileName = Path.GetFileName(filePath);
            Console.WriteLine($"Making container for{fileName}");
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            Console.WriteLine($"container for{fileName}");

            using var fileStream = File.OpenRead(filePath);

            var progress = new Progress<long>(bytesTransferred =>
            {
                double percent = (bytesTransferred / (double)fileStream.Length) * 100;
                progressCallback?.Invoke((int)percent);
            });
            // Upload file with progress tracking
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                ProgressHandler = progress
            });

            uploadedFiles++;
            progressCallback?.Invoke((int)((uploadedFiles / (double)totalFiles) * 100));
            Console.WriteLine($"Uploaded: {fileName}");
        }

        Console.WriteLine("Migration completed!");
    }
public BlobClient GetBlobClient(string fileName) => _mediaContainerClient.GetBlobClient(fileName);

}
