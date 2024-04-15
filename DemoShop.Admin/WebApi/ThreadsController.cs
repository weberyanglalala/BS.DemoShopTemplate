using DemoShop.Admin.Dtos;
using DemoShop.Admin.Models.Settings;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DemoShop.Admin.WebApi;

[Route("api/[controller]/[action]")]
[ApiController]
public class ThreadsController : ControllerBase
{
    private readonly OpenAISettings _openAiSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public ThreadsController(IHttpClientFactory httpClientFactory, OpenAISettings openAiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _openAiSettings = openAiSettings;
    }

    [HttpPost]
    public async Task<IActionResult> CreateThreadAndRun([FromBody] CreateThreadAndRunRequest request)
    {
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);

        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

        var endpoint = _openAiSettings.AssistantRunAPIUrl;

        var payload = new
        {
            assistant_id = request.AssistantId,
            thread = new
            {
                messages = request.Thread.Messages
            }
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return Ok(new
            {
                IsSuccess = true,
                Body = responseContent,
                Message = "Thread created successfully."
            });
        }
        else
        {
            return Ok(new
            {
                IsSuccess = false,
                Message = "Failed to create a thread"
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteThread([FromBody] DeleteThreadRequest request)
    {
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);

        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{request.ThreadId}";

        var response = await client.DeleteAsync(endpoint);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return Ok(new
            {
                IsSuccess = true,
                Body = result,
                Message = "Thread deleted successfully."
            });
        }
        else
        {
            return Ok(new
            {
                IsSuccess = false,
                Message = "Thread failed to be deleted."
            });
        }
    }
}