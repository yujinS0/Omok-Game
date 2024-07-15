﻿using System.ComponentModel.DataAnnotations;

namespace MatchServer.DTO;

public class MatchRequest
{
    [Required] public string PlayerId { get; set; }
}

public class MatchResponse
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
}

public class MatchCompleteResponse
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public int Success { get; set; } // 매칭 성공하면 1
    [Required] public string PlayerId { get; set; }
    [Required] public int RoomNum { get; set; }
}
public class MatchCancelResponse
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    [Required] public string Message { get; set; }
}

public class RoomInfo
{
    [Required] public int RoomNumber { get; set; }

    public RoomInfo()
    {
    }

    public RoomInfo(int roomNumber)
    {
        RoomNumber = roomNumber;
    }
}
