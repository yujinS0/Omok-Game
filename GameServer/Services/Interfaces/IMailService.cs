using GameServer.DTO;
using GameServer.Models;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IMailService
{
    Task<(ErrorCode, List<Int64>, List<string>, List<int>, List<DateTime>, List<long>, List<int>)> GetPlayerMailBox(string playerId, int pageNum);
    Task<(ErrorCode, MailDetail)> ReadMail(string playerId, Int64 mailId);
    Task<(ErrorCode, int?)> ReceiveMailItem(string playerId, Int64 mailId);
    Task<ErrorCode> DeleteMail(string playerId, Int64 mailId);
}
