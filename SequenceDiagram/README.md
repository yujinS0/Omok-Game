# SequenceDiagram

## [Register-Login](https://github.com/yujinS0/Omok-Game/blob/main/SequenceDiagram/Register-Login.md)
* Register : 계정 생성
* Login : 로그인

## [Match](https://github.com/yujinS0/Omok-Game/blob/main/SequenceDiagram/Match.md)
* Request Matching : 매칭 시작 요청
* Check Matching : 매칭 완료 여부 체크


## [GamePlay](https://github.com/yujinS0/Omok-Game/blob/main/SequenceDiagram/GamePlay.md)
* Put Omok : 돌두기 (자기 차례 플레이어)
* Giveup Put Omok : 돌두기 포기 요청 (자기 차례 플레이어)
* Turn Checking : 현재 턴 상태 요청 (차례 대기 플레이어)

* OmokGameData : 게임 데이터 가져오는 요청 (보드정보 + 플레이어 등등)


## [PlayerInfo](https://github.com/yujinS0/Omok-Game/blob/main/SequenceDiagram/PlayerInfo.md)
* Basic Player Data : 플레이어 기본 데이터 가져오는 요청  (닉네임, 레벨, 경험치, 승, 패, 무)
* Update NickName : 닉네임 변경 요청

## [Item](https://github.com/yujinS0/Omok-Game/blob/main/SequenceDiagram/Item.md)
* Get Player Items : 플레이어의 아이템을 가져오는 요청

## [MailBox](https://github.com/yujinS0/Omok-Game/blob/main/SequenceDiagram/MailBox.md)
* Get Player MailBox : 플레이어의 우편함 리스트 받아오는 요청
* Read Mail : 우편을 보는 요청
* Receive item : 우편에 있는 아이템 수령하는 요청
* Delete Mail : 우편을 삭제하는 요청
