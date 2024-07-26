﻿using HiveServer.DTO;
using HiveServer.Repository;
using HiveServer.Services.Interfaces;

namespace HiveServer.Services;

public class LoginService : ILoginService
{
    private readonly ILogger<LoginService> _logger;
    private readonly IHiveDb _hiveDb;
    private readonly string _saltValue = "Com2usSalt";

    public LoginService(ILogger<LoginService> logger, IHiveDb hiveDb)
    {
        _logger = logger;
        _hiveDb = hiveDb;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var (error, hiveUserId) = await _hiveDb.VerifyUser(request.HiveUserId, request.HiveUserPw);
        if (error != ErrorCode.None)
        {
            return new LoginResponse { Result = error };
        }

        var token = Security.MakeHashingToken(_saltValue, hiveUserId);
        var tokenSet = await _hiveDb.SaveToken(hiveUserId, token);

        if (!tokenSet)
        {
            return new LoginResponse { Result = ErrorCode.InternalError };
        }

        return new LoginResponse { HiveUserId = hiveUserId, HiveToken = token, Result = ErrorCode.None };
    }
}
