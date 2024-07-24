# 시퀀스 다이어그램
## OmokGamePlay

## 오목 보드 가져오기 
### POST OmokGamePlay/board
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


## 30초 지나면 호출되는, 턴 바꾸기 (Long Polling)
### POST OmokGamePlay/turn-change
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


## 1초마다 호출되는, 현재 게임 턴 체크 (Polling)
### POST OmokGamePlay/current-turn-player
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
### POST turn-change/winner
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

