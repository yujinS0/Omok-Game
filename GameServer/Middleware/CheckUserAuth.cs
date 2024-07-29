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

        var redisToken = await _memoryDb.GetUserLoginToken(playerId);

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

        // 락 설정
        var userLockKey = KeyGenerator.UserLockKey(playerId);
        if (!await _memoryDb.SetUserReqLock(userLockKey))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.AuthTokenFailSetNx
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return;
        }


        //TODO: 버그. 락 옵션 해제를 여기서 하면 위에서 락 건 것을 바로 풀어버리게 됩니다
        await _memoryDb.DelUserReqLock(userLockKey); // 락 해제

        await _next(context);
    }

    class MiddlewareResponse
    {
        public ErrorCode Result { get; set; }
    }
}
