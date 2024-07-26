using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using ServerShared;
using SqlKata.Execution;

namespace GameServer.Repository;

public class MasterDb : IMasterDb // TODO syj 구현중
{
    readonly IOptions<DbConfig> _dbConfig;
    readonly ILogger<MasterDb> _logger;

    private Version _version { get; set; }
    //private List<CostumeData> _costumeList { get; set; }
    //private List<CostumeSetData> _costumeSetList { get; set; }
    //private List<FoodData> _foodList { get; set; }
    //private List<SkillData> _skillList { get; set; }
    //private List<GachaRewardData> _gachaRewardList { get; set; }
    //private List<ItemLevelData> _itemLevelList { get; set; }

    public MasterDb(ILogger<MasterDb> logger, IOptions<DbConfig> dbConfig)
    {
        _logger = logger;
        _dbConfig = dbConfig;
    }

    public void Dispose()
    {
    }

    public async Task<bool> Load() // 생성자에서 로드
    {
        try
        {
            var dbConn = new MySqlConnection(_dbConfig.Value.MasterDBConnection);
            dbConn.Open();

            var compiler = new SqlKata.Compilers.MySqlCompiler();
            var queryFactory = new QueryFactory(dbConn, compiler);


            //_version = await queryFactory.Query($"version").FirstOrDefaultAsync<VersionDAO>();
            //_attendanceRewardList = (await queryFactory.Query($"master_attendance_reward").GetAsync<AttendanceRewardData>()).ToList();
            //_characterList = (await queryFactory.Query($"master_char").GetAsync<CharacterData>()).ToList();
            //_skillList = (await queryFactory.Query($"master_skill").GetAsync<SkillData>()).ToList();
            //_itemLevelList = (await queryFactory.Query($"master_item_level").GetAsync<ItemLevelData>()).ToList();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            //_logger.ZLogError(e,
                //$"[MasterDb.Load] ErrorCode: {ErrorCode.MasterDB_Fail_LoadData}");
            return false;
        }
        finally
        {
            //dbConn.Close();
        }

        return true;
    }
}