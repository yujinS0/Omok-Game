using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using GameServer.Services.Interfaces;
using ServerShared;
using StackExchange.Redis;

namespace GameServer.Services;

public class CharacterService : ICharacterService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CharacterService> _logger;
    private readonly IGameDb _gameDb;

    public CharacterService(IHttpClientFactory httpClientFactory, ILogger<CharacterService> logger, IGameDb gameDb)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _gameDb = gameDb;
    }

    public async Task<(ErrorCode, CharInfoDTO?)> GetCharInfoSummaryAsync(string playerId)
    {
        var charInfo = await _gameDb.GetCharInfoSummaryAsync(playerId);

        if (charInfo == null)
        {
            return (ErrorCode.CharacterNotFound, null);
        }
        return (ErrorCode.None, charInfo);
    }

    public async Task<ErrorCode> UpdateCharacterNameAsync(string playerId, string newCharName)
    {
        var result = await _gameDb.UpdateCharacterNameAsync(playerId, newCharName);

        if (!result)
        {
            return ErrorCode.UpdateCharacterNameFailed;
        }

        return ErrorCode.None;
    }
}