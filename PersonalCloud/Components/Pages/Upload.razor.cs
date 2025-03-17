using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PersonalCloud.Services;

namespace PersonalCloud.Components.Pages;
public partial class Upload
{
    [Inject]
    public IMediaService MediaService {get;set;}
    private List<IBrowserFile> files = new();
    private InputFile inputFile;
    private bool isUploading = false;
    private int uploadProgress = 0;
    private string uploadMessage = string.Empty;

    private void OnFilesSelected(InputFileChangeEventArgs e)
    {
        files = e.GetMultipleFiles().ToList();
        uploadMessage = string.Empty; // Clear old message
    }

    private async Task UploadMedia()
    {
        if (files.Count == 0)
            return;

        isUploading = true;
        uploadProgress = 0;
        uploadMessage = string.Empty;

        int totalFiles = files.Count;
        int uploadedFiles = 0;
        List<string> uploadedFileNames = new();

        foreach (var file in files)
        {
            await MediaService.UploadMedia(file);
            uploadedFiles++;
            uploadProgress = (int)((uploadedFiles / (double)totalFiles) * 100);
            uploadedFileNames.Add(file.Name);
            StateHasChanged();
        }

        // Reset after upload
        files.Clear();
        await Task.Delay(300); // Small delay for smooth UX
        isUploading = false;
        uploadProgress = 0;
        uploadMessage = $"Uploaded files: {string.Join(", ", uploadedFileNames)}";

        StateHasChanged();
    }
}