using System.Net.Http;
using System.Text.Json;
using System.Text;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Services.Interfaces;
using ServerShared;
using StackExchange.Redis;
using GameServer.Repository.Interfaces;

namespace GameServer.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AttendanceService> _logger;
    private readonly IGameDb _gameDb;

    public AttendanceService(IHttpClientFactory httpClientFactory, ILogger<AttendanceService> logger, IGameDb gameDb)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _gameDb = gameDb;
    }

    public async Task<(ErrorCode, AttendanceInfo?)> GetAttendanceInfo(long playerUid)
    {
        var attendanceInfo = await _gameDb.GetAttendanceInfo(playerUid);

        if (attendanceInfo == null)
        {
            return (ErrorCode.AttendanceInfoNotFound, null);
        }
        return (ErrorCode.None, attendanceInfo);
    }

    public async Task<ErrorCode> AttendanceCheck(long playerUid)
    {
        // 최근 출석 일시 가져오기
        var lastAttendanceDate = await _gameDb.GetCurrentAttendanceDate(playerUid);

        if (lastAttendanceDate.HasValue && lastAttendanceDate.Value.Date == DateTime.Today)
        {
            return ErrorCode.AttendanceCheckFailAlreadyChecked;
        }

        // 트랜잭션 처리
        var result = await _gameDb.ExecuteTransaction(async transaction =>
        {
            // 출석 정보 업데이트
            var updateResult = await _gameDb.UpdateAttendanceInfo(playerUid, transaction);
            if (!updateResult)
            {
                return false;
            }

            // 출석 횟수 가져오기
            var attendanceCount = await _gameDb.GetAttendanceCount(playerUid, transaction);
            if (attendanceCount == -1)
            {
                return false;
            }

            // 보상 아이템 추가
            var rewardResult = await _gameDb.AddAttendanceRewardToPlayer(playerUid, attendanceCount, transaction);
            if (!rewardResult)
            {
                return false;
            }

            return true;
        });

        if (!result)
        {
            return ErrorCode.AttendanceCheckFailException;
        }

        return ErrorCode.None;
    }
    
}