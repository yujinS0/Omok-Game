using Microsoft.AspNetCore.Mvc;
using MatchServer.DTO;
using MatchServer.Services.Interfaces;

namespace MatchServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckMatcingController : ControllerBase
    {
        private readonly ILogger<CheckMatcingController> _logger;
        private readonly ICheckMatchingService _checkMatchingService;

        public CheckMatcingController(ILogger<CheckMatcingController> logger, ICheckMatchingService checkMatchingService)
        {
            _logger = logger;
            _checkMatchingService = checkMatchingService;
        }

        [HttpPost]
        public async Task<MatchCompleteResponse> IsMatched([FromBody] MatchRequest request)
        {
            return await _checkMatchingService.IsMatched(request);
        }

        // <IActionResult>와 함께 BadRequest / OK 형태로 응답을 처리하는 게 좋을까요?
        //[HttpPost]
        //public async Task<IActionResult> IsMatched([FromBody] MatchRequest request)
        //{
        //    var response = await _checkMatchingService.IsMatched(request);
        //    if (response.Result == ErrorCode.InvalidRequest)
        //    {
        //        return BadRequest(response);
        //    }
        //    return Ok(response);
        //}
    }
}
