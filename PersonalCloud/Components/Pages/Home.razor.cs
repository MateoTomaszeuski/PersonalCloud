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
        MediaService.IsImage(filePath);

    private bool IsVideo(string filePath) =>
        MediaService.IsVideo(filePath);

    protected override async Task OnInitializedAsync()
    {
        await LoadMediaAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Console.WriteLine("OnAfterRenderAsync triggered");
        Console.WriteLine(firstRender);
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("console.log", "Blazor is calling initLazyLoading");
            await JSRuntime.InvokeVoidAsync("window.initLazyLoading");
        }
    }
    private async Task LoadMediaAsync()
    {
        var m = await MediaService.GetAllMediaAsync();
        mediaList = m.ToList();
        selectedMedia = mediaList.ToDictionary(media => media, media => false);
        StateHasChanged();
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
    }
   private async Task DownloadSelectedMedia()
{
    var selectedPaths = selectedMedia.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
    var fileNames = selectedPaths.Select(Path.GetFileName).ToList();

    var zipName = $"media_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
    var zipPath = await MediaService.CreateZipFromFiles(fileNames, zipName);

    var downloadUrl = $"/download/{zipName}";
    await JSRuntime.InvokeVoidAsync("downloadFile", downloadUrl);
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