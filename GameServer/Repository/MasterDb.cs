using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using ServerShared;
using SqlKata.Execution;
using GameServer.DTO;
using GameServer.Models;
using GameServer.Repository.Interfaces;


namespace GameServer.Repository;

public class MasterDb : IMasterDb
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<MasterDb> _logger;

    private GameServer.Models.Version _version { get; set; }
    private List<AttendanceReward> _attendanceRewardList { get; set; }
    private List<Item> _itemList { get; set; }
    private List<FirstItem> _firstItemList { get; set; }

    public MasterDb(ILogger<MasterDb> logger, IOptions<DbConfig> dbConfig)
    {
        _logger = logger;
        _dbConfig = dbConfig;

        //TODO: (08.01) 여기에서 DB 객체를 만들 필요 없습니다. Load()에서 만들고 함수 종료 때 DB 연결을 끊어야 합니다.
        //=> 수정 완료했습니다.

        //TODO: (08.01) 기획데이터 로딩이 실패하면 서버 실행이 중단되도록 해야합니다.
        //=> 수정 완료했습니다.
        var loadTask = Load();
        loadTask.Wait();

        if (!loadTask.Result)
        {
            throw new InvalidOperationException("Failed to load master data from the database. Server is shutting down.");
        }
    }
    
    //TODO: (08.01) 불필요한 코드입니다
    //=> 수정 완료했습니다. 기존 Dispose() 삭제 후 Load() 메서드 끝부분에서 항상 close 하도록 수정했습니다!


    public async Task<bool> Load()
    {
        MySqlConnection connection = null;
        try
        {
            connection = new MySqlConnection(_dbConfig.Value.MasterDBConnection);
            connection.Open();

            var queryFactory = new QueryFactory(connection, new MySqlCompiler());

            // Load Version
            var getVersionResult = await queryFactory.Query("version").FirstOrDefaultAsync();
            if (getVersionResult == null)
            {
                _logger.LogWarning("No Version data found [MasterDb]");
                return false;
            }

            var version = new GameServer.Models.Version
            {
                AppVersion = getVersionResult.app_version,
                MasterDataVersion = getVersionResult.master_data_version
            };
            _version = version;

            _logger.LogInformation($"Loaded version: AppVersion={_version.AppVersion}, MasterDataVersion={_version.MasterDataVersion}");

            // Load AttendanceReward
            var attendanceRewardsResult = await queryFactory.Query("attendance_reward").GetAsync();
            if (attendanceRewardsResult == null || !attendanceRewardsResult.Any())
            {
                _logger.LogWarning("No AttendanceReward data found [MasterDb]");
                return false;
            }

            _attendanceRewardList = attendanceRewardsResult.Select(ar => new AttendanceReward
            {
                DaySeq = ar.day_seq,
                RewardItem = ar.reward_item,
                ItemCount = ar.item_count
            }).ToList();

            _logger.LogInformation("Loaded attendance rewards:");
            foreach (var reward in _attendanceRewardList)
            {
                _logger.LogInformation($"DaySeq={reward.DaySeq}, RewardItem={reward.RewardItem}, ItemCount={reward.ItemCount}");
            }

            // Load Item
            var itemsResult = await queryFactory.Query("item").GetAsync();
            if (itemsResult == null || !itemsResult.Any())
            {
                _logger.LogWarning("No Item data found [MasterDb]");
                return false;
            }

            _itemList = itemsResult.Select(it => new Item
            {
                ItemCode = it.item_code,
                Name = it.name,
                Description = it.description
            }).ToList();

            _logger.LogInformation("Loaded items:");
            foreach (var item in _itemList)
            {
                _logger.LogInformation($"ItemCode={item.ItemCode}, Name={item.Name}, Description={item.Description}");
            }

            // Load FirstItem
            var firstItemsResult = await queryFactory.Query("first_item").GetAsync();
            if (firstItemsResult == null || !firstItemsResult.Any())
            {
                _logger.LogWarning("No FirstItem data found [MasterDb]");
                return false;
            }

            _firstItemList = firstItemsResult.Select(fi => new FirstItem
            {
                ItemCode = fi.item_code,
                Count = fi.count
            }).ToList();

            _logger.LogInformation("Loaded first items:");
            foreach (var firstItem in _firstItemList)
            {
                _logger.LogInformation($"ItemCode={firstItem.ItemCode}, Count={firstItem.Count}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[MasterDb.Load] Error loading data from database.");
            return false;
        }
        finally
        {
            // Load 메서드가 끝나면 항상 연결을 닫습니다.
            if (connection != null)
            {
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
        }

        return true;
    }

    public GameServer.Models.Version GetVersion()
    {
        return _version;
    }

    public List<AttendanceReward> GetAttendanceRewards()
    {
        return _attendanceRewardList;
    }

    public List<Item> GetItems()
    {
        return _itemList;
    }

    public List<FirstItem> GetFirstItems()
    {
        return _firstItemList;
    }
}