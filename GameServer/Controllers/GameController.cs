using GameServer.DTO;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    //[HttpPost("WaitForTurnChange")]
    //public async Task<IActionResult> WaitForTurnChange([FromBody] PlayerRequest request, int timeoutSeconds = 30)
    //{
    //    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
    //    try
    //    {
    //        var gameInfo = await _gameService.WaitForTurnChangeAsync(request.PlayerId, cts.Token);
    //        return Ok(gameInfo);
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        return StatusCode(StatusCodes.Status204NoContent); // 타임아웃
    //    }
    //}

    //[HttpPost("CheckTurn")]
    //public async Task<IActionResult> CheckTurn([FromBody] PlayerRequest request)
    //{
    //    var currentTurn = await _gameService.CheckTurnAsync(request.PlayerId);
    //    return Ok(new { CurrentTurn = currentTurn });
    //}
}
