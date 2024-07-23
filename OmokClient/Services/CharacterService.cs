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
        if (response != null && response.Result == ErrorCode.None)
        {
            return response.CharInfoDTO.CharName;
        }
        return null;
    }

    public async Task<CharacterInfoDTOResponse> GetCharacterInfoAsync(string playerId)
    {
        var client = await CreateClientWithHeadersAsync("GameAPI");
        var response = await client.PostAsJsonAsync("Character/getinfo", new CharacterInfoRequest { PlayerId = playerId });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CharacterInfoDTOResponse>();
        }
        else
        {
            return new CharacterInfoDTOResponse { Result = ErrorCode.InternalServerError };
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
            return new UpdateCharacterNameResponse { Result = ErrorCode.InternalServerError };
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
    public ErrorCode Result { get; set; }
}


public class CharacterInfoDTOResponse
{
    public ErrorCode Result { get; set; }
    public CharInfoDTO CharInfoDTO { get; set; }
}


public class CharInfoDTO
{
    public string CharName { get; set; }
    public int Exp { get; set; }
    public int Level { get; set; }
    public int Win { get; set; }
    public int Lose { get; set; }
    public int Draw { get; set; }
}
