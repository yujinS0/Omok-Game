# 시퀀스 다이어그램 (OmokGamePlay)

------------------------------

## POST OmokGamePlay/turn-change
### : 30초 지나면 호출되는, 턴 바꾸기 (Long Polling) 
```mermaid
sequenceDiagram
	actor User
	participant Game Server
  	participant Redis

	User ->> Game Server : 게임 턴 변경 요청
	Game Server ->> Redis : GetCurrentTurn 현재 턴 체크
  	Redis ->> Game Server : 
	  Game Server ->> Game Server : AutoChangeTurn()
	  Game Server ->> Redis : 현재 턴 변경
	  Redis ->> Game Server : GetBoard
	  Game Server ->> User : Board와 CurrentTurn 정보

```

```css
public class PlayerRequest
{
    public string PlayerId { get; set; }
}

public class TurnChangeResponse
{
    public ErrorCode Result { get; set; }
    public GameInfo GameInfo { get; set; }
}
```

------------------------------

## POST OmokGamePlay/turn-checking
### : 1초마다 호출되는, 현재 게임 턴 체크 (Polling)
```mermaid
sequenceDiagram
	actor User
	participant Game Server
  	participant Redis

	User ->> Game Server : 현재 게임 턴 체크 요청
	Game Server ->> Redis : GetCurrentTurn 현재 턴 체크
  	Redis ->> Game Server : 
  	Game Server ->> User : CurrentTurnPlayerId 정보

```


```css
public class PlayerRequest
{
    public string PlayerId { get; set; }
}

public class PlayerResponse
{
    public ErrorCode Result { get; set; }
    public string PlayerId { get; set; }
}
```

------------------------------


## POST OmokGamePlay/board
### : 오목 보드 가져오기 
```mermaid
sequenceDiagram
	actor User
	participant Game Server
  	participant Redis

	User ->> Game Server: 보드 정보 요청
	Game Server ->> Game Server : GameRoomId (Key) 생성
  Game Server ->> Redis : GetBoard
  Game Server ->> User : 보드 byte[] 정보

```

```css
public class PlayerRequest
{
    public string PlayerId { get; set; }
}

public class BoardResponse
{
    public ErrorCode Result { get; set; }
    public byte[] Board { get; set; }
}
```



------------------------------



## POST turn-change/winner
### : 승자 정보 가져오기
```mermaid
sequenceDiagram
	actor User
	participant Game Server
  	participant Redis

	User ->> Game Server: 보드 정보 요청
	Game Server ->> Game Server : GameRoomId (Key) 생성
  	Game Server ->> Redis : GetWinnerData
	Redis ->> Game Server : 
  	Game Server ->> User : Winner(Stone, ID) 정보

```

```css
public class PlayerRequest
{
    public string PlayerId { get; set; }
}

public class WinnerResponse
{
    public ErrorCode Result { get; set; }
    public Winner Winner { get; set; }
}

public class Winner
{
    public OmokStone Stone { get; set; }
    public string PlayerId { get; set; }
}
```


