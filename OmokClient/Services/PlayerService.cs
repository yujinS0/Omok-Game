﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using AntDesign;
using Blazored.SessionStorage;
using System.Reflection;

namespace OmokClient.Services;

public class PlayerService : BaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PlayerService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage)
            : base(httpClientFactory, sessionStorage) { }

    public async Task<string> GetNickNameAsync(string playerId)
    {
        var response = await GetPlayerBasicInfoAsync(playerId);
        if (response != null && response.Result == ErrorCode.None)
        {
            return response.PlayerBasicInfo.NickName;
        }
        return null;
    }

    public async Task<PlayerBasicInfoResponse> GetPlayerBasicInfoAsync(string playerId)
    {
        var client = await CreateClientWithHeadersAsync("GameAPI");
        var response = await client.PostAsJsonAsync("PlayerInfo/basic-player-data", new PlayerBasicInfoRequest { PlayerId = playerId });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PlayerBasicInfoResponse>();
        }
        else
        {
            return new PlayerBasicInfoResponse { Result = ErrorCode.InternalServerError };
        }
    }

    public async Task<UpdateNickNameResponse> UpdateNickNameAsync(string playerId, string newNickName)
    {
        var client = await CreateClientWithHeadersAsync("GameAPI");
        var response = await client.PostAsJsonAsync("PlayerInfo/update-nickname", new UpdateNickNameRequest { PlayerId = playerId, NickName = newNickName });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UpdateNickNameResponse>();
        }
        else
        {
            return new UpdateNickNameResponse { Result = ErrorCode.InternalServerError };
        }
    }
}


// Player DTO
public class PlayerBasicInfoRequest
{
    public string PlayerId { get; set; }
}

public class PlayerBasicInfoResponse
{
    public ErrorCode Result { get; set; }
    public PlayerBasicInfo PlayerBasicInfo { get; set; }
}

public class PlayerBasicInfo
{
    public string NickName { get; set; }
    public int Exp { get; set; }
    public int Level { get; set; }
    public int Win { get; set; }
    public int Lose { get; set; }
    public int Draw { get; set; }
}

public class UpdateNickNameRequest
{
    public string PlayerId { get; set; }
    public string NickName { get; set; }
}

public class UpdateNickNameResponse
{
    public ErrorCode Result { get; set; }
}