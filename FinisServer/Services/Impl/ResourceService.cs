using FinisServer.Configurations.Database;
using FinisServer.Models;
using FinisServer.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinisServer.Services.Impl;


public class ResourceService(IWebHostEnvironment webHostEnvironment, FinisDbContext finisDbContext) : IResourceService
{
    
    public string GetAvatar(int id)
    {
        string filename = finisDbContext.Users.FirstOrDefault(u => u.Id == id)?.Avatar
                          ?? throw new ResourceNotFoundException("未找到请求的资源");
        string path;
        if (filename == "DefaultAvatar.jpeg")
        {
            path = Path.Combine(webHostEnvironment.ContentRootPath, "Resources", filename);
            if (!File.Exists(path))
            {
                throw new ResourceNotFoundException("未找到请求的资源");
            }
        }
        else
        {
            path = GetImage(filename);
        }
        return path;
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file.Length == 0) throw new InvalidUploadFileException("文件为空");
        if (file.Length > 5 * 1024 * 1024) throw new InvalidUploadFileException("文件超过 5MB 限额");
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension)) throw new InvalidUploadFileException("不支持的文件格式");
        var newFileName = $"{Guid.NewGuid():N}{extension}";
        var subDir1 = newFileName.Substring(0, 2);
        var subDir2 = newFileName.Substring(2, 2);
        var targetFolder = Path.Combine(Constants.UploadRoot, subDir1, subDir2);
        var relativePath = $"{subDir1}/{subDir2}/{newFileName}";
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }
        var fullPath = Path.Combine(targetFolder, newFileName);
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        return relativePath;
    }

    public string GetImage(string filename)
    {
        string path = Path.Combine(Constants.UploadRoot, filename);
        if (!File.Exists(path))
        {
            throw new ResourceNotFoundException("未找到请求的资源");
        }
        return path;
    }

    public async Task<string> UploadAvatarAsync(IFormFile file)
    {
        return await UploadImageAsync(file);
    }
}