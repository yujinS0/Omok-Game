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

    //TODO: 서비스에서 AccountResponse를 반환하지 않도록 해주세요 
    //=> 수정 완료했습니다.
    public async Task<ErrorCode> Register(string hiveUserId, string hiveUserPw)
    {
        AccountResponse response = new();
        ErrorCode result = await _hiveDb.RegisterAccount(hiveUserId, hiveUserPw);
        return result;
    }
}
