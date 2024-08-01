# 시퀀스 다이어그램 (MailBox)

------------------------------

## Get Player MailBox
### : 플레이어의 우편함을 가져오는 요청 /mail/get-mailbox
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  participant GameDB

	Player ->> Game Server : 우편함 로드 요청 (with page number)
	Game Server ->> GameDB : MailBox 가져오기
	GameDB -->> Game Server : 
	alt 존재 X
		Game Server -->> Player : 오류 응답
	else 존재 O
		Game Server -->> Player : MailBox 응답
	end
```




------------------------------

## Add Mail
### : 플레이어의 우편함에 우편을 넣는 요청 /mail/add
```mermaid
sequenceDiagram
	actor Admin
	participant Game Server
  	participant GameDB

	Admin ->> Game Server : 우편 추가 요청
	Game Server ->> GameDB : 우편 추가
	GameDB -->> Game Server : 
	Game Server -->> Admin : 성공여부 응답
```





------------------------------

## Receive Mail
### : 플레이어가 자신의 우편함에서 우편을 수령하는 요청 /mail/receive
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  	participant GameDB

	Player ->> Game Server : 우편(아이템) 수령 요청
	Game Server ->> GameDB : 우편함 우편 상태 조회
	GameDB -->> Game Server : 
	alt 수령 불가능 상태
		Game Server -->> Player : 오류 응답
	else 수령 가능 상태
		Game Server ->> GameDB : 수령 상태로 변경
		GameDB ->> GameDB : 아이템 테이블에 추가
		GameDB -->> Game Server : 
		
		Game Server -->> Player : 성공여부 응답
	end
```




------------------------------
