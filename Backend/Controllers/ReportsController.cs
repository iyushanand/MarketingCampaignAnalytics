using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

        [HttpGet("marketing-report/{reportType}")]
        public async Task<IActionResult> GetMarketingReportData(string reportType)
        {
            var data = await _reportService.GetMarketingReportDataAsync(reportType);
            return Content(data, "application/json");
        }

        [HttpGet("download/excel")]
        public async Task<IActionResult> DownloadExcelReport()
        {
            var fileContents = await _reportService.GenerateExcelReportAsync();
            if (fileContents == null || fileContents.Length == 0)
            {
                return BadRequest("No report available.");
            }
            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Marketing_Campaign_Analytics_Report.xlsx");
        }

        [HttpGet("download/pdf")]
        public async Task<IActionResult> DownloadPdfReport()
        {
            var fileContents = await _reportService.GeneratePdfReportAsync();
            if (fileContents == null || fileContents.Length == 0)
            {
                return BadRequest("No report available.");
            }
            return File(fileContents, "application/pdf", "Marketing_Campaign_Executive_Summary.pdf");
        }
    }
}
