using HiveServer.DTO;

namespace HiveServer.Services.Interfaces;

public interface IRegisterService
{
    Task<AccountResponse> Register(AccountRequest request);
}
