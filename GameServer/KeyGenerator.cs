namespace GameServer;

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
    public static string GeneratePlayerLoginKey(string playerId)
    {
        return $"U_{playerId}_Login";
    }
}
