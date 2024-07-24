# 시퀀스 다이어그램 (Character)

------------------------------

## POST Character/updatename
### : CharName(닉네임) 업데이트
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

```css
public class UpdateCharacterNameRequest
{
    public string PlayerId { get; set; }
    public string CharName { get; set; }
}

public class UpdateCharacterNameResponse
{
    public ErrorCode Result { get; set; }
}

```

------------------------------

## POST Character/getinfo
### : Character 정보를 가져오는 요청
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



```css
public class CharacterSummaryRequest
{
    public string PlayerId { get; set; }
}

public class CharacterSummaryResponse
{
    public ErrorCode Result { get; set; }
    public CharSummary CharSummary { get; set; }
}

public class CharSummary
{
    public string CharName { get; set; }
    public int Exp { get; set; }
    public int Level { get; set; }
    public int Win { get; set; }
    public int Lose { get; set; }
    public int Draw { get; set; }
}

```

------------------------------

