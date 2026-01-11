using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using BacklogBasement.Services;
using System.Linq;
using System.Threading.Tasks;

namespace BacklogBasement.Controllers
{
    [Route("api/test-igdb")]
    [ApiController]
    public class TestIgdbController : ControllerBase
    {
        private readonly IIgdbService _igdbService;

        public TestIgdbController(IIgdbService igdbService)
        {
            _igdbService = igdbService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> TestIgdbSearch([FromQuery] string query = "mario")
        {
            try
            {
                var results = await _igdbService.SearchGamesAsync(query);
                return Ok(new 
                { 
                    success = true, 
                    resultCount = results.Count(),
                    results = results.Select(g => new { g.Id, g.Name })
                });
            }
            catch (HttpRequestException ex)
            {
                return Ok(new 
                { 
                    success = false, 
                    error = "HTTP Error", 
                    details = ex.Message,
                    statusCode = ex.StatusCode?.ToString()
                });
            }
            catch (Exception ex)
            {
                return Ok(new 
                { 
                    success = false, 
                    error = "Other Error",
                    details = ex.Message 
                });
            }
        }
    }
}