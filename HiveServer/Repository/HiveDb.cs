using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using HiveServer.Services;
using Microsoft.Extensions.Options;
using System.Transactions;

namespace HiveServer.Repository
{
    public class HiveDb : IHiveDb
    {
        private readonly IOptions<DbConfig> _dbConfig;
        private readonly ILogger<HiveDb> _logger;
        private MySqlConnection _connection;
        readonly QueryFactory _queryFactory;
        private readonly int _tokenExpiryHours;

        public HiveDb(IOptions<DbConfig> dbConfig, ILogger<HiveDb> logger)
        {
            _dbConfig = dbConfig;
            _logger = logger;

            _connection = new MySqlConnection(_dbConfig.Value.MysqlHiveDBConnection); 
            _connection.Open();

            _queryFactory = new QueryFactory(_connection, new MySqlCompiler());
            _tokenExpiryHours = dbConfig.Value.TokenExpiryHours;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        public async Task<ErrorCode> RegisterAccount(string hiveUserId, string hiveUserPw)
        {
            using (var transaction = await _connection.BeginTransactionAsync()) // 트랜잭션 시작
            {
                try
                {
                    var salt = Security.SaltString();
                    var hashedPassword = Security.MakeHashingPassWord(salt, hiveUserPw);

                    var id = await _queryFactory.Query("account")
                                        .InsertGetIdAsync<int>(new
                                        {
                                            hive_user_id = hiveUserId,
                                            hive_user_pw = hashedPassword,
                                            salt = salt
                                        }, transaction); // 트랜잭션 전달

                    _logger.LogInformation($"Account successfully registered with ID: {id}.");

                    //TODO: 여기에서 실패가 발새했을 때 바로 위의 InsertGetIdAsync을 롤백해야 합니다.
                    //=> 수정 완료했습니다. (트랜잭션 사용)

                    // login_token 테이블에 "기본 데이터" 삽입
                    var tokenResult = await InitializeLoginToken(hiveUserId, transaction);
                    if (tokenResult != ErrorCode.None)
                    {
                        _logger.LogError("Failed to initialize token entry for UserId: {UserId}", hiveUserId);
                        await transaction.RollbackAsync();
                        return tokenResult;
                    }
                    await transaction.CommitAsync();
                    return ErrorCode.None; // Success
                }
                catch (MySqlException ex)
                {
                    _logger.LogError(ex, "Database error when registering account with UserId: {UserId}", hiveUserId);
                    await transaction.RollbackAsync();
                    return ErrorCode.DatabaseError;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register account with UserId: {UserId}", hiveUserId);
                    await transaction.RollbackAsync();
                    return ErrorCode.InternalError;
                }
            }
        }

        // login_token 테이블에 기본 세팅 (초기화 담당)
        private async Task<ErrorCode> InitializeLoginToken(string hiveUserId, MySqlTransaction transaction)
        {
            try
            {
                await _queryFactory.Query("login_token").InsertAsync(new
                {
                    hive_user_id = hiveUserId,
                    hive_token = "", // 빈 문자열로 초기화
                    create_dt = DateTime.UtcNow,
                    expires_dt = DateTime.UtcNow.AddHours(_tokenExpiryHours)
            }, transaction);

                _logger.LogInformation("Token entry initialized successfully for UserId: {UserId}", hiveUserId);
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error when initializing token entry for UserId: {UserId}", hiveUserId);
                return ErrorCode.DatabaseError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize token entry for UserId: {UserId}", hiveUserId);
                return ErrorCode.InternalError;
            }
        }


        // 하이브 로그인 시 id와 pw가 일치하는지 salt적용해서 검증하는 함수
        public async Task<ErrorCode> VerifyUser(string hiveUserId, string hiveUserPw)
        {
            try
            {
                //TODO: 변수는 user 인데 내용은 player 이라서 서로 일치가 되지 않습니다. 클라이언트를 지칭하는게 hive에서는 user, game에서는 player로 통일해주세요.
                //=> 수정 완료했습니다.
                // 1. 사용자 정보 가져오기
                var user = await _queryFactory.Query("account")
                                              .Select("hive_user_id", "hive_user_pw", "salt")
                                              .Where("hive_user_id", hiveUserId)
                                              .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", hiveUserId);
                    return ErrorCode.UserNotFound;
                }

                // 2. 입력된 비밀번호 해싱 (salt 값으로)
                var hashedInputPassword = Security.MakeHashingPassWord(user.salt, hiveUserPw);

                // 3. 해싱된 비밀번호를 비교
                if (user.hive_user_pw != hashedInputPassword)
                {
                    _logger.LogWarning("Password mismatch for UserId: {UserId}", hiveUserId);
                    return ErrorCode.LoginFailPwNotMatch;
                }

                _logger.LogInformation("User verified successfully with ID: {UserId}", hiveUserId);
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error when verifying user with UserId: {UserId}", hiveUserId);
                return ErrorCode.DatabaseError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify user with UserId: {UserId}", hiveUserId);
                return ErrorCode.InternalError;
            }
        }

        // login_token 테이블에 token 값 업데이트하는 함수 (실패시 false 반환)
        public async Task<bool> SaveToken(string hiveUserId, string token)
        {
            try
            {
                var expirationTime = DateTime.UtcNow.AddHours(_tokenExpiryHours);

                var affectedRows = await _queryFactory.Query("login_token")
                                              .Where("hive_user_id", hiveUserId)
                                              .UpdateAsync(new
                                              {
                                                  hive_token = token,
                                                  create_dt = DateTime.UtcNow,
                                                  expires_dt = expirationTime
                                              });

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Token successfully saved for UserId: {UserId}", hiveUserId);
                    return true;
                }

                _logger.LogWarning("No rows affected when saving token for UserId: {UserId}", hiveUserId);
                return false;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error when saving token for UserId: {UserId}", hiveUserId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save token for UserId: {UserId}", hiveUserId);
                return false;
            }
        }

        // login_token 테이블에서 hive_user_id에 해당하는 토큰 값을 검증하는 함수
        public async Task<bool> ValidateTokenAsync(string hiveUserId, string token)
        {
            try
            {
                var query = _queryFactory.Query("login_token")
                                         .Select("hive_token", "expires_dt")
                                         .Where("hive_user_id", hiveUserId);

                var tokenData = await query.FirstOrDefaultAsync();

                if (tokenData == null)
                {
                    _logger.LogWarning("Token not found for UserId: {UserId}", hiveUserId);
                    return false;
                }

                var storedToken = tokenData.hive_token;
                var expirationTime = tokenData.expires_dt;

                if (storedToken == token && expirationTime > DateTime.UtcNow) // 토큰 일치 여부 및 유효 시간 확인
                {
                    _logger.LogInformation("Token validated successfully for UserId: {UserId}", hiveUserId);
                    return true;
                }

                _logger.LogWarning("Token validation failed for UserId: {UserId}", hiveUserId);
                return false;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Database error when validating token for UserId: {UserId}", hiveUserId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate token for UserId: {UserId}", hiveUserId);
                return false;
            }
        }

    }
}