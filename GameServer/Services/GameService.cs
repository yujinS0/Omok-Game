using GameServer.DTO;
using ServerShared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using GameServer.Services.Interfaces;
using GameServer.Models;
using StackExchange.Redis;
using GameServer.Repository.Interfaces;

namespace GameServer.Services;

public class GameService : IGameService
{
    private readonly IMemoryDb _memoryDb;
    private readonly IGameDb _gameDb;
    private readonly ILogger<GameService> _logger;

    public GameService(IMemoryDb memoryDb, IGameDb gameDb, ILogger<GameService> logger)
    {
        _memoryDb = memoryDb;
        _gameDb = gameDb;
        _logger = logger;
    }

    public async Task<(ErrorCode, Winner)> PutOmok(string playerId, int x, int y)
    {
        var (validatePlayerTurn, omokGameData, gameRoomId) = await ValidatePlayerTurn(playerId);

        if (validatePlayerTurn != ErrorCode.None)
        {
            _logger.LogError("validatePlayerTurn : Fail");
            return (validatePlayerTurn, null);
        }

        //TODO: (08.08) 아래에서 예외가 발생할 일이 있을까요? 이미 레포지트에서 예외처리는 다 하고 있을테니
        //  다른 코드에도 이런 것이 보이는데 불필요하게 에외문을 사용하지는 마세요
        //=> [진행중] 다른 부분 코드 확인중입니다.

        //=> 바로 아래부분은 수정 완료했습니다.
        omokGameData.SetStone(playerId, x, y);
        bool updateResult = await _memoryDb.UpdateGameData(gameRoomId, omokGameData.GetRawData());

        if (!updateResult)
        {
            _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.UpdateGameDataFailException, null);
        }

        //TODO: (08.08) 게임이 끝난 경우라면 GameRoomId 으로 만들어지는 두명의 플레이의 키를 저장한 데이터도 게임이 끝났을 때의 UpdateGameData에서 사용하는 expire 시간과 동일한 시간내에서 삭제되도록 해야합니다.
        // 이렇게 해야 게임이 끝났는데도 계속 데이터를 요청하는 어뷰징을 막을 수 있습니다
        //=> 수정 완료했습니다. RedisExpireTime에서 GameData와 InGamePlayerInfo를 동일하게 설정했습니다.
        var (result, winner) = await CheckForWinner(omokGameData);
        if (result != ErrorCode.None)
        {
            return (result, null);
        }

        return (ErrorCode.None, winner);
    }

    private async Task<(ErrorCode, OmokGameEngine, string)> ValidatePlayerTurn(string playerId)
    {
        //TODO: (08.08) 코드 가독성을 위해 아래 코드들을 함수로 분리하도록 하시죠
        //=> 수정 완료했습니다.

        //TODO: (08.08) var gameRoomId = await _memoryDb.GetGameRoomId(playerId);와 중복코드 아닌가요?
        //=> 맞습니다 수정 완료했습니다.
        string gameRoomId = await _memoryDb.GetGameRoomId(playerId);

        if (gameRoomId == null)
        {
            _logger.LogError("Failed to retrieve playing player info for PlayerId: {PlayerId}", playerId);
            return (ErrorCode.PlayerGameDataNotFound, null, null);
        }


        var omokGameData = await GetGameDataByGameRoomId(gameRoomId);

        if (omokGameData == null)
        {
            return (ErrorCode.GameRoomNotFound, null, null);
        }


        var gameEndError = CheckIfGameEnded(omokGameData, playerId);
        if (gameEndError != ErrorCode.None)
        {
            return (gameEndError, null, null);
        }


        var (result, isMyTurn) = await TurnChecking(playerId);
        if (result != ErrorCode.None)
        {
            return (result, null, null);
        }
        else
        {
            if (isMyTurn == false)
            {
                _logger.LogError("It is not the player's turn. PlayerId: {PlayerId}", playerId);
                return (ErrorCode.NotYourTurn, null, null);
            }
        }

        return (ErrorCode.None, omokGameData, gameRoomId);
    }

    private ErrorCode CheckIfGameEnded(OmokGameEngine omokGameData, string playerId)
    {
        OmokStone winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone != OmokStone.None)
        {
            _logger.LogError("Game End. PlayerId: {PlayerId}", playerId);
            return ErrorCode.GameAlreadyEnd;
        }
        return ErrorCode.None;
    }

    private async Task<(ErrorCode, Winner)> CheckForWinner(OmokGameEngine omokGameData)
    {
        var (winnerPlayerId, loserPlayerId) = omokGameData.GetWinnerAndLoser();

        if (winnerPlayerId == null || loserPlayerId == null)
        {
            return (ErrorCode.None, null);
        }

        try
        {
            var updateResult = await _gameDb.UpdateGameResult(winnerPlayerId, loserPlayerId, GameConstants.WinExp, GameConstants.LoseExp);
            if (!updateResult)
            {
                return (ErrorCode.UpdateGameResultFail, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game result for winner: {WinnerId}, loser: {LoserId}", winnerPlayerId, loserPlayerId);
            return (ErrorCode.UpdateGameResultFail, null);
        }

        return (ErrorCode.None, new Winner { Stone = omokGameData.GetWinnerStone(), PlayerId = winnerPlayerId });
    }

    public async Task<(ErrorCode, GameInfo)> GiveUpPutOmok(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            return (ErrorCode.GameRoomNotFound, null);
        }
        var rawData = await _memoryDb.GetGameData(gameRoomId);

        var omokGameData = new OmokGameEngine();

        try
        {
            var (result, updatedRawData) = omokGameData.ChangeTurn(rawData, playerId);
            if (result != ErrorCode.None)
            {
                return (result, null);
            }
            await _memoryDb.UpdateGameData(gameRoomId, updatedRawData);
            _logger.LogInformation("Turn changed successfully for player {PlayerId}", playerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change turn for player {PlayerId}", playerId);
        }

        var (errorCode, gameData) = await GetGameRawData(playerId);

        return (ErrorCode.None, new GameInfo
        {
            Board = gameData,
            CurrentTurn = await GetCurrentTurnStone(playerId)
        });
    }

    public async Task<(ErrorCode, bool)> TurnChecking(string playerId)
    {
        var currentTurnPlayerId = await GetCurrentTurnPlayerId(playerId);

        if (string.IsNullOrEmpty(currentTurnPlayerId))
        {
            return (ErrorCode.GameTurnPlayerNotFound, false);
        }

        if (playerId == currentTurnPlayerId)
        {
            _logger.LogInformation("It is your turn ! {PlayerId}", playerId);
            return (ErrorCode.None, true);
        }
        else
        {
            _logger.LogWarning("It is not the player's turn. PlayerId: {PlayerId}", playerId);
            return (ErrorCode.None, false);
        }
    }

    public async Task<(ErrorCode, byte[]?)> GetGameRawData(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            _logger.LogWarning("Game room not found for player: {PlayerId}", playerId);
            return (ErrorCode.GameRoomNotFound, null);
        }

        var rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            _logger.LogWarning("Game data not found for game room: {GameRoomId}", gameRoomId);
            return (ErrorCode.GameBoardNotFound, null);
        }

        return (ErrorCode.None, rawData);
    }


    private async Task<OmokGameEngine> GetGameDataByGameRoomId(string gameRoomId)
    {
        byte[] rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            return null;
        }

        var omokGameData = new OmokGameEngine();
        omokGameData.Decoding(rawData);
        return omokGameData;
    }

    private async Task<string> GetCurrentTurnPlayerId(string playerId)
    {
        string gameRoomId = await _memoryDb.GetGameRoomId(playerId);

        var omokGameData = await GetGameDataByGameRoomId(gameRoomId);
        return omokGameData?.GetCurrentTurnPlayerId();
    }

    private async Task<OmokStone> GetCurrentTurnStone(string playerId)
    {
        string gameRoomId = await _memoryDb.GetGameRoomId(playerId);

        var omokGameData = await GetGameDataByGameRoomId(gameRoomId);
        return omokGameData?.GetCurrentTurn() ?? OmokStone.None;
    }
}
