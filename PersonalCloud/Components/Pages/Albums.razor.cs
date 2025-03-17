using Microsoft.AspNetCore.Components;
using PersonalCloud.Services;

namespace PersonalCloud.Components.Pages;
public partial class Albums
{
    [Inject]
    public IMediaService MediaService {get;set;}
    private List<string> albumList = new List<string>();
    private string newAlbumName = string.Empty;

    protected override void OnInitialized()
    {
        RefreshAlbums();
    }

    private void RefreshAlbums()
    {
        albumList = MediaService.GetAlbums().ToList();
    }

    private void CreateAlbum()
    {
        if (!string.IsNullOrWhiteSpace(newAlbumName))
        {
            MediaService.CreateAlbum(newAlbumName);
            newAlbumName = string.Empty;
            RefreshAlbums();
        }
    }
}