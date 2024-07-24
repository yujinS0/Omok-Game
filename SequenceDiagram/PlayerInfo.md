# 시퀀스 다이어그램 (PlayerInfo)

------------------------------
------------------------------

## Basic Player Data
### : 플레이어 기본 데이터 가져오는 요청 (닉네임, 레벨, 경험치, 승, 패, 무)
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  participant GameDB

	Player ->> Game Server : Character 정보 요청
	Game Server ->> GameDB : GetCharInfoSummaryAsync 로 가져오기
  GameDB ->> Game Server : 
  Game Server ->> Player : Result, CharSummary 결과 정보

```



------------------------------


## Update NickName
### : 닉네임 변경 요청
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  participant GameDB

	Player ->> Game Server : 닉네임 업데이트 요청
	Game Server ->> GameDB : UpdateCharacterNameAsync 닉네임 업데이트
  GameDB ->> Game Server : 
  Game Server ->> Player : Result 결과 정보

```


------------------------------

