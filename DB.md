# MasterData
### attendance_reward 테이블
```sql
  CREATE TABLE IF NOT EXISTS attendance_reward (
      day_seq INT,
      reward_item INT,
      item_count INT
  );
```

* 초기 데이터 (임시 수동 입력)
```sql
  INSERT INTO attendance_reward (day_seq, reward_item, item_count) VALUES
  (1, 1, 100), (2, 1, 100), (3, 1, 100), (4, 1, 100), (5, 1, 100), (6, 1, 100), (7, 1, 200), (8, 1, 200), (9, 1, 200), (10, 1, 200),
  (11, 1, 200), (12, 2, 10), (13, 2, 10), (14, 2, 10), (15, 2, 10), (16, 2, 10), (17, 2, 10), (18, 2, 10), (19, 2, 10), (20, 2, 10),
  (21, 2, 10), (22, 2, 10), (23, 2, 20), (24, 2, 20), (25, 2, 20), (26, 2, 20), (27, 2, 20), (28, 2, 20), (29, 2, 20), (30, 2, 20),
  (31, 3, 1);
```

### item 테이블
```sql
  CREATE TABLE item (
    item_code INT,
    name VARCHAR(64) NOT NULL,
    description VARCHAR(128) NOT NULL
  );
```

* 초기 데이터 (임시 수동 입력)
```sql
INSERT INTO item (item_code, name, description) VALUES
  (1, '돈', '게임 머니'),
  (2, '실버', '실버(은) 보석'),
  (3, '무르기 아이템', ''),
  (4, '닉네임변경', '');
```


### first_item 테이블
```sql
CREATE TABLE first_item (
    item_code INT,
    count INT
  );
```

* 초기 데이터 (임시 수동 입력)
```sql
INSERT INTO first_item (item_code, count) VALUES
  (1, 1000),
  (3, 1),
  (4, 1);
```


### version 테이블
```sql
CREATE TABLE version (
    app_version VARCHAR(64),
    master_data_version VARCHAR(64)
  );
```

* 초기 데이터 (임시 수동 입력)
```sql
INSERT INTO version (app_version, master_data_version) VALUES
  ('0.1.0', '0.1.0');
```


---------------------------------------

# HiveDB
### account 테이블
```sql
  CREATE TABLE account (
    account_uid INT AUTO_INCREMENT PRIMARY KEY,
    hive_player_id VARCHAR(255) NOT NULL UNIQUE,
    hive_player_pw CHAR(64) NOT NULL,  -- SHA-256 해시 결과는 항상 64 길이의 문자열
    create_dt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    salt CHAR(64) NOT NULL
  );
```

### login_token 테이블
```sql
  CREATE TABLE login_token (
    hive_player_id VARCHAR(255) NOT NULL PRIMARY KEY,
    hive_token CHAR(64) NOT NULL,
    create_dt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_dt DATETIME NOT NULL
  );
```


---------------------------------------

# GameDB

### player_info 테이블

```sql
CREATE TABLE player_info (
	player_uid BIGINT AUTO_INCREMENT PRIMARY KEY,
	hive_player_id VARCHAR(255) NOT NULL UNIQUE,
	nickname VARCHAR(100),
	exp INT,
	level INT,
	win INT,
	lose INT,
	draw INT,
	create_dt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### player_item 테이블 

```sql
CREATE TABLE IF NOT EXISTS player_item (
	player_item_uid BIGINT AUTO_INCREMENT PRIMARY KEY,
    	player_uid INT NOT NULL COMMENT '플레이어 UID',
    	item_code INT NOT NULL COMMENT '아이템 ID',
    	item_cnt INT NOT NULL COMMENT '아이템 수'
);
```


### mailbox 테이블

```sql
CREATE TABLE mailbox (
	mail_id BIGINT AUTO_INCREMENT NOT NULL PRIMARY KEY,
	title VARCHAR(300) NOT NULL,
	item_code INT NOT NULL,
	item_cnt INT NOT NULL,
	send_dt TIMESTAMP NOT NULL,
	expire_dt TIMESTAMP NOT NULL,
	receive_dt TIMESTAMP NULL,
	receive_yn TINYINT NOT NULL DEFAULT 0 COMMENT '수령 유무',
	FOREIGN KEY (player_uid) REFERENCES player_info(player_uid)
);
```
