public class DbConfig
{
    public string MysqlGameDBConnection { get; set; } ="";
    public string RedisGameDBConnection { get; set; } ="";
    public int RedisExpiryHours { get; set; } // ��ȿ �Ⱓ�� �ð� ������ ����
}