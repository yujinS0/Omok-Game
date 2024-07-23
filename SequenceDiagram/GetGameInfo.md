# 시퀀스 다이어그램
## GetGameInfo

## 오목 보드 가져오기 
### POST GetGameInfo/board
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


## 30초 턴 체크 (Long Polling)
### POST GetGameInfo/WaitForTurnChange
```mermaid
sequenceDiagram
	actor User
	participant Game Server
  	participant Redis

	User ->> Game Server : 게임 턴 체크 요청
	Game Server ->> Redis : GetCurrentTurn 현재 턴 체크
  	Redis ->> Game Server : 
  
alt 30초 전에 돌 두었을 때
  Game Server ->> Redis : 현재 턴 변경
  Redis ->> Game Server : GetBoard
  Game Server ->> User : Board와 CurrentTurn 정보
end

alt 30초 지나서 타임 아웃
  Game Server ->> Game Server : AutoChangeTurn()
  Game Server ->> Redis : 현재 턴 변경
  Redis ->> Game Server : GetBoard
  Game Server ->> User : Board와 CurrentTurn 정보
end

```


## 현재 게임 턴 체크 (Polling)
### POST GetGameInfo/turnplayer
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

## 승자 정보 가져오기
### POST GetGameInfo/winner
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

