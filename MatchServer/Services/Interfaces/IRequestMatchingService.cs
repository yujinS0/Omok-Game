using System.Threading.Tasks;
using MatchServer.DTO;

namespace MatchServer.Services.Interfaces;

public interface IRequestMatchingService
{
    Task<MatchResponse> Match(MatchRequest request);
}
