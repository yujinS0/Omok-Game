using MatchServer.DTO;
using MatchServer.Repository;
using MatchServer.Services.Interfaces;

namespace MatchServer.Services;

public class CheckMatchingService : ICheckMatchingService
{
    private readonly ILogger<CheckMatchingService> _logger;
    private readonly IMemoryDb _memoryDb;

    public CheckMatchingService(ILogger<CheckMatchingService> logger, IMemoryDb memoryDb)
    {
        _logger = logger;
        _memoryDb = memoryDb;
    }

    public async Task<MatchCompleteResponse> IsMatched(MatchRequest request)
    {
        _logger.LogInformation($"POST CheckMatcing : {request.PlayerId}", request.PlayerId);

        if (request == null || string.IsNullOrEmpty(request.PlayerId))
        {
            _logger.LogError("Invalid CheckMatcing request data.");
            return new MatchCompleteResponse
            {
                Result = ErrorCode.InvalidRequest,
                Success = 0,
                PlayerId = request.PlayerId,
                RoomNum = -1
            };
        }

        var roomNum = await _memoryDb.GetMatchResultAsync(request.PlayerId);

        if (roomNum.HasValue)
        {
            return new MatchCompleteResponse
            {
                Result = ErrorCode.None,
                Success = 1,
                PlayerId = request.PlayerId,
                RoomNum = roomNum.Value
            };
        }

        return new MatchCompleteResponse
        {
            Result = ErrorCode.None,
            Success = 0,
            PlayerId = request.PlayerId,
            RoomNum = -1
        };
    }
}
