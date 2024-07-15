using MatchServer.DTO;

namespace MatchServer.Repository;

public interface IMemoryDb : IDisposable
{
    //Task<int?> PopRoomNumAsync();
    Task StoreMatchResultAsync(string playerId, int roomNum);
    Task<int?> GetMatchResultAsync(string playerId);
}
