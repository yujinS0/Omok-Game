using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Services.Interfaces;
using ServerShared;
using StackExchange.Redis;
using GameServer.Repository.Interfaces;

namespace GameServer.Services;

public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    private readonly IGameDb _gameDb;
    private readonly IMemoryDb _memoryDb;
    private const int PageSize = 15;

    public MailService(ILogger<MailService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }

    public async Task<(ErrorCode, List<int>, List<string>, List<int>, List<DateTime>, List<long>, List<bool>)> GetPlayerMailBox(string playerId, int pageNum)
    {
        var playerUid = await _memoryDb.GetPlayerUid(playerId);
        if (playerUid == -1)
        {
            return (ErrorCode.InValidPlayerUidError, null, null, null, null, null, null);
        }

        int skip = (pageNum - 1) * PageSize;
        var (mailId, title, itemCode, sendDate, expriryDuration, receiveYns) = await _gameDb.GetPlayerMailBox(playerUid, skip, PageSize);
        
        return (ErrorCode.None, mailId, title, itemCode, sendDate, expriryDuration, receiveYns);
    }
}
