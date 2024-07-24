# HiveDB
### Account 테이블
```sql
  CREATE TABLE account (
    account_uid INT AUTO_INCREMENT PRIMARY KEY,
    hive_player_id VARCHAR(255) NOT NULL UNIQUE,
    hive_player_pw CHAR(64) NOT NULL,  -- SHA-256 해시 결과는 항상 64 길이의 문자열
    create_dt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    salt CHAR(64) NOT NULL
  );
```

### Login_Token 테이블
```sql
  CREATE TABLE login_token (
    hive_player_id VARCHAR(255) NOT NULL PRIMARY KEY,
    hive_token CHAR(64) NOT NULL,
    create_dt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_dt DATETIME NOT NULL
  );
```



# GameDB

### Char_Info 테이블

```sql
  CREATE TABLE char_info (
    char_uid INT AUTO_INCREMENT PRIMARY KEY,
    hive_player_id VARCHAR(255) NOT NULL UNIQUE,
    char_name VARCHAR(100),
    char_exp INT,
    char_level INT,
    char_win INT,
    char_lose INT,
    char_draw INT,
    create_dt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  );
```
