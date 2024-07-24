using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface ILoginService
{
    Task<(ErrorCode Result, string ResponseBody)> VerifyTokenAsync(VerifyTokenRequest verifyTokenRequest);
    Task<ErrorCode> SaveLoginInfoAsync(GameLoginRequest request);
    Task<ErrorCode> InitializeUserDataAsync(string playerId);
}
