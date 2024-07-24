# 시퀀스 다이어그램
## PutOmok

## 오목 돌 두기
### POST PutOmok
```mermaid
sequenceDiagram
	actor User
	participant Game Server
	participant GameDB
  	participant Redis

	User ->> Game Server: 돌 두기 요청
	Game Server ->> Game Server : playingUserKey 생성
 	Game Server ->> Redis : userGameData 가져오기
	Redis ->> Game Server :  
	  Game Server ->> Redis : GameRoomId로 gameData 가져오기
	  Redis ->> Game Server: 

	Game Server ->> Game Server : 자기 턴 맞는지 확인

	Game Server ->> Redis : 돌 두기
	Redis ->> Game Server :  

	Game Server ->> Game Server : 승자 체크 요청
	alt 승자 존재
	  Game Server ->> GameDB : 게임 결과 (승/패) 업데이트
	  GameDB ->> Game Server :   
	end

  	Game Server ->> User : 돌두기 성공 + 승자 정보

```


```css
public class PutOmokRequest
{
    [Required] public string PlayerId { get; set; }
    [Required] public int X { get; set; }
    [Required] public int Y { get; set; }
}
```

```css
public class PutOmokResponse
{
    [Required] public ErrorCode Result { get; set; } = ErrorCode.None;
    public Winner Winner { get; set; }
}
```
