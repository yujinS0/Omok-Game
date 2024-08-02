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

    [HttpPost("read")]
    public async Task<MailDetailResponse> ReadPlayerMail([FromBody] ReadMailRequest request)
    {
        var (errorCode, mailDetail) = await _mailService.ReadMail(request.PlayerId, request.MailId);

        return new MailDetailResponse
        {
            Result = errorCode,
            MailId = mailDetail.MailId,
            Title = mailDetail.Title,
            Content = mailDetail.Content,
            ItemCode = mailDetail.ItemCode,
            ItemCnt = mailDetail.ItemCnt,
            SendDate = mailDetail.SendDate,
            ExpireDate = mailDetail.ExpireDate,
            ReceiveDate = mailDetail.ReceiveDate,
            ReceiveYn = mailDetail.ReceiveYn
        };
    }

    [HttpPost("receive-item")]
    public async Task<ErrorCode> ReceiveMailItem([FromBody] ReceiveMailItemRequest request)
    {
        var errorCode = await _mailService.ReceiveMailItem(request.PlayerId, request.MailId);
        return errorCode;
    }

    [HttpPost("delete")]
    public async Task<ErrorCode> DeleteMail([FromBody] DeleteMailRequest request)
    {
        var errorCode = await _mailService.DeleteMail(request.PlayerId, request.MailId);
        return errorCode;
    }
}