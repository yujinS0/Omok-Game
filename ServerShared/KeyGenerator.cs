using System;
using System.Collections.Generic;
using System.Text;

namespace ServerShared;

public class KeyGenerator
{
    public static string MatchResult(string playerId)
    {
        return $"M_{playerId}_Result";
    }

    public static string PlayingUser(string playerId)
    {
        return $"P_{playerId}_Info";
    }
    public static string UserLogin(string playerId)
    {
        return $"U_{playerId}_Login";
    }

    public static string GameRoomId()
    {
        return Guid.NewGuid().ToString(); // SYJ Ulid 확인 후, 바꾸기
    }
    public static string UserLockKey(string playerId)
    {
        return $"user_lock:{playerId}";
    }
}
