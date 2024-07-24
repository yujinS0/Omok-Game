# 시퀀스 다이어그램 (GamePlay)
------------------------------
## Put Omok
### : 돌두기 (자기 차례 플레이어)
```mermaid
sequenceDiagram
	actor Player(턴 받은)
	participant Game Server
  	participant Redis

	

```

------------------------------

## Giveup Put Omok 
### : 돌두기 포기 요청 (자기 차례 플레이어)
```mermaid
sequenceDiagram
	actor Player(턴 받은)
	participant Game Server
  	participant Redis

	Player(턴 받은) ->> Game Server : 게임 턴 변경 요청
	Game Server ->> Redis : GetCurrentTurn 현재 턴 체크
  	Redis ->> Game Server : 
	  Game Server ->> Game Server : AutoChangeTurn()
	  Game Server ->> Redis : 현재 턴 변경
	  Redis ->> Game Server : GetBoard
	  Game Server ->> Player(턴 받은) : Board와 CurrentTurn 정보

```



------------------------------

## Turn Checking 
### : 현재 턴 상태 요청 (차례 대기 플레이어)

```mermaid
sequenceDiagram
	actor Player(턴 기다리는)
	participant Game Server
  	participant Redis

	Player(턴 기다리는) ->> Game Server : 현재 게임 턴 체크 요청
	Game Server ->> Redis : GetCurrentTurn 현재 턴 체크
  	Redis ->> Game Server : 
  	Game Server ->> Player(턴 기다리는) : CurrentTurnPlayerId 정보

```


------------------------------


## OmokGameData 
### : 게임 데이터 가져오는 요청 (보드정보 + 플레이어 등등)

```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  	participant Redis

	Player ->> Game Server: 보드 정보 요청
	Game Server ->> Game Server : GameRoomId (Key) 생성
  Game Server ->> Redis : GetBoard
  Game Server ->> Player : 보드 byte[] 정보

```


