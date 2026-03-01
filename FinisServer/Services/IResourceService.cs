using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Services;

public interface IResourceService
{
    public string GetAvatar(string filename);
    public Task<string> UploadImageAsync(IFormFile file);
    public string GetImage(string filename);
}
