using GameServer.Models;
using ServerShared;

namespace GameServer.DTO;


public class GetPlayerMailBoxRequest
{
    public string PlayerId { get; set; }
    public int PageNum { get; set; }
}

public class MailBoxResponse
{
    public ErrorCode Result { get; set; }
    public List<int> MailIds { get; set; }
    public List<string> Titles { get; set; }
    public List<int> ItemCodes { get; set; }
    public List<DateTime> SendDates { get; set; }
    public List<long> ExpiryDurations { get; set; }
    public List<bool> ReceiveYns { get; set; }
}
public class MailDetailResponse
{
    public ErrorCode Result { get; set; }
    public int MailId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int ItemCode { get; set; }
    public int ItemCnt { get; set; }
    public long ExpiryDuration { get; set; }
    public bool ReceiveYn { get; set; }
}
