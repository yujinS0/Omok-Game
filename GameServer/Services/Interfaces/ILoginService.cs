using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface ILoginService
{
    Task<ErrorCode> VerifyTokenAndInitializePlayerDataAsync(VerifyTokenRequest verifyTokenRequest, GameLoginRequest request);
}
