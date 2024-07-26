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
                HiveUserId = request.PlayerId,
                HiveToken = request.Token
            };

            //TODO: 이름이 login 이라고 하는 것이 좋을 것 같습니다. 이 함수 안에서 하이브에서 토큰을 검증하고, 플레이어 데이터를 초기화하는 것이기 때문입니다.
            // VerifyTokenAndInitializePlayerDataAsync은 너무 이름이 구체화 되어서 로그인 과정에 변경이 발생하면 이 이름도 바뀌어야 해서 유연하지 않습니다
            //=> 수정 완료했습니다.
            var result = await _loginService.login(request.PlayerId, request.Token, request.AppVersion, request.DataVersion);
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
