namespace HiveServer.Model.DAO; 
public class HdbAccount
{
    public long account_uid { get; set;}
    public required string hive_player_id {get; set;} // email
    public required string hive_player_pw { get; set;}
}