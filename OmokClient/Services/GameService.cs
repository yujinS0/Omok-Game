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

public class GameService : BaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GameService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage)
            : base(httpClientFactory, sessionStorage) { }

    public async Task<PutOmokResponse> PutStoneAsync(string playerId, int x, int y)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        var response = await gameClient.PostAsJsonAsync("PutOmok", new { PlayerId = playerId, X = x, Y = y });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PutOmokResponse>();
            return result;
        }
        return new PutOmokResponse { Result = ErrorCode.RequestFailed };
    }

    public async Task<byte[]> GetBoardAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        Console.WriteLine($"Sending request to GetGameInfo/board for PlayerId: {playerId}");

        var response = await gameClient.PostAsJsonAsync("GetGameInfo/board", new { PlayerId = playerId });
        Console.WriteLine($"Response status code: {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<BoardResponse>();
            if (result != null)
            {
                Console.WriteLine($"Received board data. Result: {result.Result}, RawData Length: {result.Board?.Length}");
            }
            else
            {
                Console.WriteLine("Received null result.");
            }

            if (result?.Board != null)
            {
                var decodedData = Convert.FromBase64String(result.Board);
                Console.WriteLine($"Decoded raw data length: {decodedData.Length}");
                Console.WriteLine($"Decoded raw data: {BitConverter.ToString(decodedData)}");
                return decodedData;
            }
        }
        else
        {
            Console.WriteLine("Failed to get board data.");
        }

        return null;
    }

    public async Task<string> GetBlackPlayerAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        var response = await gameClient.PostAsJsonAsync("GetGameInfo/black", new { PlayerId = playerId });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PlayerResponse>();
            return result?.PlayerId ?? string.Empty;
        }
        return string.Empty;
    }

    public async Task<string> GetWhitePlayerAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        var response = await gameClient.PostAsJsonAsync("GetGameInfo/white", new { PlayerId = playerId });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PlayerResponse>();
            return result?.PlayerId ?? string.Empty;
        }
        return string.Empty;
    }

    public async Task<string> GetCurrentTurnAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");

        var response = await gameClient.PostAsJsonAsync("GetGameInfo/turn", new { PlayerId = playerId });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CurrentTurnResponse>();
            return result?.CurrentTurn.ToString().ToLower() ?? "none";
        }
        return "none";
    }

    // Long Polling
    public async Task<WaitForTurnChangeResponse> WaitForTurnChangeAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");
        var response = await gameClient.PostAsJsonAsync("GetGameInfo/WaitForTurnChange", new { PlayerId = playerId });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<WaitForTurnChangeResponse>();
            return result;
        }
        return new WaitForTurnChangeResponse
        {
            Result = ErrorCode.RequestFailed,
            GameInfo = null
        };
    }

    public async Task<string> CheckTurnAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");
        var response = await gameClient.PostAsJsonAsync("GetGameInfo/turnplayer", new { PlayerId = playerId });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PlayerResponse>();
            return result.PlayerId;
        }
        return null;
    }

    public async Task<WinnerResponse> GetWinnerAsync(string playerId)
    {
        var gameClient = await CreateClientWithHeadersAsync("GameAPI");
        var response = await gameClient.PostAsJsonAsync("GetGameInfo/winner", new { PlayerId = playerId });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<WinnerResponse>();
            return result;
        }
        return new WinnerResponse
        {
            Result = ErrorCode.RequestFailed,
            Winner = null
        };
    }


}



public class GameInfo
{
    public byte[] Board { get; set; }
    public OmokStone CurrentTurn { get; set; }
}


public class BoardResponse
{
    public ErrorCode Result { get; set; }
    public string Board { get; set; }
}

public class PlayerResponse
{
    public ErrorCode Result { get; set; }
    public string PlayerId { get; set; }
}

public class CurrentTurnResponse
{
    public ErrorCode Result { get; set; }
    public OmokStone CurrentTurn { get; set; }
}

public class PutOmokResponse
{
    public ErrorCode Result { get; set; }
    public Winner Winner { get; set; }
}
public class CheckTurnResponse
{
    public ErrorCode Result { get; set; }
}

public class WinnerResponse
{
    public ErrorCode Result { get; set; }
    public Winner Winner { get; set; }
}

public class Winner
{
    public OmokStone Stone { get; set; }
    public string PlayerId { get; set; }
}

public class WaitForTurnChangeResponse
{
    public ErrorCode Result { get; set; }
    public GameInfo GameInfo { get; set; }
}