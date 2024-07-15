using System.Threading.Tasks;
using MatchServer.DTO;

namespace MatchServer.Services.Interfaces;

public interface ICheckMatchingService
{
    Task<MatchCompleteResponse> IsMatched(MatchRequest request);
}