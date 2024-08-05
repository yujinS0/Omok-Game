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
    public async Task<(ErrorCode, List<Int64>, List<string>, List<int>, List<DateTime>, List<long>, List<int>)> GetPlayerMailBox(Int64 playerUid, int pageNum)
    {
        //TODO: (08.05) playerUid == -1 는 할필요가 없습니다. 이것이 오류라면 미들웨어도 문제이니
        if (playerUid == -1)
        {
            return (ErrorCode.InValidPlayerUidError, null, null, null, null, null, null);
        }

        int skip = (pageNum - 1) * PageSize;
        var (mailId, title, itemCode, sendDate, expriryDuration, receiveYns) = await _gameDb.GetPlayerMailBox(playerUid, skip, PageSize);
        
        return (ErrorCode.None, mailId, title, itemCode, sendDate, expriryDuration, receiveYns);
    }

    public async Task<(ErrorCode, MailDetail)> ReadMail(Int64 playerUid, Int64 mailId)
    {
        
        if (playerUid == -1)
        {
            return (ErrorCode.InValidPlayerUidError, null);
        }

        //TODO: (08.05) 메일을 읽었다면 읽었다는 체크를 해야합니다

        var mailDetail = await _gameDb.GetMailDetail(playerUid, mailId);
        if (mailDetail == null)
        {
            return (ErrorCode.MailNotFound, null);
        }

        return (ErrorCode.None, mailDetail);
    }

    public async Task<(ErrorCode, int?)> ReceiveMailItem(Int64 playerUid, Int64 mailId)
    {
        if (playerUid == -1)
        {
            return (ErrorCode.InValidPlayerUidError, null);
        }

        //TODO: (08.05) 아직 아이템을 가져 가지 않은지와, 어떤 아이템인지만 알면 되겠네요
        var (errorCode, mailDetail) = await ReadMail(playerUid, mailId);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, null);
        }

        if (mailDetail.ReceiveYn == 1)
        {
            return (ErrorCode.None, mailDetail.ReceiveYn);
        }

        //TODO: (08.05) 아아 아이템을 가져갔다고 업데이트할 것 같은데 이 함수가 무엇을 업데이트 하는지 모르겠네요
        await _gameDb.UpdateMailReceiveStatus(playerUid, mailId);

        //TODO: (08.05) 아이템 가져가기가 실패하면 위의 것도 롤백해야합니다
        await _gameDb.AddPlayerItem(playerUid, mailDetail.ItemCode, mailDetail.ItemCnt);

        return (ErrorCode.None, mailDetail.ReceiveYn);
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
