namespace OmokClient;

public static class MasterData
{
    public static readonly Dictionary<int, string> ItemCodeToNameMap = new Dictionary<int, string>
    {
        { 3, "무르기" },
        { 4, "닉네임 변경" }
        // 필요한 만큼 아이템을 추가합니다.
    };
}
