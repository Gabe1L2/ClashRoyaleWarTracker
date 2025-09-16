using ClashRoyaleWarTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClashRoyaleWarTracker.Web.Pages
{
    public class ScheduledTaskModel : PageModel
    {
        private readonly IApplicationService _applicationService;
        private readonly ILogger<ScheduledTaskModel> _logger;
        private readonly IConfiguration _configuration;

        public ScheduledTaskModel(
            IApplicationService applicationService, 
            ILogger<ScheduledTaskModel> logger,
            IConfiguration configuration)
        {
            _applicationService = applicationService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync(string? task = null, string? weeks = null, string? key = null)
        {
            try
            {
                // Security: Validate API key
                var expectedKey = _configuration["ScheduledTask:SecurityKey"];
                if (!string.IsNullOrEmpty(expectedKey))
                {
                    if (string.IsNullOrEmpty(key) || key != expectedKey)
                    {
                        _logger.LogWarning("Unauthorized scheduled task attempt from IP: {IP} with key: {Key}", 
                            HttpContext.Connection.RemoteIpAddress, key ?? "NULL");
                        Response.StatusCode = 401; // Unauthorized
                        return new JsonResult(new { 
                            success = false, 
                            message = "Invalid or missing security key",
                            timestamp = DateTime.Now
                        });
                    }
                }

                // Validate the task parameter
                if (string.IsNullOrEmpty(task))
                {
                    _logger.LogWarning("Scheduled task called without task parameter from IP: {IP}", 
                        HttpContext.Connection.RemoteIpAddress);
                    return BadRequest("Task parameter is required");
                }

                // Parse the weeks parameter for player averages (default to 4)
                if (!int.TryParse(weeks, out int numWeeksForPlayerAverages))
                {
                    numWeeksForPlayerAverages = 4; // Default value as per WeeklyUpdateAsync signature
                }

                // Validate the weeks parameter
                if (numWeeksForPlayerAverages < 1 || numWeeksForPlayerAverages > 10)
                {
                    _logger.LogWarning("Invalid weeks parameter: {Weeks} from IP: {IP}", 
                        weeks, HttpContext.Connection.RemoteIpAddress);
                    return BadRequest("Weeks must be between 1 and 10");
                }

                // Execute the task based on the parameter
                switch (task.ToLower())
                {
                    case "weekly":
                    case "weeklyupdate":
                        _logger.LogInformation("Starting authorized scheduled weekly update with {WeeksForPlayerAverages} weeks for player averages from IP: {IP}", 
                            numWeeksForPlayerAverages, HttpContext.Connection.RemoteIpAddress);
                        var result = await _applicationService.WeeklyUpdateAsync(numWeeksForPlayerAverages);
                        
                        if (result.Success)
                        {
                            _logger.LogInformation("Scheduled weekly update completed successfully: {Message}", result.Message);
                            Response.StatusCode = 200; // OK
                            return new JsonResult(new { 
                                success = true, 
                                message = result.Message,
                                timestamp = DateTime.Now,
                                weeksForPlayerAverages = numWeeksForPlayerAverages
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Scheduled weekly update failed: {Message}", result.Message);
                            return BadRequest(new { 
                                success = false, 
                                message = result.Message,
                                timestamp = DateTime.Now,
                                weeksForPlayerAverages = numWeeksForPlayerAverages
                            });
                        }
                    
                    default:
                        _logger.LogWarning("Unknown scheduled task: {Task} from IP: {IP}", 
                            task, HttpContext.Connection.RemoteIpAddress);
                        return BadRequest($"Unknown task: {task}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled task: {Task} from IP: {IP}", 
                    task, HttpContext.Connection.RemoteIpAddress);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An unexpected error occurred during the scheduled task",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}