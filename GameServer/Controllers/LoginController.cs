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
    public async Task<LoginResponse> Login([FromBody] LoginRequest request)
    {
        try
        {
            var verifyTokenRequest = new VerifyTokenRequest
            {
                hive_player_id = request.PlayerId,
                hive_token = request.Token
            };

            var (result, responseBody) = await _loginService.VerifyTokenAsync(verifyTokenRequest);
            _logger.LogInformation("Received response with status {Result}: {ResponseBody}", result, responseBody);

            if (result != ErrorCode.None)
            {
                _logger.LogWarning("Token validation failed with result: {Result}", result);
                return new LoginResponse
                {
                    Result = result
                };
            }

            int validationResult;
            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                JsonElement root = doc.RootElement;
                validationResult = root.GetProperty("result").GetInt32();
            }

            if (validationResult != 0)
            {
                _logger.LogWarning("Token validation failed with result: {Result}", validationResult);
                return new LoginResponse
                {
                    Result = (ErrorCode)validationResult
                };
            }

            var saveResult = await _loginService.SaveLoginInfoAsync(request);
            if (saveResult != ErrorCode.None)
            {
                return new LoginResponse { Result = saveResult };
            }

            var initializeResult = await _loginService.InitializeUserDataAsync(request.PlayerId);
            if (initializeResult != ErrorCode.None)
            {
                return new LoginResponse { Result = initializeResult };
            }

            _logger.LogInformation("Successfully authenticated user with token");

            return new LoginResponse
            {
                Result = ErrorCode.None
            };
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "HTTP request to token validation service failed.");
            return new LoginResponse { Result = ErrorCode.ServerError };
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error parsing JSON from token validation service.");
            return new LoginResponse { Result = ErrorCode.JsonParsingError };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred during login.");
            return new LoginResponse { Result = ErrorCode.InternalError };
        }

        //var response = await _loginService.Login(request);
        //_logger.LogInformation($"[Login] PlayerId: {request.PlayerId}, Result: {response.Result}");
        //return response;
    }
}
