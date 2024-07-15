using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using GameServer.Services.Interfaces;

namespace GameServer.Services;

public class LoginService : ILoginService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<LoginService> _logger;
    private readonly IGameDb _gameDb;

    public LoginService(IHttpClientFactory httpClientFactory, ILogger<LoginService> logger, IGameDb gameDb)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _gameDb = gameDb;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var client = _httpClientFactory.CreateClient();

        var verifyTokenRequest = new VerifyTokenRequest
        {
            hive_player_id = request.player_id,
            hive_token = request.token
        };

        var content = new StringContent(JsonSerializer.Serialize(verifyTokenRequest), Encoding.UTF8, "application/json");
        _logger.LogInformation("Sending token validation request to external API with content: {Content}", await content.ReadAsStringAsync());

        try
        {
            var response = await client.PostAsync("http://localhost:5284/VerifyToken", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received response with status {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);

            int result;
            using (JsonDocument doc = JsonDocument.Parse(responseBody))
            {
                JsonElement root = doc.RootElement;
                result = root.GetProperty("result").GetInt32();
            }

            _logger.LogInformation("Validation result from API: {Result}", result);

            if (result != 0)
            {
                _logger.LogWarning("Token validation failed with result: {Result}", result);
                return new LoginResponse
                {
                    Result = (ErrorCode)result
                };
            }

            _logger.LogInformation("Successfully authenticated user with token");

            var charInfo = await _gameDb.GetUserGameDataAsync(request.player_id);
            if (charInfo == null)
            {
                _logger.LogInformation("First login detected, creating new char_info for hive_player_id: {PlayerId}", request.player_id);
                charInfo = await _gameDb.CreateUserGameDataAsync(request.player_id);
            }

            return new LoginResponse
            {
                Result = ErrorCode.None,
                //Token = gameToken,
                //Uid = request.UserNum,
                //UserGameData = charInfo
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
    }
}
