using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestLogsController : ControllerBase
    {
        private readonly ILogger<TestLogsController> _logger;
        private readonly ILogger _logger1;
        private readonly ILogger _logger2;

        public TestLogsController(ILogger<TestLogsController> logger,
                                  [FromKeyedServices("Logger1")] ILogger logger1,
                                  [FromKeyedServices("Logger2")] ILogger logger2)
        {
            _logger = logger;
            _logger1 = logger1;
            _logger2 = logger2;
        }

        [HttpGet("Logger1")]
        public IActionResult LogTest1()
        {
            _logger1.LogInformation("Test logger1 {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            _logger1.LogDebug("Test logger1 {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            _logger1.LogWarning("Test logger1 {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            try
            {
                string x = null;
                x = x.ToLower();
            }
            catch (Exception e)
            {
                _logger1.LogError(e, "Test logger1");
            }
            return Ok(true);
        }

        [HttpGet("Logger2")]
        public IActionResult LogTest2()
        {
            _logger2.LogInformation("Test logger2 {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            _logger2.LogDebug("Test logger2 {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            _logger2.LogWarning("Test logger2 {a} {@b}", 123, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            try
            {
                string x = null;
                x = x.ToLower();
            }
            catch (Exception e)
            {
                _logger2.LogError(e, "Test logger2");
            }
            return Ok(true);
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
