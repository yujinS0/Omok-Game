public class DbConfig
{
    public string MysqlGameDBConnection { get; set; } ="";
    public string RedisGameDBConnection { get; set; } ="";
    public int RedisExpiryHours { get; set; } // 유효 기간을 시간 단위로 설정
}