using Microsoft.AspNetCore.Mvc;
using HiveServer.Model.DTO;
using HiveServer.Repository;

namespace HiveServer.Controllers;

[ApiController]
[Route("[controller]")]
public class VerifyTokenController : ControllerBase
{
    private readonly ILogger<VerifyTokenController> _logger;
    private readonly IHiveDb _hiveDb;

    public VerifyTokenController(ILogger<VerifyTokenController> logger, IHiveDb hiveDb)
    {
        _logger = logger;
        _hiveDb = hiveDb;
    }

    [HttpPost]
    public async Task<VerifyTokenResponse> Verify([FromBody] VerifyTokenRequest request)
    {
        bool isValid = await _hiveDb.ValidateTokenAsync(request.hive_player_id, request.hive_token);

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
