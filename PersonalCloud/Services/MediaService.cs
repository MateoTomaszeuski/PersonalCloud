using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using SkiaSharp;

namespace PersonalCloud.Services
{
    public class MediaService : IMediaService
    {
        private readonly string mediaFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media");
        private readonly string albumsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "albums");
        private readonly string thumbnailsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "thumbnails");

        public MediaService()
        {
            // Ensure media, albums, and thumbnails directories exist
            Directory.CreateDirectory(mediaFolder);
            Directory.CreateDirectory(albumsFolder);
            Directory.CreateDirectory(thumbnailsFolder);
        }

        public async Task<IEnumerable<string>> GetAllMediaAsync()
        {
            return await Task.Run(() => Directory.GetFiles(mediaFolder)
                .Select(file => Path.Combine("media", Path.GetFileName(file))));
        }

        public IEnumerable<string> GetAllMedia()
        {
            return Directory.GetFiles(mediaFolder)
                .Select(file => Path.Combine("media", Path.GetFileName(file)));
        }

        public void DeleteMedia(string fileName)
        {
            var filePath = Path.Combine(mediaFolder, fileName);
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Deleted: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting {fileName}: {ex.Message}");
            }
        }

        public async Task UploadMedia(IBrowserFile file)
        {
            var filePath = Path.Combine(mediaFolder, file.Name);
            try
            {
                await using FileStream fs = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(long.MaxValue).CopyToAsync(fs);
                Console.WriteLine($"Uploaded: {file.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file {file.Name}: {ex.Message}");
            }
        }

        public string CreateZipFromFiles(List<string> fileNames, string zipName)
        {
            var zipPath = Path.Combine("wwwroot", "downloads", zipName);
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in fileNames)
                {
                    var fullPath = Path.Combine(mediaFolder, file);
                    if (File.Exists(fullPath))
                    {
                        archive.CreateEntryFromFile(fullPath, file);
                    }
                }
            }

            _ = DeleteZipAfterDelay(zipPath, TimeSpan.FromMinutes(5));
            return $"/downloads/{zipName}";
        }

        private async Task DeleteZipAfterDelay(string zipPath, TimeSpan delay)
        {
            await Task.Delay(delay);
            try
            {
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                    Console.WriteLine($"Deleted temporary ZIP file: {zipPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting ZIP file: {ex.Message}");
            }
        }

        public string GetThumbnail(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"Error: File not found - {fullPath}");
                return "/images/default-thumbnail.jpg"; // Return default thumbnail
            }

            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var extension = Path.GetExtension(fullPath);
            var thumbnailPath = Path.Combine(thumbnailsFolder, $"{fileName}_thumb{extension}");

            if (!File.Exists(thumbnailPath))
            {
                try
                {
                    GenerateThumbnail(fullPath, thumbnailPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Thumbnail generation failed: {ex.Message}");
                    return "/images/default-thumbnail.jpg"; // Fallback
                }
            }

            return $"/media/thumbnails/{fileName}_thumb{extension}";
        }

        private void GenerateThumbnail(string originalPath, string thumbnailPath)
{
    if (!File.Exists(originalPath))
    {
        Console.WriteLine($"Error: Original file not found - {originalPath}");
        return;
    }

    try
    {
        using var inputStream = File.OpenRead(originalPath);
        using var originalImage = SKBitmap.Decode(inputStream);

        if (originalImage == null)
        {
            Console.WriteLine($"Error: Failed to decode image - {originalPath}");
            return;
        }

        int thumbnailSize = 150;
        float ratio = Math.Min((float)thumbnailSize / originalImage.Width, (float)thumbnailSize / originalImage.Height);
        int width = (int)(originalImage.Width * ratio);
        int height = (int)(originalImage.Height * ratio);

        using var resizedImage = originalImage.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
        if (resizedImage == null)
        {
            Console.WriteLine($"Error: Failed to resize image - {originalPath}");
            return;
        }

        using var image = SKImage.FromBitmap(resizedImage);
        using var output = File.OpenWrite(thumbnailPath);
        image.Encode(SKEncodedImageFormat.Jpeg, 80).SaveTo(output);

        Console.WriteLine($"Thumbnail generated: {thumbnailPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error generating thumbnail for {originalPath}: {ex.Message}");
    }
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
}
