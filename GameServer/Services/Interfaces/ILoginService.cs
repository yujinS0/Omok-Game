using GameServer.DTO;
using GameServer.Models;

namespace GameServer.Services.Interfaces;

public interface ILoginService
{
    Task<LoginResponse> Login(LoginRequest request);
}
