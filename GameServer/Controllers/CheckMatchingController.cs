using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServer.DTO;
using GameServer.Services.Interfaces;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckMatchingController : ControllerBase
{
    private readonly ILogger<CheckMatchingController> _logger;
    private readonly ICheckMatchingService _checkMatchingService;

    public CheckMatchingController(ILogger<CheckMatchingController> logger, ICheckMatchingService checkMatchingService)
    {
        _logger = logger;
        _checkMatchingService = checkMatchingService;
    }

    [HttpPost]
    public async Task<MatchCompleteResponse> IsMatched([FromBody] MatchRequest request) // TODO : IsMatched() 함수 이름 바꾸기
    {
        return await _checkMatchingService.IsMatched(request);
    }
}
// Redis에서 Player의 매칭 결과 확인
// 없으면 아직 매칭 중이라고 클라이언트에게 통보

// 있으면
// 매칭 데이터를 가져와서 이 사람의 게임 데이터를 레디스에 만들어주기
// 게임 데이터는 매칭 당시 시간과 GameRoomId