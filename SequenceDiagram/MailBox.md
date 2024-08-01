# 시퀀스 다이어그램 (MailBox)

------------------------------

## Get Player MailBox
### : 플레이어의 우편함을 가져오는 요청 /mail/get-mailbox
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  participant GameDB

	Player ->> Game Server :  로드 요청 (with page number)
	Game Server ->> GameDB : PlayerItem 가져오기
	GameDB -->> Game Server : 
	alt 존재 X
		Game Server -->> Player : 오류 응답
	else 존재 O
		Game Server -->> Player : PlayerItem 응답
	end
```




------------------------------

## Add Mail
### : 플레이어의 우편함에 우편을 넣는 요청 /mail/add
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  participant GameDB

	Player ->> Game Server : 아이템 로드 요청 (with page number)
	Game Server ->> GameDB : PlayerItem 가져오기
	GameDB -->> Game Server : 
	alt 존재 X
		Game Server -->> Player : 오류 응답
	else 존재 O
		Game Server -->> Player : PlayerItem 응답
	end
```





------------------------------

## Receive Mail
### : 플레이어가 자신의 우편함에서 우편을 수령하는 요청 /mail/receive
```mermaid
sequenceDiagram
	actor Player
	participant Game Server
  participant GameDB

	Player ->> Game Server : 아이템 로드 요청 (with page number)
	Game Server ->> GameDB : PlayerItem 가져오기
	GameDB -->> Game Server : 
	alt 존재 X
		Game Server -->> Player : 오류 응답
	else 존재 O
		Game Server -->> Player : PlayerItem 응답
	end
```




------------------------------
