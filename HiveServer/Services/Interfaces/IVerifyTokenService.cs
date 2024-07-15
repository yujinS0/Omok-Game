using HiveServer.DTO;

namespace HiveServer.Services.Interfaces;

public interface IVerifyTokenService
{
    Task<bool> ValidateTokenAsync(string hivePlayerId, string hiveToken);
    Task<VerifyTokenResponse> Verify(VerifyTokenRequest request);
}
