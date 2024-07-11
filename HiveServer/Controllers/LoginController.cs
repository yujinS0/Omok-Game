using Microsoft.AspNetCore.Mvc;
using HiveServer.Services;
using HiveServer.Repository;
using HiveServer.DTO;

namespace HiveServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly IHiveDb _hiveDb;
    string saltValue = "Com2usSalt";


    public LoginController(ILogger<LoginController> logger, IHiveDb hiveDb, IConfiguration config)
    {
        _logger = logger;
        _hiveDb = hiveDb;
    }

    [HttpPost]
    public async Task<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var (error, hive_player_id) = await _hiveDb.VerifyUser(request.hive_player_id, request.hive_player_pw);
        if (error != ErrorCode.None)  // 에러 코드가 None이 아닐 때 로그인 실패 처리
        {
            return new LoginResponse { Result = error }; // 에러 코드를 포함한 응답
        }

        var token = Security.MakeHashingToken(saltValue, hive_player_id);
        bool tokenSet = await _hiveDb.SaveToken(hive_player_id, token);

        if (!tokenSet)
        {
            return new LoginResponse { Result = ErrorCode.InternalError };
        }

        return new LoginResponse { hive_player_id = hive_player_id, hive_token = token, Result = ErrorCode.None }; // 성공 응답
    }

}