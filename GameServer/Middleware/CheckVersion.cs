﻿using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ServerShared;

namespace GameServer.Middleware;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class CheckVersion
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CheckVersion> _logger;

    public CheckVersion(RequestDelegate next, ILogger<CheckVersion> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var appVersion = httpContext.Request.Headers["AppVersion"].ToString();
        var dataVersion = httpContext.Request.Headers["DataVersion"].ToString();

        if (!(await VersionCompare(appVersion, dataVersion, httpContext)))
        {
            return;
        }

        await _next(httpContext);
    }

    private async Task<bool> VersionCompare(string appVersion, string dataVersion, HttpContext context)
    {
        if (!appVersion.Equals("0.1.0")) // TODO 이게 맞나
        {
            context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.InvalidAppVersion
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return false;
        }

        if (!dataVersion.Equals("0.1.0"))
        {
            context.Response.StatusCode = StatusCodes.Status426UpgradeRequired;
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = ErrorCode.InvalidDataVersion
            });
            await context.Response.WriteAsync(errorJsonResponse);
            return false;
        }

        return true;
    }

    private class MiddlewareResponse
    {
        public ErrorCode Result { get; set; }
    }
}

//// Extension method used to add the middleware to the HTTP request pipeline.
//public static class CheckVersionExtensions
//{
//    public static IApplicationBuilder UseCheckVersion(this IApplicationBuilder builder)
//    {
//        return builder.UseMiddleware<CheckVersion>();
//    }
//}