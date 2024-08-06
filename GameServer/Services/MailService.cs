using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Services.Interfaces;
using ServerShared;
using StackExchange.Redis;
using GameServer.Repository.Interfaces;
using System;

namespace GameServer.Services;

public class MailService : IMailService
{
    private readonly ILogger<MailService> _logger;
    private readonly IGameDb _gameDb;
    private readonly IMemoryDb _memoryDb;
    private const int PageSize = 15;

    public MailService(ILogger<MailService> logger, IGameDb gameDb, IMemoryDb memoryDb)
    {
        _logger = logger;
        _gameDb = gameDb;
        _memoryDb = memoryDb;
    }

    //TODO: (08.05) 이렇게 반환 개수가 많으면 클래스를 정의해서 반환하세요
    //=> 수정 완료했습니다.
    public async Task<(ErrorCode, MailBoxList)> GetPlayerMailBoxList(Int64 playerUid, int pageNum)
    {
        //TODO: (08.05) playerUid == -1 는 할필요가 없습니다. 이것이 오류라면 미들웨어도 문제이니
        //=> 수정 완료했습니다.
        try
        {
            int skip = (pageNum - 1) * PageSize; // SYJ 페이징할 때 고려해봐야함!
            MailBoxList mailBoxList = await _gameDb.GetPlayerMailBoxList(playerUid, skip, PageSize);

            if (mailBoxList == null || !mailBoxList.MailIds.Any())
            {
                return (ErrorCode.None, new MailBoxList()); // 비어있는 MailBoxList 반환
            }

            return (ErrorCode.None, mailBoxList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching the player's mailbox.");
            return (ErrorCode.GameDatabaseError, null); // 적절한 오류 코드 반환
        }
    }

    public async Task<(ErrorCode, MailDetail)> ReadMail(Int64 playerUid, Int64 mailId)
    {
        
        if (playerUid == -1)
        {
            return (ErrorCode.InValidPlayerUidError, null);
        }

        //TODO: (08.05) 메일을 읽었다면 읽었다는 체크를 해야합니다
        //=> 메일 읽었는지 여부 대신 아이템 수령 여부로 대체되었습니다!

        var mailDetail = await _gameDb.GetMailDetail(playerUid, mailId);
        if (mailDetail == null)
        {
            return (ErrorCode.MailNotFound, null);
        }

        return (ErrorCode.None, mailDetail);
    }

    public async Task<(ErrorCode, int?)> ReceiveMailItem(long playerUid, long mailId)
    {
        //TODO: (08.05) 아직 아이템을 가져 가지 않은지와, 어떤 아이템인지만 알면 되겠네요
        //=> 수정 완료했습니다.
        var (success, receiveYn) = await _gameDb.ReceiveMailItemTransaction(playerUid, mailId); // 트랜잭션 처리 GameDb.cs에서 진행중..

        if (!success)
        {
            return (ErrorCode.GameDatabaseError, null);
        }

        return (ErrorCode.None, receiveYn); 
        //TODO: (08.05) 아아 아이템을 가져갔다고 업데이트할 것 같은데 이 함수가 무엇을 업데이트 하는지 모르겠네요
        //TODO: (08.05) 아이템 가져가기가 실패하면 위의 것도 롤백해야합니다
        //=> 수정 완료했습니다.
    }


    public async Task<ErrorCode> DeleteMail(Int64 playerUid, long mailId)
    {
        if (playerUid == -1)
        {
            return ErrorCode.InValidPlayerUidError;
        }

        var mailDetail = await _gameDb.GetMailDetail(playerUid, mailId);
        if (mailDetail == null)
        {
            return ErrorCode.MailNotFound;
        }

        if (mailDetail.ReceiveYn == 0) // 보상 미수령 상태 확인
        {
            return ErrorCode.FailToDeleteMailItemNotReceived;
        }

        await _gameDb.DeleteMail(playerUid, mailId);

        return ErrorCode.None;
    }

}
