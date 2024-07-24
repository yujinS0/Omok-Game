using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using GameServer.Models;
using GameServer.Repository;
using System.Text.Json;
using ServerShared;

namespace GameServer.Middleware;

public class CheckUserAuth
{
    private readonly RequestDelegate _next;
    private readonly IMemoryDb _memoryDb;

    public CheckUserAuth(RequestDelegate next, IMemoryDb memoryDb)
    {
        _next = next;
        _memoryDb = memoryDb;
    }

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
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.MissingHeader
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return;
        }

        var redisToken = await _memoryDb.GetUserLoginTokenAsync(playerId);

        if (string.IsNullOrEmpty(redisToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.AuthTokenKeyNotFound
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return;
        }

        if (token != redisToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.AuthTokenFailWrongAuthToken
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return;
        }

        // TODO 미들웨어에 락 추가
        // 락 설정
        var userLockKey = $"user_lock:{playerId}";
        if (!await _memoryDb.SetUserReqLockAsync(userLockKey)) // TODO keyGen 사용하기
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.AuthTokenFailSetNx
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return;
        }

        // 락 해제
        await _memoryDb.DelUserReqLockAsync(userLockKey);


        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }

    class MiddlewareResponse
    {
        public ErrorCode Result { get; set; }
    }
}
