using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCloud.Services
{
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

        public string CreateZipFromFiles(List<string> fileNames, string zipName)
        {
            var zipPath = Path.Combine("wwwroot", "downloads", zipName);
            Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in fileNames)
                {
                    var fullPath = Path.Combine("wwwroot", "media", file);
                    if (File.Exists(fullPath))
                    {
                        archive.CreateEntryFromFile(fullPath, file);
                    }
                }
            }

            // Schedule deletion of the ZIP file
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

        // New method to get the storage usage percentage
        public double GetStorageUsagePercentage()
        {
            // Get the drive information for the directory where media and albums are stored
            var driveInfo = new DriveInfo(Path.GetPathRoot(Directory.GetCurrentDirectory()));

            // Get total and available space
            long totalSpace = driveInfo.TotalSize;
            long availableSpace = driveInfo.AvailableFreeSpace;

            // Calculate used space and the percentage of usage
            long usedSpace = totalSpace - availableSpace;
            double usagePercentage = (double)usedSpace / totalSpace * 100;

            return usagePercentage;
        }
        public string GetThumbnail(string fullPath)
        {
            // Generate or retrieve a low-resolution version of the image
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var extension = Path.GetExtension(fullPath);

            var thumbnailPath = Path.Combine(directory, "thumbnails", $"{fileName}_thumb{extension}");

            // Check if the thumbnail exists; if not, create one
            if (!File.Exists(thumbnailPath))
            {
                GenerateThumbnail(fullPath, thumbnailPath);
            }

            return thumbnailPath;
        }

        private void GenerateThumbnail(string originalPath, string thumbnailPath)
        {
            using var image = System.Drawing.Image.FromFile(originalPath);
            var thumbnail = image.GetThumbnailImage(150, 150, () => false, IntPtr.Zero);
            thumbnail.Save(thumbnailPath);
        }
    }
}
