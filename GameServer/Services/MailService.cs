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

    public async Task<(ErrorCode, List<Int64>, List<string>, List<int>, List<DateTime>, List<long>, List<int>)> GetPlayerMailBox(Int64 playerUid, int pageNum)
    {
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

        var (errorCode, mailDetail) = await ReadMail(playerUid, mailId);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, null);
        }

        if (mailDetail.ReceiveYn == 1)
        {
            return (ErrorCode.None, mailDetail.ReceiveYn);
        }

        await _gameDb.UpdateMailReceiveStatus(playerUid, mailId);
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
