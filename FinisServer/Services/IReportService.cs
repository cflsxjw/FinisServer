using FinisServer.Models.Dtos;
namespace FinisServer.Services;
public interface IReportService
{
    Task SubmitReportAsync(ReportDto reportDto);
    Task ReportComplete(int id, int actionType);
    Task<List<ReportInfoDto>> GetReportsAsync();
}