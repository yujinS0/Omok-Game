# 시퀀스 다이어그램 (GamePlay)
------------------------------
## Put Omok
### : 돌두기 (자기 차례 플레이어)
```mermaid
sequenceDiagram
	actor P as 자기차례 Player
	participant G as Game Server
	participant GD as GameDB
  	participant R as Redis

	P ->> G: 돌 두기 요청
	G ->> G : playingUserKey 생성
 	G ->> R : userGameData 가져오기
	R ->> G :  
  	G ->> R : GameRoomId로 gameData 가져오기
  	R ->> G: 

	G ->> G : 자기 턴 맞는지 확인

	G ->> R : 돌 두기
	R ->> G :  

	G ->> G : 승자 체크 요청
	alt 승자 존재
	  G ->> GD : 게임 결과 (승/패) 업데이트
	  GD ->> G :   
	end

  	G ->> P : 돌두기 성공 + 승자 정보

```

------------------------------

## Giveup Put Omok 
### : 돌두기 포기 요청 (자기 차례 플레이어)
```mermaid
sequenceDiagram
	actor P as 자기차례 Player
	participant G as Game Server
	participant GD as GameDB
  	participant R as Redis

	P ->> G : 게임 턴 변경 요청
	G ->> R : GetCurrentTurn 현재 턴 체크
  	R ->> G : 
	G ->> G : AutoChangeTurn()
	G ->> R : 현재 턴 변경
	R ->> G  : GetBoard
	G ->> P : Board와 CurrentTurn 정보

```



------------------------------

## Turn Checking 
### : 현재 턴 상태 요청 (차례 대기 플레이어)

```mermaid
sequenceDiagram
	actor P as 차례대기 Player
	participant G as Game Server
  	participant R as Redis

	P ->> G : 현재 게임 턴 체크 요청
	G ->> R : GetCurrentTurn 현재 턴 체크
  	R ->> G : 
  	G ->> P : CurrentTurnPlayerId 정보

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
  Game Server ->> Redis : GetGameData
  Game Server ->> Player : GameData byte[] 정보

```


