using PersonalCloud.Components;
using PersonalCloud.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IMediaService, MediaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var mediaService = app.Services.GetRequiredService<IMediaService>();
app.MapGet("/media/{filename}", async (string filename, IMediaService mediaService) =>
{
    var blobClient =  mediaService.GetBlobClient(filename);
    if (await blobClient.ExistsAsync())
    {
        var stream = await blobClient.OpenReadAsync();
        var contentType = "application/octet-stream";
        if (mediaService.IsImage(filename)) contentType = "image/jpeg";
        else if (mediaService.IsVideo(filename)) contentType = "video/mp4";

        return Results.Stream(stream, contentType);
    }

    return Results.NotFound();
});

app.MapGet("/download/{zipName}", async (string zipName) =>
{
    var zipPath = Path.Combine(Path.GetTempPath(), zipName);
    if (System.IO.File.Exists(zipPath))
    {
        var stream = System.IO.File.OpenRead(zipPath);
        return Results.File(stream, "application/zip", zipName);
    }
    return Results.NotFound();
});

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
