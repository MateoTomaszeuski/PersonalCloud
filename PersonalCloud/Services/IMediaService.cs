using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Components.Forms;

namespace PersonalCloud.Services;

public interface IMediaService
{
    Task<string> CreateZipFromFiles(List<string> fileNames, string zipName);
    Task DeleteMedia(string fileName);
    Task<IEnumerable<string>> GetAllMediaAsync();
    Task<string> GetThumbnailAsync(string fileName);
    bool IsImage(string filePath);
    bool IsVideo(string filePath);
    Task UploadMedia(IBrowserFile file);
    Task UploadToBlob(Action<int> progressCallback);
    BlobClient GetBlobClient(string fileName);

}

