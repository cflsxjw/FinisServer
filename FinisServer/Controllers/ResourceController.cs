using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Exceptions;
using FinisServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ResourceController(IResourceService resourceService, IContentTypeProvider contentTypeProvider) : ControllerBase
{
    [HttpGet("avatar/{id:int}")]
    public IActionResult GetAvatar(int id)
    {
        var path = resourceService.GetAvatar(id);
        contentTypeProvider.TryGetContentType(path, out var contentType);
        var etag = $"\"{path}\"";
        if (Request.Headers.TryGetValue("If-None-Match", out var incomingEtag) && incomingEtag == etag)
        {
            return StatusCode(304);
        }
        Response.Headers.Append("ETag", etag);
        Response.Headers.Append("Cache-Control", "no-cache");
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
    public async Task<Result<PathDto>> UploadImagesAsync([FromForm] IFormFile file)
    {
        return Result<PathDto>.Success(new PathDto(await resourceService.UploadImageAsync(file)));
    }
    [HttpPost("upload/avatar")]
    public async Task<Result<PathDto>> UploadAvatarAsync([FromForm] IFormFile file)
    {
        return Result<PathDto>.Success(new PathDto(await resourceService.UploadAvatarAsync(file)));
    }
}