using System;
using System.Collections.Generic;
using System.Text;

namespace ServerShared;

public class KeyGenerator
{
    public static string GenerateMatchResultKey(string playerId)
    {
        return $"M_{playerId}_Result";
    }

    public static string GeneratePlayingUserKey(string playerId)
    {
        return $"P_{playerId}_Info";
    }
    public static string GenerateUserLoginKey(string playerId)
    {
        return $"U_{playerId}_Login";
    }

    public static string GenerateGameRoomId()
    {
        return Guid.NewGuid().ToString(); // [TODO] Ulid 확인 후, 바꾸기
    }
}
