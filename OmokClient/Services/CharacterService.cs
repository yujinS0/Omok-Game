using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using AntDesign;
using Blazored.SessionStorage;
using System.Reflection;

namespace OmokClient.Services;

public class CharacterService : BaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CharacterService(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage)
            : base(httpClientFactory, sessionStorage) { }

    public async Task<string> GetCharacterNameAsync(string playerId)
    {
        var response = await GetCharacterInfoAsync(playerId);
        if (response != null && response.Error == ErrorCode.None)
        {
            return response.CharacterInfo.CharName;
        }
        return null;
    }

    public async Task<CharacterInfoResponse> GetCharacterInfoAsync(string playerId)
    {
        var client = await CreateClientWithHeadersAsync("GameAPI");
        var response = await client.PostAsJsonAsync("Character/getinfo", new CharacterInfoRequest { PlayerId = playerId });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CharacterInfoResponse>();
        }
        else
        {
            return new CharacterInfoResponse { Error = ErrorCode.InternalServerError };
        }
    }

    public async Task<UpdateCharacterNameResponse> UpdateCharacterNameAsync(string playerId, string newCharName)
    {
        var client = await CreateClientWithHeadersAsync("GameAPI");
        var response = await client.PostAsJsonAsync("Character/updatename", new UpdateCharacterNameRequest { PlayerId = playerId, CharName = newCharName });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UpdateCharacterNameResponse>();
        }
        else
        {
            return new UpdateCharacterNameResponse { Error = ErrorCode.InternalServerError };
        }
    }
}


public class UpdateCharacterNameRequest
{
    public string PlayerId { get; set; }
    public string CharName { get; set; }
}

public class CharacterInfoRequest
{
    public string PlayerId { get; set; }
}

public class UpdateCharacterNameResponse
{
    public ErrorCode Error { get; set; }
}

public class CharacterInfoResponse
{
    public ErrorCode Error { get; set; }
    public CharacterDetails CharacterInfo { get; set; }
}

public class CharacterDetails  // 클라이언트에게 제공할 정보만
{
    public string CharName { get; set; }
    public int CharExp { get; set; }
    public int CharLevel { get; set; }
    public int CharWin { get; set; }
    public int CharLose { get; set; }
    public int CharDraw { get; set; }
}
