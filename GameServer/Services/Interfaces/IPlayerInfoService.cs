using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IPlayerInfoService
{
    Task<(ErrorCode, PlayerBasicInfo?)> GetPlayerBasicDataAsync(string playerId);
    Task<ErrorCode> UpdateNickNameAsync(string playerId, string newNickName);
}
