using GameServer.DTO;
using GameServer.Services;
using GameServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ServerShared;

namespace GameServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MailController : ControllerBase
{
    private readonly ILogger<MailController> _logger;
    private readonly IMailService _mailService;

    public MailController(ILogger<MailController> logger, IMailService mailService)
    {
        _logger = logger;
        _mailService = mailService;
    }

    [HttpPost("get-mailbox")]
    public async Task<MailBoxResponse> GetPlayerMailBox([FromBody] GetPlayerMailBoxRequest request)
    {
        var (errorCode, mailIds, titles, itemCodes, sendDates, expiryDurations, receiveYns) = await _mailService.GetPlayerMailBox(request.PlayerId, request.PageNum);

        return new MailBoxResponse
        {
            Result = errorCode,
            MailIds = mailIds,
            Titles = titles,
            ItemCodes = itemCodes,
            SendDates = sendDates,
            ExpiryDurations = expiryDurations,
            ReceiveYns = receiveYns
        };
    }
}