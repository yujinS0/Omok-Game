namespace GameServer.Models;

public class PlayerInfo
{
    public int PlayerUid { get; set; }
    public string PlayerId { get; set; }
    public string NickName { get; set; }
    public int Exp { get; set; }
    public int Level { get; set; }
    public int Win { get; set; }
    public int Lose { get; set; }
    public int Draw { get; set; }
}

public class MailBoxList
{
    public List<long> MailIds { get; set; }
    public List<string> MailTitles { get; set; }
    public List<int> ItemCodes { get; set; }
    public List<DateTime> SendDates { get; set; }
    public List<int> ReceiveYns { get; set; }

    public MailBoxList()
    {
        MailIds = new List<long>();
        MailTitles = new List<string>();
        ItemCodes = new List<int>();
        SendDates = new List<DateTime>();
        ReceiveYns = new List<int>();
    }
}