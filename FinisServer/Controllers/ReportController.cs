using FinisServer.Models;
using FinisServer.Models.Dtos;
using FinisServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinisServer.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ReportController(IReportService reportService) : ControllerBase
{
    [Authorize]
    [HttpPost("submit")]
    public async Task<Result> SubmitReport([FromBody] ReportDto reportDto)
    {
        await reportService.SubmitReportAsync(reportDto);
        return Result.Success();
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("complete/{id:int}/{actionType:int}")]
    public async Task<Result> CompleteReport(int id, int actionType)
    {
        await reportService.ReportComplete(id, actionType);
        return Result.Success();
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("list")]
    public async Task<Result<List<ReportInfoDto>>> GetReports()
    {
        var result = await reportService.GetReportsAsync();
        return Result<List<ReportInfoDto>>.Success(result);
    }
}