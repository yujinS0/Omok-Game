using Microsoft.AspNetCore.Mvc;
using HiveServer.Repository;
using HiveServer.DTO;

namespace HiveServer.Controllers;
[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    private readonly IHiveDb _hiveDb;
    private readonly ILogger<RegisterController> _logger; // 로거 인스턴스 추가

    public RegisterController(IHiveDb hiveDb, ILogger<RegisterController> logger)
    {
        _hiveDb = hiveDb;
        _logger = logger;
    }

    [HttpPost]
    public async Task<AccountResponse> Register([FromBody] AccountRequest request) 
    {
        AccountResponse response = new();

        response.Result = await _hiveDb.RegisterAccount(request.hive_player_id, request.hive_player_pw);

        return response;
    }

}