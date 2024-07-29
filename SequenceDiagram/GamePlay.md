# 시퀀스 다이어그램 (GamePlay)

------------------------------
## OmokGameData 
### : 게임 데이터 가져오는 요청 (모든 플레이어)
게임 데이터 = 오목 보드 정보 + 참가 플레이어 + 현재 턴 + 승자 등등


#### 어떤 상태의 플레이어가 요청하는지?

* 기본적으로 LoadGameStateAsync() 라는 함수에서 게임 오목판을 비롯한 게임 데이터를 로드하고 있다.
  + 게임 첫 시작 시 : OnInitializedAsync()
  + 자기 차례 아닐 때 : 턴 상태 요청하면서 StartTurnPollingAsync()
    + 이때 자기 차례가 되었을 때 로드

```mermaid
sequenceDiagram
	actor P as 모든 Player
	participant G as Game Server
  	participant R as Redis

	P ->> G: 게임 데이터 정보 요청
	G ->> R : GameData 가져오기
	R-->>G: 
	alt 데이터 존재 X
		G-->>P: 오류 응답
	else 데이터 존재 O
		G -->> P : GameData 정보 응답
	end
```

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
	G ->> R : playingUserKey 생성 후 userGameData 가져오기
	R -->> G :  
  	G ->> R : GameRoomId로 GameData 가져오기
  	R -->> G: 

	G ->> G : 자기 턴 맞는지 확인
	alt 내 차례 X
		G-->>P: NotYourTurn 오류 응답
	else 내 차례 o
		G ->> R : 돌 두기 정보 업데이트
		R ->> G :  
	
		G ->> G : 승자 체크 요청
		alt 승자 존재
		  G ->> GD : 게임 결과 (승/패) 업데이트
		  GD -->> G :   
		end
	
	  	G -->> P : 성공 + GameData 정보 응답
	end
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

	P ->> G : 돌두기 포기 요청
	G ->> G : 자기 턴 맞는지 확인
	alt 내 차례 X
		G-->>P: NotYourTurn 오류 응답
	else 내 차례 o
		G ->> G : 턴 변경
		G ->> R : 돌 두기 정보 업데이트
		R -->> G :  
	  	G -->> P : 성공 + GameData 정보 응답
	end

```



------------------------------

## Turn Checking 
### : 현재 턴 상태 요청 (차례 대기 플레이어) 1초마다 요청

```mermaid
sequenceDiagram
	actor P as 차례대기 Player
	participant G as Game Server
  	participant R as Redis

	P ->> G : 현재 턴 체크 요청
	G ->> R : GetCurrentTurn 현재 턴 체크
  	R -->> G : 
  	G -->> P : CurrentTurnPlayerId 정보 응답

```


------------------------------


