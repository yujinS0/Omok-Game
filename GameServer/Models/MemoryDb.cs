namespace GameServer.Models;

public class PlayerLoginInfo
{
    public string PlayerId { get; set; }
    public string Token { get; set; }
    public string AppVersion { get; set; }
    public string DataVersion { get; set; }
}
