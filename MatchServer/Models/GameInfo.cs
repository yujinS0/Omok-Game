namespace MatchServer.Models;

public class GameInfo
{
    public string PlayerA { get; set; }
    public string PlayerB { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PlayingUserInfo
{
    public string PlayerId { get; set; }
    public string GameRoomId { get; set; }
    public DateTime CreatedAt { get; set; }
}