using HiveServer.DTO;
using HiveServer.Repository;
using HiveServer.Services.Interfaces;

namespace HiveServer.Services;

public class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly IHiveDb _hiveDb;
    private readonly string _saltValue = "Com2usSalt";

    public LoginService(ILogger<LoginService> logger, IHiveDb hiveDb)
    {
        _logger = logger;
        _hiveDb = hiveDb;
    }

    public async Task<(ErrorCode, string)> VerifyUser(string hivePlayerId, string hivePlayerPw)
    {
        return await _hiveDb.VerifyUser(hivePlayerId, hivePlayerPw);
    }

    public async Task<bool> SaveToken(string hivePlayerId, string token)
    {
        return await _hiveDb.SaveToken(hivePlayerId, token);
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var (error, hivePlayerId) = await VerifyUser(request.hive_player_id, request.hive_player_pw);
        if (error != ErrorCode.None)
        {
            return new LoginResponse { Result = error };
        }

        var token = Security.MakeHashingToken(_saltValue, hivePlayerId);
        var tokenSet = await SaveToken(hivePlayerId, token);

        if (!tokenSet)
        {
            return new LoginResponse { Result = ErrorCode.InternalError };
        }

        return new LoginResponse { hive_player_id = hivePlayerId, hive_token = token, Result = ErrorCode.None };
    }
}
