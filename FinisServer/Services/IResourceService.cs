using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Services;

public interface IResourceService
{
    public string GetAvatar(int id);
    public Task<string> UploadImageAsync(IFormFile file);
    public string GetImage(string filename);
    public Task<string> UploadAvatarAsync(IFormFile file);
}
