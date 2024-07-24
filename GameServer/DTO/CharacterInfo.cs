using GameServer.Models;
using ServerShared;

namespace GameServer.DTO;

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
