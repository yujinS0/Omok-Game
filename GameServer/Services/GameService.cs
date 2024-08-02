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

        try
        {
            omokGameData.SetStone(playerId, x, y);
            bool updateResult = await _memoryDb.UpdateGameData(gameRoomId, omokGameData.GetRawData());

            if (!updateResult)
            {
                _logger.LogError("Failed to update game data for RoomId: {RoomId}", gameRoomId);
                return (ErrorCode.UpdateGameDataFailException, null);
            }

            // 오목 두고 승자 체크하기
            var (result, winner) = await CheckForWinner(omokGameData);
            if (result != ErrorCode.None)
            {
                return (result, null);
            }

            return (ErrorCode.None, null);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation error occurred while setting stone at ({X}, {Y}) for PlayerId: {PlayerId}", x, y, playerId);
            return (ErrorCode.InvalidOperationException, null);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Redis error occurred while updating game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.RedisException, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set stone at ({X}, {Y}) for PlayerId: {PlayerId}", x, y, playerId);
            return (ErrorCode.SetStoneFailException, null);
        }
    }

    private async Task<(ErrorCode, OmokGameData, string)> ValidatePlayerTurn(string playerId)
    {
        string playingUserKey = KeyGenerator.PlayingUser(playerId);
        UserGameData userGameData = await _memoryDb.GetPlayingUserInfo(playingUserKey);

        if (userGameData == null)
        {
            _logger.LogError("Failed to retrieve playing user info for PlayerId: {PlayerId}", playerId);
            return (ErrorCode.UserGameDataNotFound, null, null);
        }

        string gameRoomId = userGameData.GameRoomId;

        byte[] rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            _logger.LogError("Failed to retrieve game data for RoomId: {RoomId}", gameRoomId);
            return (ErrorCode.GameRoomNotFound, null, null);
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        // 게임이 끝난 상태인지 체크
        OmokStone winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone != OmokStone.None)
        {
            _logger.LogError("Game End. PlayerId: {PlayerId}", playerId);
            return (ErrorCode.GameAlreadyEnd, null, null);
        }

        string currentTurnPlayerName = omokGameData.GetCurrentTurnPlayerName();
        if (playerId != currentTurnPlayerName)
        {
            _logger.LogError("It is not the player's turn. PlayerId: {PlayerId}", playerId);
            return (ErrorCode.NotYourTurn, null, null);
        }
        
        return (ErrorCode.None, omokGameData, gameRoomId);
    }

    private async Task<(ErrorCode, Winner)> CheckForWinner(OmokGameData omokGameData)
    {
        var winnerStone = omokGameData.GetWinnerStone();
        if (winnerStone == OmokStone.None)
        {
            return (ErrorCode.None, null);
        }

        var winnerPlayerId = winnerStone == OmokStone.Black ? omokGameData.GetBlackPlayerName() : omokGameData.GetWhitePlayerName();
        var loserPlayerId = winnerStone == OmokStone.Black ? omokGameData.GetWhitePlayerName() : omokGameData.GetBlackPlayerName();

        try
        {
            //TODO: UpdateGameResult 메서드 호출시 실패가 발생했을 때에 대한 부분이 없습니다(예 DB업데이트 실패 등)
            //=> 수정중. try-catch로 기본 예외 처리
            //=> + 메서드의 반환값을 통해 처리하기? Update 반환값 찾아보기 SYJ
            await _gameDb.UpdateGameResult(winnerPlayerId, loserPlayerId); // GameDb에 결과 업데이트
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game result for winner: {WinnerId}, loser: {LoserId}", winnerPlayerId, loserPlayerId);
            return (ErrorCode.UpdateGameResultFail, null);
        }

        return (ErrorCode.None, new Winner { Stone = winnerStone, PlayerId = winnerPlayerId });
    }

    public async Task<(ErrorCode, GameInfo)> GiveUpPutOmok(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            return (ErrorCode.GameRoomNotFound, null);
        }
        var rawData = await _memoryDb.GetGameData(gameRoomId);

        var omokGameData = new OmokGameData();

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
            CurrentTurn = await GetCurrentTurn(playerId)
        });
    }

    //TODO: (07.31) 코드를 함수로 나누어서 코드 가독성을 올려주시기 바랍니다.
    //=> 수정 완료했습니다.
    public async Task<(ErrorCode, bool)> TurnChecking(string playerId)
    {
        var currentTurn = await GetCurrentTurn(playerId);

        if (currentTurn == OmokStone.None)
        {
            return (ErrorCode.GameTurnNotFound, false);
        }

        var (errorCode, currentTurnPlayer) = await GetPlayerForCurrentTurn(currentTurn, playerId);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, false);
        }

        return (ErrorCode.None, IsPlayerTurn(playerId, currentTurnPlayer));
    }

    private async Task<(ErrorCode, string)> GetPlayerForCurrentTurn(OmokStone currentTurn, string playerId)
    {
        string currentTurnPlayer;

        if (currentTurn == OmokStone.Black)
        {
            currentTurnPlayer = await GetBlackPlayer(playerId);
        }
        else if (currentTurn == OmokStone.White)
        {
            currentTurnPlayer = await GetWhitePlayer(playerId);
        }
        else
        {
            return (ErrorCode.GameTurnPlayerNotFound, null);
        }

        if (string.IsNullOrEmpty(currentTurnPlayer))
        {
            return (ErrorCode.GameTurnPlayerNotFound, null);
        }

        return (ErrorCode.None, currentTurnPlayer);
    }

    private bool IsPlayerTurn(string playerId, string currentTurnPlayer)
    {
        return currentTurnPlayer == playerId;
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

    private async Task<OmokGameData> GetGameData(string playerId)
    {
        var gameRoomId = await _memoryDb.GetGameRoomId(playerId);
        if (gameRoomId == null)
        {
            return null;
        }

        var rawData = await _memoryDb.GetGameData(gameRoomId);
        if (rawData == null)
        {
            return null;
        }

        var omokGameData = new OmokGameData();
        omokGameData.Decoding(rawData);

        return omokGameData;
    }

    private async Task<string> GetBlackPlayer(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetBlackPlayerName();
    }

    private async Task<string> GetWhitePlayer(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetWhitePlayerName();
    }

    private async Task<OmokStone> GetCurrentTurn(string playerId)
    {
        var omokGameData = await GetGameData(playerId);
        return omokGameData?.GetCurrentTurn() ?? OmokStone.None;
    }
}
