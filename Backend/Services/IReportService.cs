using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    /// <summary>
    /// Service interface for marketing reports and automated Excel/PDF document exports.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generates a professional Excel report, saves it to disk, and returns the physical path.
        /// </summary>
        Task<string> GenerateExcelReportAsync();

        /// <summary>
        /// Generates a professional PDF report, saves it to disk, and returns the physical path.
        /// </summary>
        Task<string> GeneratePdfReportAsync();

        /// <summary>
        /// Retrieves metadata for all previously generated Excel and PDF reports.
        /// </summary>
        Task<IEnumerable<ReportFileDto>> GetGeneratedReportsAsync();

        /// <summary>
        /// Gets predefined marketing report data in JSON format (legacy dashboard support).
        /// </summary>
        Task<string> GetMarketingReportDataAsync(string reportType);
    }
}
