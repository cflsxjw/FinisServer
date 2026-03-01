using FinisServer.Models;
using FinisServer.Models.Exceptions;
using FinisServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ResourceController(IResourceService resourceService, IContentTypeProvider contentTypeProvider) : ControllerBase
{
    [HttpGet("avatar/{filename}")]
    public IActionResult GetAvatar(string filename)
    {
        var path = resourceService.GetAvatar(filename);
        contentTypeProvider.TryGetContentType(path, out var contentType);
        return PhysicalFile(path, contentType ?? "application/octet-stream");
    }
    
    [HttpGet("image/{*filename}")]
    public IActionResult GetImage(string filename)
    {
        var path = resourceService.GetImage(filename);
        contentTypeProvider.TryGetContentType(path, out var contentType);
        return PhysicalFile(path, contentType ?? "application/octet-stream");
    }
    
    [HttpPost("upload/image")]
    public async Task<Result<string>> UploadImagesAsync([FromForm] IFormFile file)
    {

        return Result<string>.Success(await resourceService.UploadImageAsync(file));
    }
}