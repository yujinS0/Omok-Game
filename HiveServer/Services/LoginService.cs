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

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var (error, hivePlayerId) = await _hiveDb.VerifyUser(request.HivePlayerId, request.HivePlayerPw);
        if (error != ErrorCode.None)
        {
            return new LoginResponse { Result = error };
        }

        var token = Security.MakeHashingToken(_saltValue, hivePlayerId);
        var tokenSet = await _hiveDb.SaveToken(hivePlayerId, token);

        if (!tokenSet)
        {
            return new LoginResponse { Result = ErrorCode.InternalError };
        }

        return new LoginResponse { HivePlayerId = hivePlayerId, HiveToken = token, Result = ErrorCode.None };
    }
}
