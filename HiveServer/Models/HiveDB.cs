namespace HiveServer.Models;
public class HdbAccount
{
    public long AccountUid { get; set; }
    public required string HivePlayerId { get; set; } // email
    public required string HivePlayerPw { get; set; }
}