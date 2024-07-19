﻿using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.SessionStorage;

namespace OmokClient.Services;

public class MatchingService : BaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MatchingService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage)
            : base(httpClientFactory, sessionStorage) { }

    public async Task<MatchResponse?> RequestMatchingAsync(string playerId)
    {
        var matchRequest = new MatchRequest { PlayerID = playerId };
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        var response = await gameClient.PostAsJsonAsync("RequestMatching", matchRequest);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<MatchResponse>();
        }
        return null;
    }

    public async Task<MatchResponse?> CheckMatchingAsync(string playerId)
    {
        var checkRequest = new MatchRequest { PlayerID = playerId };
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        var response = await gameClient.PostAsJsonAsync("CheckMatching", checkRequest);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<MatchResponse>();
        }
        return null;
    }
}

public class MatchRequest
{
    public string PlayerID { get; set; }
}

public class MatchResponse
{
    public ErrorCode Result { get; set; }
    public int Success { get; set; }
}
