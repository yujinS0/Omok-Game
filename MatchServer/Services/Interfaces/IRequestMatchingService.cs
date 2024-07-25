using ServerShared;

namespace MatchServer.Services.Interfaces;

public interface IRequestMatchingService
{
    ErrorCode Matching(string playerId);
}
