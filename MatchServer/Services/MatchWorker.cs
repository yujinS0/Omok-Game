using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using MatchServer.Models;
using MatchServer.Repository;
using ServerShared;

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
                        var gameRoomId = KeyGenerator.GameRoomId();

                        var matchResultA = new MatchResult { GameRoomId = gameRoomId, Opponent = playerB };
                        var matchResultB = new MatchResult { GameRoomId = gameRoomId, Opponent = playerA };

                        // TODO 결과 없으면 바로 continue;로 빠져나가도록

                        // TODO 로직 함수화 (매칭결과 저장 & 게임데이터 저장)

                        var keyA = KeyGenerator.MatchResult(playerA);
                        var keyB = KeyGenerator.MatchResult(playerB);

                        _memoryDb.StoreMatchResultAsync(keyA, matchResultA, RedisExpireTime.MatchResult).Wait();
                        _memoryDb.StoreMatchResultAsync(keyB, matchResultB, RedisExpireTime.MatchResult).Wait();

                        _logger.LogInformation("Matched {PlayerA} and {PlayerB} with RoomId: {RoomId}", playerA, playerB, gameRoomId);

                        // 게임 플레이 데이터 만드는 부분
                        var omokGameData = new OmokGameData();

                        byte[] gameRawData = omokGameData.MakeRawData(playerA, playerB);

                        _memoryDb.StoreGameDataAsync(gameRoomId, gameRawData, RedisExpireTime.GameData).Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while running matching.");
                }
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing MatchWorker");
        }
    }
}
