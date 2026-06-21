using System.IO;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IReportService
    {
        Task<byte[]> GenerateExcelReportAsync();
        Task<byte[]> GeneratePdfReportAsync();
        Task<string> GetMarketingReportDataAsync(string reportType);
    }
}
