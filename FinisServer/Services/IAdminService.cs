using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Models.Entities;
using FinisServer.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Services;

public interface IAdminService
{
    Task BlockArticleAsync(int id);
    Task UnBlockArticleAsync(int id);
}