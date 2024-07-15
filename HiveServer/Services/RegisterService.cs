using HiveServer.DTO;
using HiveServer.Repository;
using HiveServer.Services.Interfaces;

namespace HiveServer.Services;

public class RegisterService : IRegisterService
{
    private readonly ILogger<RegisterService> _logger;
    private readonly IHiveDb _hiveDb;

    public RegisterService(ILogger<RegisterService> logger, IHiveDb hiveDb)
    {
        _logger = logger;
        _hiveDb = hiveDb;
    }

    public async Task<ErrorCode> RegisterAccount(string hivePlayerId, string hivePlayerPw)
    {
        return await _hiveDb.RegisterAccount(hivePlayerId, hivePlayerPw);
    }

    public async Task<AccountResponse> Register(AccountRequest request)
    {
        AccountResponse response = new();
        response.Result = await RegisterAccount(request.hive_player_id, request.hive_player_pw);
        return response;
    }
}
