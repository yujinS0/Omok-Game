using GameServer.Models;

namespace GameServer.Repository;

public interface IMasterDb : IDisposable
{
    Task<bool> Load();
    GameServer.Models.Version GetVersion();
    List<AttendanceReward> GetAttendanceRewards();
    List<Item> GetItems();
    List<FirstItem> GetFirstItems();
}
