using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestLogsController : ControllerBase
    {
        private readonly ILogger<TestLogsController> _logger;

        public TestLogsController(ILogger<TestLogsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("LogTest")]
        public IActionResult LogTest()
        {
            _logger.LogInformation("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            _logger.LogDebug("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            _logger.LogWarning("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            try
            {
                string x = null;
                x = x.ToLower();
            }
            catch (Exception e) 
            {
                _logger.LogError(e, "Test logger");
            }
            return Ok(true);
        }

        [HttpGet("LogImmediately")]
        public IActionResult LogImmediately()
        {
            _logger.LogCritical("Test logger {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            return Ok(true);
        }
    }
}
