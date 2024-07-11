using Microsoft.AspNetCore.Mvc;
using GameServer.Repository;
using GameServer.DTO;
using GameServer.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GameServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginController> _logger;
        private readonly IGameDb _gameDb;

        public LoginController(IHttpClientFactory httpClientFactory, ILogger<LoginController> logger, IGameDb gameDb)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _gameDb = gameDb;
        }

        [HttpPost]
        public async Task<LoginResponse> Login([FromBody] LoginRequest request)
        {
            var client = _httpClientFactory.CreateClient();

            // VerifyTokenRequest 객체 생성
            var verifyTokenRequest = new VerifyTokenRequest
            {
                hive_player_id = request.player_id,
                hive_token = request.token
            };

            var content = new StringContent(JsonSerializer.Serialize(verifyTokenRequest), Encoding.UTF8, "application/json");
            _logger.LogInformation("Sending token validation request to external API with content: {Content}", content.ReadAsStringAsync().Result);

            try
            {
                var response = await client.PostAsync("http://localhost:5284/VerifyToken", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Received response with status {StatusCode}: {ResponseBody}", response.StatusCode, responseBody);

                // JSON 응답에서 result 값 추출
                int result;
                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    JsonElement root = doc.RootElement;
                    result = root.GetProperty("result").GetInt32();
                }

                _logger.LogInformation("Validation result from API: {Result}", result);

                // result가 0 (None)이 아닌 경우 오류 처리
                if (result != 0)
                {
                    _logger.LogWarning("Token validation failed with result: {Result}", result);
                    return new LoginResponse
                    {
                        Result = (ErrorCode)result
                    };
                }

                // result가 0 (None)인 경우 성공 처리
                _logger.LogInformation("Successfully authenticated user with token");

                // char_info 테이블에서 hive_player_id 확인
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
}
