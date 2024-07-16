namespace GameServer;

public class KeyGenerator
{
    public static string GeneratePlayerLoginKey(string playerId)
    {
        return $"U_{playerId}_Login";
    }
}
