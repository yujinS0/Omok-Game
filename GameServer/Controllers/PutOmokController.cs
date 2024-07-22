using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GameServer.DTO;
using GameServer.Services.Interfaces;
using ServerShared;
using GameServer.Repository;
using GameServer.Models;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class PutOmokController : ControllerBase
{
    private readonly ILogger<PutOmokController> _logger;
    private readonly IGameService _gameService;

    public PutOmokController(ILogger<PutOmokController> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [HttpPost]
    public async Task<PutOmokResponse> PutOmok([FromBody] PutOmokRequest request)
    {
        var (result, winner) = await _gameService.PutOmokAsync(request);

        if (result != ErrorCode.None)
        {
            _logger.LogError($"[PutOmok] PlayerId: {request.PlayerId}, ErrorCode: {result}");
        }

        return new PutOmokResponse { Result = result, Winner = winner };
    }
}