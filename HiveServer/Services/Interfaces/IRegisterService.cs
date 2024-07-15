using HiveServer.DTO;

namespace HiveServer.Services.Interfaces;

public interface IRegisterService
{
    Task<ErrorCode> RegisterAccount(string hivePlayerId, string hivePlayerPw);
    Task<AccountResponse> Register(AccountRequest request);
}
