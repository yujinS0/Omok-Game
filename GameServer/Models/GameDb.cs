namespace GameServer.Models
{
    public class CharInfo
    {
        public int CharUid { get; set; }
        public string HivePlayerId { get; set; }
        public string CharName { get; set; }
        public int Exp { get; set; }
        public int Level { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public int Draw { get; set; }
    }
}
