using System.Net.Http.Headers;
using DemoShop.Admin.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace DemoShop.Admin.WebApi;

[Route("api/[controller]/[action]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAISettings _openAiSettings;

    public MessagesController(IHttpClientFactory httpClientFactory, OpenAISettings openAiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _openAiSettings = openAiSettings;
    }

    public async Task<IActionResult> GetMessagesByThreadId(string threadId)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);
        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}/messages";
        var response = await client.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return Ok(new
            {
                IsSuccess = true,
                Body = responseContent,
                Message = "Messages retrieved successfully."
            });
        }
        else
        {
            return Ok(new
            {
                IsSuccess = false,
                Message = "Failed to retrieve messages."
            });
        }
    }
}