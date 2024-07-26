namespace HiveServer.Models;
public class HdbAccount
{
    public long AccountUid { get; set; }
    public required string HiveUserId { get; set; } // email
    public required string HiveUserPw { get; set; }
}