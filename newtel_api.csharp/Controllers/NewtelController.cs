using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using newtel_api.csharp.Services;

namespace newtel_api.csharp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewtelController : ControllerBase
    {
        private readonly ILogger<NewtelController> _logger;
        private readonly INewtelService _newtelService;

        public NewtelController(ILogger<NewtelController> logger,INewtelService newtelService)
        {
            _logger = logger;
            _newtelService = newtelService;
        }

        [HttpPost]
        [Route("CallPassword")]
        public async Task<IActionResult> CallPassword(string phoneNumber)
        {
            try
            {
                return Ok(await _newtelService.CallPassword(phoneNumber));
            }
            catch (HttpRequestException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}