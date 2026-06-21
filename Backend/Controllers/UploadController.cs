using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService ?? throw new ArgumentNullException(nameof(uploadService));
        }

        [HttpPost("sample")]
        public async Task<IActionResult> LoadSampleDataset()
        {
            var success = await _uploadService.LoadSampleDatasetAsync();
            if (!success)
            {
                return BadRequest(new { Message = "Failed to load sample dataset." });
            }
            return Ok(new { Message = "Sample dataset loaded successfully." });
        }

        [HttpPost("csv")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file uploaded or file is empty." });
            }

            using var stream = file.OpenReadStream();
            var success = await _uploadService.UploadCsvAsync(stream);
            if (!success)
            {
                return BadRequest(new { Message = "Failed to parse and store CSV data." });
            }

            return Ok(new { Message = "CSV dataset processed and loaded successfully." });
        }
    }
}
