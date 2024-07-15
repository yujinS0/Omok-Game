using System.Threading.Tasks;
using HiveServer.DTO;

namespace HiveServer.Services.Interfaces;
public interface ILoginService
{
    Task<(ErrorCode, string)> VerifyUser(string hivePlayerId, string hivePlayerPw);
    Task<bool> SaveToken(string hivePlayerId, string token);
    Task<LoginResponse> Login(LoginRequest request);
}
