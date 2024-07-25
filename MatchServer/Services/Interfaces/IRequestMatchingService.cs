using System.Threading.Tasks;
using MatchServer.DTO;
using ServerShared;

namespace MatchServer.Services.Interfaces;

public interface IRequestMatchingService
{
    ErrorCode Matching(string playerId);
}
