using HiveServer.DTO;
using HiveServer.Repository;
using HiveServer.Services.Interfaces;

namespace HiveServer.Services;

public class VerifyTokenService : IVerifyTokenService
{
    private readonly ILogger<VerifyTokenService> _logger;
    private readonly IHiveDb _hiveDb;

    public VerifyTokenService(ILogger<VerifyTokenService> logger, IHiveDb hiveDb)
    {
        _logger = logger;
        _hiveDb = hiveDb;
    }

    public async Task<bool> ValidateTokenAsync(string hivePlayerId, string hiveToken)
    {
        return await _hiveDb.ValidateTokenAsync(hivePlayerId, hiveToken);
    }

    public async Task<VerifyTokenResponse> Verify(VerifyTokenRequest request)
    {
        bool isValid = await ValidateTokenAsync(request.hive_player_id, request.hive_token);

        if (!isValid)
        {
            return new VerifyTokenResponse
            {
                Result = ErrorCode.VerifyTokenFail,
            };
        }

        return new VerifyTokenResponse
        {
            Result = ErrorCode.None,
        };
    }
}
