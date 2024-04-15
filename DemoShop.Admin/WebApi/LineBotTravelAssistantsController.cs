using System.Net.Http.Headers;
using System.Text;
using DemoShop.Admin.Models.Settings;
using DemoShop.Admin.Services;
using isRock.LineBot;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DemoShop.Admin.WebApi;

public class LineBotTravelAssistantsController : LineWebHookControllerBase
{
    private readonly LineBotSettings _lineBotSettings;
    private readonly TravelConsultantService _travelConsultantService;

    public LineBotTravelAssistantsController(LineBotSettings lineBotSettings, TravelConsultantService travelConsultantService)
    {
        _lineBotSettings = lineBotSettings;
        _travelConsultantService = travelConsultantService;
    }

    [Route("api/LineBotChatGPTWebHook")]
    [HttpPost]
    public async Task<IActionResult> POST()
    {
        string adminUserId = _lineBotSettings.AdminUserId;
        try
        {
            //設定ChannelAccessToken
            this.ChannelAccessToken = _lineBotSettings.ChannelAccessToken;
            //配合Line Verify
            if (ReceivedMessage.events == null || ReceivedMessage.events.Count() <= 0 ||
                ReceivedMessage.events.FirstOrDefault().replyToken == "00000000000000000000000000000000")
                return Ok();
            //取得Line Event
            var LineEvent = this.ReceivedMessage.events.FirstOrDefault();
            var responseMsg = "";
            //準備回覆訊息
            if (LineEvent.type.ToLower() == "message" && LineEvent.message.type == "text")
            {
                // responseMsg = ChatGPT.getResponseFromGPT(LineEvent.message.text);
                responseMsg = await _travelConsultantService.GetSingleResponseFromAssistant(LineEvent.message.text);
            }
            else if (LineEvent.type.ToLower() == "message")
                responseMsg = $"收到 event : {LineEvent.type} type: {LineEvent.message.type} ";
            else
                responseMsg = $"收到 event : {LineEvent.type} ";

            //回覆訊息
            this.ReplyMessage(LineEvent.replyToken, responseMsg);
            //response OK
            return Ok();
        }
        catch (Exception ex)
        {
            //回覆訊息
            this.PushMessage(adminUserId, "發生錯誤:\n" + ex.Message);
            //response OK
            return Ok();
        }
    }
}