using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCloud.Services;
public class MediaService : IMediaService
{
    private readonly string mediaFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
    private readonly string albumsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "albums");

    public MediaService()
    {
        // Ensure the media and albums folders exist in wwwroot
        if (!Directory.Exists(mediaFolder)) Directory.CreateDirectory(mediaFolder);
        if (!Directory.Exists(albumsFolder)) Directory.CreateDirectory(albumsFolder);
    }

    public IEnumerable<string> GetAllMedia()
    {
        var d = Directory.GetFiles(mediaFolder).Select(file => Path.Combine("media", Path.GetFileName(file)));
        return d;
    }

    public void DeleteMedia(string fileName)
    {
        var filePath = Path.Combine(mediaFolder, fileName);
        try
        {

            if (File.Exists(filePath)) File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }

    public async Task UploadMedia(IBrowserFile file)
    {
        var filePath = Path.Combine(mediaFolder, file.Name);
        await using FileStream fs = new FileStream(filePath, FileMode.Create);
        await file.OpenReadStream(long.MaxValue).CopyToAsync(fs);
    }

    public IEnumerable<string> GetAlbums()
    {
        return Directory.GetDirectories(albumsFolder).Select(Path.GetFileName);
    }

    public void CreateAlbum(string albumName)
    {
        var albumPath = Path.Combine(albumsFolder, albumName);
        if (!Directory.Exists(albumPath)) Directory.CreateDirectory(albumPath);
    }

    public IEnumerable<string> GetAlbumMedia(string albumName)
    {
        var albumPath = Path.Combine(albumsFolder, albumName);
        return Directory.Exists(albumPath) ? Directory.GetFiles(albumPath).Select(Path.GetFileName) : new List<string>();
    }

    public void AddMediaToAlbum(string albumName, string fileName)
    {
        var albumPath = Path.Combine(albumsFolder, albumName);
        var sourcePath = Path.Combine(mediaFolder, fileName);
        var destinationPath = Path.Combine(albumPath, fileName);
        if (File.Exists(sourcePath) && !File.Exists(destinationPath)) File.Copy(sourcePath, destinationPath);
    }
}
