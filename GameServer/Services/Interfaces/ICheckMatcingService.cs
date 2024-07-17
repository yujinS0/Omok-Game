﻿using System.Threading.Tasks;
using GameServer.DTO;

namespace GameServer.Services.Interfaces;

public interface ICheckMatchingService
{
    Task<MatchCompleteResponse> IsMatched(MatchRequest request);
}