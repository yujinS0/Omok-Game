using System.Text;
enum OmokStone
{
    None,
    Black,
    White
}

class OmokGameData
{
    const int BoardSize = 15;
    const int BoardSizeSquare = BoardSize * BoardSize;

    const byte BlackStone = 1;
    const byte WhiteStone = 2;

    // 오목판 정보 BoardSize * BoardSize
    // 블랙 플레이어의 이름: 1(이름 바이트 수) + N(앞에서 구한 길이)
    // 화이트 플레이어의 이름: 1(이름 바이트 수) + N(앞에서 구한 길이)
    byte[] _rawData;

    string _blackPlayer;
    string _whitePlayer;

    //TODO 턴 받은 플레이어
    OmokStone _turnPlayerStone;

    // 턴 받은 시간 유닉스 시간(초)
    UInt64 _turnTimeMilli;


    public byte[] GetRawData()
    {
        return _rawData;
    }

    public byte[] MakeRawData(int rawDataSize, string blackPlayer, string whitePlayer)
    {
        var rawData = new byte[rawDataSize];

        //TODO blackPlayer, whitePlayer -> byte[]

        return rawData;
    }

    public void Decoding(byte[] rawData)
    {
        _rawData = rawData;

        DecodingUserName();
    }


    public void SetStone(bool isBlack, int x, int y)
    {

    }

    public void OmokCheck()
    {

    }


    void DecodingUserName()
    {
        var index = BoardSizeSquare;

        int blackPlayerNameLength = _rawData[index];
        index += 1;
        _blackPlayer = Encoding.UTF8.GetString(_rawData, index, blackPlayerNameLength);

        index += blackPlayerNameLength;
        int whitePlayerNameLength = _rawData[index];
        index += 1;
        _whitePlayer = Encoding.UTF8.GetString(_rawData, index, whitePlayerNameLength);
    }
}