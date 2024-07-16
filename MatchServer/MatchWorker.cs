using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MatchServer.Models;
using MatchServer.Repository;
using GameServer;

namespace MatchServer.Services
{
    public class MatchWorker : IDisposable
    {
        private readonly ILogger<MatchWorker> _logger;
        private readonly IMemoryDb _memoryDb;
        private static readonly ConcurrentQueue<string> _reqQueue = new();

        private readonly System.Threading.Thread _matchThread;

        public MatchWorker(ILogger<MatchWorker> logger, IMemoryDb memoryDb)
        {
            _logger = logger;
            _memoryDb = memoryDb;

            _matchThread = new System.Threading.Thread(RunMatching);
            _matchThread.Start();
        }

        public void AddMatchRequest(string playerId)
        {
            _reqQueue.Enqueue(playerId); // 요청 큐에 플레이어 넣기
        }

        private void RunMatching()
        {
            while (true)
            {
                try
                {
                    if (_reqQueue.Count < 2)
                    {
                        System.Threading.Thread.Sleep(100); // 잠시 대기
                        continue;
                    }

                    if (_reqQueue.TryDequeue(out var playerA) && _reqQueue.TryDequeue(out var playerB))
                    {
                        var gameRoomId = KeyGenerator.GenerateRoomId();

                        var matchResultA = new MatchResult { GameRoomId = gameRoomId, Opponent = playerB };
                        var matchResultB = new MatchResult { GameRoomId = gameRoomId, Opponent = playerA };

                        var keyA = KeyGenerator.GenerateMatchResultKey(playerA);
                        var keyB = KeyGenerator.GenerateMatchResultKey(playerB);

                        _memoryDb.StoreMatchResultAsync(keyA, matchResultA, TimeSpan.FromMinutes(10)).Wait();
                        _memoryDb.StoreMatchResultAsync(keyB, matchResultB, TimeSpan.FromMinutes(10)).Wait();

                        _logger.LogInformation("Matched {PlayerA} and {PlayerB} with RoomId: {RoomId}", playerA, playerB, gameRoomId);

                        ///////////////////////////
                        //// 게임 플레이 데이터 만드는 부분
                        var omokGameData = new OmokGameData();

                        int rawDataSize = 328;

                        byte[] gameRawData = omokGameData.MakeRawData(rawDataSize, playerA, playerB);

                        _memoryDb.StoreGameDataAsync(gameRoomId, gameRawData, TimeSpan.FromHours(2)).Wait();




                        ////////////////////////////////////////////////////////////////////
                        //// RoomId를 key 값으로 해서 GameInfo 라는 클래스의 value 값 생성
                        //// -> 내가 임의로 만들었던 게임 플레이 데이터인데, 이제 위와 같은 바이너리 데이터로 진행
                        //// 이때 GameInfo 는 playerA, playerB, 룸 생성시간 포함
                        //var gameInfo = new GameInfo
                        //{
                        //    PlayerA = playerA,
                        //    PlayerB = playerB,
                        //    CreatedAt = DateTime.UtcNow
                        //};

                        //_memoryDb.StoreGameInfoAsync(gameRoomId, gameInfo, TimeSpan.FromHours(2)).Wait();
                        //_logger.LogInformation("Stored GameInfo for RoomId: {RoomId}", gameRoomId);



                        ////////////////////////
                        // ?? 여기서 하는 거 아닙니다 !
                        // 현재 플레이중인 유저 정보 playingUserInfo 생성
                        //var playingUserInfoA = new PlayingUserInfo { PlayerId = playerA, RoomId = roomId};
                        //var playingUserInfoB = new PlayingUserInfo { PlayerId = playerB, RoomId = roomId };
                        //var playingUserkeyA = KeyGenerator.GeneratePlayingUserKey(playerA);
                        //var playingUserkeyB = KeyGenerator.GeneratePlayingUserKey(playerB);
                        //_memoryDb.StorePlayingUserInfoAsync(playingUserkeyA, playingUserInfoA, TimeSpan.FromHours(2)).Wait();
                        //_memoryDb.StorePlayingUserInfoAsync(playingUserkeyB, playingUserInfoB, TimeSpan.FromHours(2)).Wait();

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while running matching.");
                }
            }
        }

        //public async Task<MatchResult> GetMatchResultAsync(string playerId)
        //{
        //    var key = KeyGenerator.GenerateMatchResultKey(playerId);
        //    return await _memoryDb.GetMatchResultAsync(key);
        //}

        public void Dispose()
        {
            _logger.LogInformation("Disposing MatchWorker");
        }
    }
}
