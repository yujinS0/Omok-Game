﻿namespace GameServer.Models;

public class PlayerLoginInfo
{
    public string PlayerId { get; set; } // 이거 없어도 상관 없음
    public string Token { get; set; }
    public string AppVersion { get; set; }
    public string DataVersion { get; set; }
}
public class UserGameData
{
    public string PlayerId { get; set; }
    public string GameRoomId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MatchResult
{
    public string GameRoomId { get; set; }
    public string Opponent { get; set; }
}