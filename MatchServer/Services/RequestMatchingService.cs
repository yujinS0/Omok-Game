using System.Collections.Concurrent;
using System.Threading.Tasks;
using MatchServer.DTO;
using MatchServer.Repository;
using MatchServer.Services.Interfaces;

namespace MatchServer.Services;

public class RequestMatchingService : IRequestMatchingService
{
    private static ConcurrentQueue<string> _reqQueue = new();
    private static int _nextRoomNum = 1;

    private readonly ILogger<RequestMatchingService> _logger;
    private readonly IMemoryDb _memoryDb;

    public RequestMatchingService(ILogger<RequestMatchingService> logger, IMemoryDb memoryDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
    }

    public async Task<MatchResponse> Match(MatchRequest request)
    {
        _logger.LogInformation($"POST RequestMatching : {request.PlayerId}", request.PlayerId);
        if (request == null || string.IsNullOrEmpty(request.PlayerId))
        {
            _logger.LogError("Invalid match request data.");
            return new MatchResponse { Result = ErrorCode.InvalidRequest };
        }

        _reqQueue.Enqueue(request.PlayerId);
        _logger.LogInformation("Added {PlayerId} to match request queue.", request.PlayerId);

        if (_reqQueue.Count >= 2)
        {
            if (_reqQueue.TryDequeue(out string playerId1) && _reqQueue.TryDequeue(out string playerId2))
            {
                var roomNum = _nextRoomNum++;
                _logger.LogInformation("Matching players {PlayerId1} and {PlayerId2} with room number {RoomNum}.", playerId1, playerId2, roomNum);
                await _memoryDb.StoreMatchResultAsync(playerId1, roomNum);
                await _memoryDb.StoreMatchResultAsync(playerId2, roomNum);
                _logger.LogInformation("Stored match results for players {PlayerId1} and {PlayerId2} in room {RoomNum}.", playerId1, playerId2, roomNum);
            }
        }

        return new MatchResponse { Result = ErrorCode.None };
    }
}
