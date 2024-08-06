﻿using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository;
using ServerShared;

namespace GameServer.Services.Interfaces;

public interface IAttendanceService
{
    Task<(ErrorCode, AttendanceInfo?)> GetAttendanceInfo(string playerId);
    Task<ErrorCode> AttendanceCheck(string playerId);
}
