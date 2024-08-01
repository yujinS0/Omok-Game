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

    //TODO: (07.31) 함수가 너무 길어서 가독성이 떨어집니다. 함수를 쪼개서 가독성을 높이세요.
    public async Task Invoke(HttpContext context)
    {
        //로그인, 회원가입 api는 토큰 검사를 하지 않는다.
        var formString = context.Request.Path.Value;
        if (string.Compare(formString, "/login", StringComparison.OrdinalIgnoreCase) == 0 ||
            string.Compare(formString, "/register", StringComparison.OrdinalIgnoreCase) == 0)
        {
            // Call the next delegate/middleware in the pipeline
            await _next(context);

            return;
        }

        // header에서 playerId & token 가져오기
        if (!context.Request.Headers.TryGetValue("PlayerId", out var playerId) ||
            !context.Request.Headers.TryGetValue("Token", out var token))
        {
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ErrorCode.MissingHeader);
            return;
        }

        // Body에서 PlayerId 가져오기
        string bodyPlayerId = null;

        if (context.Request.ContentLength > 0 && context.Request.ContentType != null && context.Request.ContentType.Contains("application/json"))
        {
            context.Request.EnableBuffering(); // Allow multiple reads of the body
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                var requestBody = JsonSerializer.Deserialize<JsonElement>(body);
                if (requestBody.TryGetProperty("PlayerId", out var playerIdElement))
                {
                    bodyPlayerId = playerIdElement.GetString();
                }
            }
        }

        // Body의 PlayerId와 Header의 PlayerId가 일치하는지 확인
        if (!string.IsNullOrEmpty(bodyPlayerId) && bodyPlayerId != playerId)
        {
            Console.WriteLine($"Mismatch between Header PlayerId ({playerId}) and Body PlayerId ({bodyPlayerId})"); // 로그 추가
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ErrorCode.PlayerIdMismatch);
            return;
        }

        var (playerUid, redisToken) = await _memoryDb.GetPlayerUidAndLoginToken(playerId);

        if (string.IsNullOrEmpty(redisToken))
        {
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, ErrorCode.AuthTokenKeyNotFound);
            return;
        }

        if (token != redisToken)
        {
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, ErrorCode.AuthTokenFailWrongAuthToken);
            return;
        }

        context.Items["PlayerUid"] = playerUid; // playerUid를 HttpContext.Items에 저장

        // 락 설정
        var userLockKey = KeyGenerator.UserLockKey(playerId);
        if (!await _memoryDb.SetUserReqLock(userLockKey))
        {
            await WriteErrorResponse(context, StatusCodes.Status429TooManyRequests, ErrorCode.AuthTokenFailSetNx);
            return;
        }

        await _next(context);

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
