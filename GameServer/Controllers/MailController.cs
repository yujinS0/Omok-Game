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
        if (HttpContext.Items.TryGetValue("PlayerUid", out var playerUidObj) && playerUidObj is long playerUid)
        {
            var (errorCode, mailIds, titles, itemCodes, sendDates, expiryDurations, receiveYns) = await _mailService.GetPlayerMailBox(playerUid, request.PageNum);

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
        else
        {
            return new MailBoxResponse
            {
                Result = ErrorCode.PlayerUidNotFound,
                MailIds = null,
                Titles = null,
                ItemCodes = null,
                SendDates = null,
                ExpiryDurations = null,
                ReceiveYns = null
            };
        }
    }

    [HttpPost("read")]
    public async Task<MailDetailResponse> ReadPlayerMail([FromBody] ReadMailRequest request)
    {
        if (HttpContext.Items.TryGetValue("PlayerUid", out var playerUidObj) && playerUidObj is long playerUid)
        {
            var (errorCode, mailDetail) = await _mailService.ReadMail(playerUid, request.MailId);

            if (mailDetail == null)
            {
                return new MailDetailResponse
                {
                    Result = errorCode,
                    MailId = -1,
                    Title = null,
                    Content = null,
                    ItemCode = -1,
                    ItemCnt = -1,
                    SendDate = null,
                    ExpireDate = null,
                    ReceiveDate = null,
                    ReceiveYn = -1
                };
            }

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
        else
        {
            return new MailDetailResponse
            {
                Result = ErrorCode.PlayerUidNotFound,
                MailId = -1,
                Title = null,
                Content = null,
                ItemCode = -1,
                ItemCnt = -1,
                SendDate = null,
                ExpireDate = null,
                ReceiveDate = null,
                ReceiveYn = -1
            };
        }
    }

    [HttpPost("receive-item")]
    public async Task<ReceiveMailItemResponse> ReceiveMailItem([FromBody] ReceiveMailItemRequest request)
    {
        if (HttpContext.Items.TryGetValue("PlayerUid", out var playerUidObj) && playerUidObj is long playerUid)
        {
            var (result, isReceived) = await _mailService.ReceiveMailItem(playerUid, request.MailId);

            return new ReceiveMailItemResponse
            {
                Result = result,
                IsAlreadyReceived = isReceived
            };
        }
        else
        {
            return new ReceiveMailItemResponse
            {
                Result = ErrorCode.PlayerUidNotFound,
                IsAlreadyReceived = null
            };
        }
    }

    [HttpPost("delete")]
    public async Task<DeleteMailResponse> DeleteMail([FromBody] DeleteMailRequest request)
    {
        if (HttpContext.Items.TryGetValue("PlayerUid", out var playerUidObj) && playerUidObj is long playerUid)
        {
            var result = await _mailService.DeleteMail(playerUid, request.MailId);
            return new DeleteMailResponse
            {
                Result = result
            };
        }
        else
        {
            return new DeleteMailResponse
            {
                Result = ErrorCode.PlayerUidNotFound
            };
        }
    }
}