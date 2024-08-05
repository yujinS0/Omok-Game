using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using GameServer.Models;
using System.Text.Json;
using ServerShared;
using GameServer.Repository.Interfaces;

namespace GameServer.Middleware;

public class CheckUserAuth
{
    private readonly RequestDelegate _next;
    private readonly IMemoryDb _memoryDb;
    private readonly ILogger<CheckUserAuth> _logger;

    public CheckUserAuth(RequestDelegate next, IMemoryDb memoryDb, ILogger<CheckUserAuth> logger)
    {
        _next = next;
        _memoryDb = memoryDb;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        //TODO: (08.05) 아래 주석처럼 함수를 만드는 것이 가독성에 좋습니다(이름도 비슷하게)
        // 함수를 단순한 기능별로 나누지 말고 컨텍스트별로 분리하세요

        // 플레이어 정보 가져오기 (이 미들웨어를 사용하지 않는 것이라면 안가져옴)

        // 인증 여부 확인

        // 플레이어 요청 lock 걸기

        // 플레이어 요청 lock 풀기


        if (IsLoginOrRegisterRequest(context))
        {
            await _next(context);
            return;
        }
                
        if (!TryGetHeaders(context, out var playerId, out var token))
        {
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ErrorCode.MissingHeader);
            return;
        }

        string bodyPlayerId = await GetBodyPlayerId(context);

        if (IsPlayerIdMismatch(playerId, bodyPlayerId))
        {
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ErrorCode.PlayerIdMismatch);
            return;
        }

        var (playerUid, redisToken) = await _memoryDb.GetPlayerUidAndLoginToken(playerId);

        if (!IsValidToken(token, redisToken))
        {
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, ErrorCode.AuthTokenFailWrongAuthToken);
            return;
        }

        context.Items["PlayerUid"] = playerUid; // HttpContext.Items에 저장

        if (!await SetUserLock(context, playerId))
        {
            await WriteErrorResponse(context, StatusCodes.Status429TooManyRequests, ErrorCode.AuthTokenFailSetNx);
            return;
        }

        await _next(context);

        await ReleaseUserLock(context, playerId);
    }

    private bool IsLoginOrRegisterRequest(HttpContext context)
    {
        var formString = context.Request.Path.Value;
        return string.Compare(formString, "/login", StringComparison.OrdinalIgnoreCase) == 0 ||
               string.Compare(formString, "/register", StringComparison.OrdinalIgnoreCase) == 0;
    }

    private bool TryGetHeaders(HttpContext context, out string playerId, out string token)
    {
        bool hasPlayerId = context.Request.Headers.TryGetValue("PlayerId", out var playerIdHeader);
        bool hasToken = context.Request.Headers.TryGetValue("Token", out var tokenHeader);

        playerId = hasPlayerId ? playerIdHeader.ToString() : null;
        token = hasToken ? tokenHeader.ToString() : null;

        return hasPlayerId && hasToken;
    }

    private async Task<string> GetBodyPlayerId(HttpContext context)
    {
        if (context.Request.ContentLength > 0 && context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                var requestBody = JsonSerializer.Deserialize<JsonElement>(body);
                if (requestBody.TryGetProperty("PlayerId", out var playerIdElement))
                {
                    return playerIdElement.GetString();
                }
            }
        }
        return null;
    }

    private bool IsPlayerIdMismatch(string headerPlayerId, string bodyPlayerId)
    {
        return !string.IsNullOrEmpty(bodyPlayerId) && bodyPlayerId != headerPlayerId;
    }

    private bool IsValidToken(string token, string redisToken)
    {
        return !string.IsNullOrEmpty(redisToken) && token == redisToken;
    }

    private async Task<bool> SetUserLock(HttpContext context, string playerId)
    {
        var userLockKey = KeyGenerator.UserLockKey(playerId);
        if (!await _memoryDb.SetUserReqLock(userLockKey))
        {
            return false;
        }
        return true;
    }

    private async Task ReleaseUserLock(HttpContext context, string playerId)
    {
        var userLockKey = KeyGenerator.UserLockKey(playerId);
        var lockReleaseResult = await _memoryDb.DelUserReqLock(userLockKey);
        if (!lockReleaseResult)
        {
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError, ErrorCode.AuthTokenFailDelNx);
        }
    }

    private async Task WriteErrorResponse(HttpContext context, int statusCode, ErrorCode errorCode)
    {
        context.Response.StatusCode = statusCode;
        var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
        {
            Result = errorCode
        });
        await context.Response.WriteAsync(errorJsonResponse);
    }

    class MiddlewareResponse
    {
        public ErrorCode Result { get; set; }
    }
}
