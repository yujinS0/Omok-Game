using Microsoft.AspNetCore.Mvc;
using HiveServer.DTO;
using HiveServer.Services.Interfaces;

namespace HiveServer.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly ILoginService _loginService;

    public LoginController(ILogger<LoginController> logger, ILoginService loginService)
    {
        _logger = logger;
        _loginService = loginService;
    }

    [HttpPost]
    public async Task<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var response = await _loginService.Login(request);
        _logger.LogInformation($"[Login] hive_user_id: {request.HiveUserId}, Result: {response.Result}");
        return response;
    }
}