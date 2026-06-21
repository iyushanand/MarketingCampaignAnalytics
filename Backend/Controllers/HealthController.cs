using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    /// <summary>
    /// Health check controller for verifying API status.
    /// </summary>
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Gets the health status of the application.
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            var health = new
            {
                status = "Healthy",
                application = "Marketing Campaign Analytics Platform",
                version = "1.0"
            };

            return Ok(health);
        }
    }
}
