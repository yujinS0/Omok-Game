using Microsoft.AspNetCore.Mvc;
using GameServer.Services.Interfaces;
using GameServer.DTO;
using ServerShared;
using System.Text.Json;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly ILoginService _loginService;

    public LoginController(ILogger<LoginController> logger, ILoginService loginService)
    {
        _logger = logger;
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<GameLoginResponse> Login([FromBody] GameLoginRequest request)
    {
        try
        {
            // Verify Token
            var verifyTokenRequest = new VerifyTokenRequest
            {
                HivePlayerId = request.PlayerId,
                HiveToken = request.Token
            };

            var result = await _loginService.VerifyTokenAndInitializePlayerDataAsync(verifyTokenRequest, request);
            return new GameLoginResponse { Result = result };
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "HTTP request to token validation service failed.");
            return new GameLoginResponse { Result = ErrorCode.ServerError };
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error parsing JSON from token validation service.");
            return new GameLoginResponse { Result = ErrorCode.JsonParsingError };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred during login.");
            return new GameLoginResponse { Result = ErrorCode.InternalError };
        }
    }
}
