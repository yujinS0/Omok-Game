using System.Collections.Concurrent;
using System.Threading.Tasks;
using MatchServer.DTO;
using MatchServer.Repository;
using ServerShared;
using MatchServer.Services.Interfaces;

namespace MatchServer.Services;

public class RequestMatchingService : IRequestMatchingService
{
    private readonly ILogger<RequestMatchingService> _logger;
    private readonly MatchWorker _matchWorker;

    public RequestMatchingService(ILogger<RequestMatchingService> logger, MatchWorker matchWorker)
    {
        _logger = logger;
        _matchWorker = matchWorker;
    }

    public MatchResponse Matching(MatchRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.PlayerId)) 
        {
            _logger.LogError("Invalid match request data.");
            return new MatchResponse { Result = ErrorCode.InvalidRequest };
        }

        _logger.LogInformation($"POST RequestMatching: {request.PlayerId}");

        _matchWorker.AddMatchRequest(request.PlayerId);

        _logger.LogInformation("Added {PlayerId} to match request queue.", request.PlayerId);

        return new MatchResponse { Result = ErrorCode.None };
    }
}