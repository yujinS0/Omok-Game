using System.Threading.Tasks;
using MatchServer.DTO;

namespace MatchServer.Services.Interfaces;

public interface IRequestMatchingService
{
    MatchResponse Matching(MatchRequest request);
}
