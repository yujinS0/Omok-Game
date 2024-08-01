# Omok-Game
2024 컴투스 지니어스 - ASP.NET core 를 사용한 API Server 학습을 위한 오목 게임 프로젝트

* [시퀀스 다이어그램](https://github.com/yujinS0/Omok-Game/tree/main/SequenceDiagram)
* [데이터베이스 스키마](https://github.com/yujinS0/Omok-Game/blob/main/DB.md)
* [MasterData(기획데이터)](https://github.com/yujinS0/Omok-Game/blob/main/MasterData.xlsx)

# TODO-LIST
앞으로의 개발 계획 및 진행상황 공유
  
완료한 작업 : ✅

## 구현해야 할 기능
| 기능                          | 완료 여부 | 서버 | 클라 |
| ----------------------------- | --------- | --- | ---|
| 계정 생성 				          |  ✅      |  ✅  |  ✅  |
| 로그인					  	        |  ✅      |  ✅  |  ✅  |
| 매칭요청					        	|  ✅      |  ✅  |  ✅  |
| 오목 게임 플레이				    |  ✅      |  ✅  |  ✅  |
| 게임 결과 저장	  			    |  ✅      |  ✅  |  ✅  |
| 유저 게임 데이터 표시				|  ✅      |  ✅  |  ✅  |
| 마스터 데이터(기획 데이터)		|  ✅      |  ✅  |  ✅  |
| 게임 아이템				      		|        |  ✅  |    |
| 우편함		          		    |        |    |    |
| 출석부			              	|        |    |    |
| 친구			            		|        |    |    |
| 상점  		              	|        |    |    |
| 오목 게임 리플레이 (복기)	|        |    |    |

---------------------------
## 일정 TODO
| 날짜               | 기능                  | 완료 |
| ----------------- | --------------------- | ------- |
| 7/23 화				    |  오목 게임 플레이         |   ✅   |
| 7/23 화  			    |  게임 결과 저장           |  ✅   |
| 7/23 화			    	|   유저 게임 데이터 표시     |  ✅    |
| 7/24 수				    |    시퀀스 다이어그램    |  ✅    |
| 7/26 금				    |    MasterDB       |   ✅    |         
| 7/29 월           |  MasterDB 완료     |   ✅   |
| 7/31 수            |       인벤토리(아이템)       |         |
| 8/1 목           |       우편함        |         |
| 8/5 월			     	|    출석부       |        |
| 8/5 월 		       	|   친구 DB 설계     |        |
| 8/6 화  		       	|   친구 기능     |         |
| 8/7 수          	|    상점    |        |
|             	|   오목 게임 리플레이     |        |
<br>

## 상세 일정

<details>
<summary>완료</summary>

> ## 7/23 화 - 게임 결과 저장 및 표시 ✅
>> **오목 게임 플레이**
>
>> **게임 결과 저장**
>> * 게임 종료 시 gameDB에 저장하는 스레드 생성
>
>> **유저 게임 데이터 표시**

<br>

> ## 7/24 수 - 리펙토링 및 시퀀스 다이어그램 ✅
>> **매칭 리펙토링**
>
>> **시퀀스 다이어그램**
>

<br>


> ## 7/25 목 
>> **게임 서버 리펙토링** 
> 시퀀스 다이어그램에 맞게 API 수정
>
>> **클라이언트 수정**
>  * API 클라에 맞게 수정
> 

> ## 7/26 금 - 게임 아이템 (MasterDB)
>> **클라이언트 화면 수정**
>  * 게임 UI
>  * 주요 기능 버튼 바 
> 
>> **MasterDB (기획 데이터) 관련 생성**
>
>> **클라 오목 게임 화면에 인벤토리 버튼**

<br>

> ## 7/29 월 - MasterDB 
>> **MasterDb 생성 및 연동**
>
>> **플레이어 아이템 DB 생성**
>
>> **클라 오목 게임 화면에 인벤토리 표시**
>


</details>

<br>





----------------------------------------

<br>



> ## ~ 8/1 목 - 우편함 + 아이템(인벤토리)

>> **우편함 DB 생성**
>> * 최대 100개까지만 저장 (이후로는 예전 것 삭제)
>> * 각 아이템마다 수령 가능 시간 존재
>
>> **아이템 수령 기능**
>> * 우편함에서 **아이템** 수령 시 인벤토리에 저장되도록
>



> ## ~ 8/5 월 - 출석부
>> **출석부 DB 생성**
>> * 월에 31번 
>
>> **출석 API**
>> * 요청 시점의 날짜를 바탕으로 DB에 출석 처리하기
>
>> **출석 보상 기능**
>> * 출석 완료 시 보상 자동적으로 우편함에 들어가기
>
>> **클라이언트 표시**
>

<br>

> ## 8/6 화 - 친구
>> **친구 DB 설계**
>
>> **친구 DB 생성**
>
>> **친구 요청 API**
>
>> **요청 수락 API**
>
>> **클라이언트 친구 페이지**

<br>

> ## 8/7 수 - 상점
>> 상점 페이지 생성
>
>> 완성된 아이템 판매
>
>> 클라이언트 추가
>

> ## 8/7 목  - 상점 완료
>> 상점 클라이언트 완료




<br>
<br>

> ## TODO
>> **아이템 컨텐츠 개발**
>> * 아이템1 : 한 수 무르기
>>   + 사용 시점 : 자신의 차례
>
>> * 아이템2 : 닉네임 변경권
>
>> * 추가)아이템 : 상대 시간 10초 감소 (총 20초)
>>   + 사용 시점 : 자신의 차례 (사용 시 다음 상태 턴 감소)
>> * 추가)아이템 : 자기 시간 10초 증가 (총 40초)
>>   + 사용 시점 : 자신의 차례 (단 5초 이하 남았을 때는 사용 불가)
>

-------------------------------

## Server 별 API 
**하이브 서버**
 
| 기능                          | 완료 여부 |
| ----------------------------- | --------- |
| [하이브 계정생성 API]   				    | ✅        |
| [하이브 로그인 API]						  	| ✅        |
| [하이브 토큰 검증 API]							| ✅        |

**게임 서버**

| 기능                           | 완료 여부 |
| -------------------------------| --------- |
| [로그인(토큰 검증 API)]						         |  ✅       |
| [매칭 요청 API]								     |   ✅      |
| [매칭 확인 API]								     |   ✅      |

| 기능                           | 완료 여부 |
| -------------------------------| --------- |
| [게임 시작 API]	             |         |
| [돌 두기 API]	             |         |
| ?             |         |


| 기능                           | 완료 여부 |
| -------------------------------| --------- |
| [유저 데이터 로드]	             |         |
| [게임 데이터 로드]	             |         |

**매칭 서버**

| 기능                           | 완료 여부 |
| -------------------------------| --------- |
| [매칭 요청 API]								     |   ✅      |
| [매칭 처리 스레드]								     |   ✅      |





...


<br>  
  
---  
아래는 구현할 기능에 대한 설명이다.    
  
### 하이브 로그인

**컨텐츠 설명**
- 하이브에 로그인 하여 이메일과 토큰을 받습니다.

**로직**
1. 클라이언트가 이메일과 비밀번호를 하이브 서버에 전달한다.
1. 클라이언트의 이메일과 생성된 토큰을 응답한다. 



### 요청 및 응답 예시

- 요청 예시

```
POST http://localhost:5284/Login
Content-Type: application/json

{
  "hive_player_id": "user@example.com",
  "hive_player_pw": "string"
}
```

- 응답 예시

```
{
  "result": 0,
  "hive_player_id": "user@example.com",
  "hive_token": "string"
}
```
  
  
---
