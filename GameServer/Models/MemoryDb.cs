namespace GameServer.Models;

public class PlayerLoginInfo
{
    public string Token { get; set; }
    public string AppVersion { get; set; }
    public string DataVersion { get; set; }
}
public class UserGameData
{
    public string GameRoomId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MatchResult
{
    public string GameRoomId { get; set; }
    public string Opponent { get; set; }
}