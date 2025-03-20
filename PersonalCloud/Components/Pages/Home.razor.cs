using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using PersonalCloud.Services;

namespace PersonalCloud.Components.Pages;

public partial class Home
{
    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    [Inject]
    public IMediaService MediaService { get; set; }
    private List<string> mediaList = new();
    private Dictionary<string, bool> selectedMedia = new();
    private string? fullImagePath;

    private bool IsImage(string filePath) =>
        filePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        filePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);

    private bool IsVideo(string filePath) =>
        filePath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
        filePath.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
        filePath.EndsWith(".avi", StringComparison.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
    {
        await LoadMediaAsync();
    }

    private string? fullVideoPath;
    private void LoadVideo(string videoPath)
    {
        fullVideoPath = videoPath;
    }
    private async Task LoadMediaAsync()
    {
        var m = await MediaService.GetAllMediaAsync(); 
        mediaList = m.ToList();
        selectedMedia = mediaList.ToDictionary(media => media, media => false);
        StateHasChanged();
    }

    private int pageSize = 20;
    private async ValueTask<ItemsProviderResult<string>> LoadMedia(ItemsProviderRequest request)
    {
        var mediaSubset = mediaList.Skip(request.StartIndex).Take(pageSize).ToList();
        return new ItemsProviderResult<string>(mediaSubset, mediaList.Count);
    }

    private string GetThumbnailPath(string fullPath)
    {
        fullPath = "/app/publish/wwwroot/" + fullPath;
        return MediaService.GetThumbnail(fullPath); // Generates a smaller version
    }
    private async ValueTask<ItemsProviderResult<string>> LoadMediaItems(ItemsProviderRequest request)
    {
        var itemsToLoad = mediaList.Skip(request.StartIndex).Take(request.Count).ToList();
        return new ItemsProviderResult<string>(itemsToLoad, mediaList.Count);
    }
    private void RefreshMedia()
    {
        mediaList = MediaService.GetAllMedia().OrderBy(media => Path.GetFileName(media)).ToList();
        selectedMedia = mediaList.ToDictionary(media => media, media => false);
    }

    private void DeleteSelectedMedia()
    {
        var filesToDelete = selectedMedia
            .Where(kv => kv.Value)
            .Select(kv => Path.GetFileName(kv.Key))
            .ToList();

        foreach (var fileName in filesToDelete)
        {
            MediaService.DeleteMedia(fileName);
        }

        RefreshMedia();
    }

    private async Task DownloadSelectedMedia()
    {
        var selectedPaths = selectedMedia.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
        var fileNames = selectedPaths.Select(Path.GetFileName).ToList();

        var zipName = $"media_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        var zipRelativePath = MediaService.CreateZipFromFiles(fileNames, zipName);

        await JSRuntime.InvokeVoidAsync("downloadFile", zipRelativePath);
    }

    private void ShowFullImage(string imagePath)
    {
        fullImagePath = imagePath;
    }

    private void CloseFullImage()
    {
        fullImagePath = null;
    }
}