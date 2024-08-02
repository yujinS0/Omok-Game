using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IMailService
{
    Task<(ErrorCode, List<Int64>, List<string>, List<int>, List<DateTime>, List<long>, List<int>)> GetPlayerMailBox(Int64 playerUid, int pageNum);
    Task<(ErrorCode, MailDetail)> ReadMail(Int64 playerUid, Int64 mailId);
    Task<(ErrorCode, int?)> ReceiveMailItem(Int64 playerUid, Int64 mailId);
    Task<ErrorCode> DeleteMail(Int64 playerUid, Int64 mailId);
}
