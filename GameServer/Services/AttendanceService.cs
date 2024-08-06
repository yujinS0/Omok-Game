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

    public async Task<(ErrorCode, AttendanceInfo?)> GetAttendanceInfo(string playerId)
    {
        var attendanceInfo = await _gameDb.GetAttendanceInfo(playerId);

        if (attendanceInfo == null)
        {
            return (ErrorCode.AttendanceInfoNotFound, null);
        }
        return (ErrorCode.None, attendanceInfo);
    }

    public async Task<ErrorCode> AttendanceCheck(string playerId)
    {
        // 최근 출석 일시 가져오기
        var result = await _gameDb.GetCurrentAttendanceDate(playerId);
            // 만약 가져온 값이 오늘이라면, return ErroCode.AttendanceCheckFailAlreadyChecked

        // 출석 정보 업데이트 후
        // 출석 횟수 가져와서
        // 보상 테이블에 추가하기
            // 이때 실패시 롤백 생각해야 함
        

        if (!result)
        {
            return ErrorCode.AttendanceCheckFailException;
        }

        return ErrorCode.None;
    }
    
}