namespace HiveServer.Repository
{
    public interface IHiveDb : IDisposable
    {
        public Task<ErrorCode> RegisterAccount(string hive_player_id, string hive_player_pw);
        public Task<(ErrorCode, string)> VerifyUser(string hive_player_id, string hive_player_pw);
        public Task<bool> SaveToken(string hive_player_id, string token);

        public Task<bool> ValidateTokenAsync(string hive_player_id, string token);  
    }
}
