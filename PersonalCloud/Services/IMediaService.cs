using Microsoft.AspNetCore.Components.Forms;

namespace PersonalCloud.Services
{
    public interface IMediaService
    {
        void AddMediaToAlbum(string albumName, string fileName);
        void CreateAlbum(string albumName);
        void DeleteMedia(string fileName);
        IEnumerable<string> GetAlbumMedia(string albumName);
        IEnumerable<string> GetAlbums();
        IEnumerable<string> GetAllMedia();
        Task UploadMedia(IBrowserFile file);
    }
}