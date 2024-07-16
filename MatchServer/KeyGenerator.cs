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

    public static string GenerateRoomId()
    {
        return Guid.NewGuid().ToString(); // [TODO] Ulid 확인 후, 바꾸기
    }
}
