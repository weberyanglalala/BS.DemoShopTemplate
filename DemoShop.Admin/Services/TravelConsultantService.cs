using System.Net.Http.Headers;
using System.Text;
using DemoShop.Admin.Dtos;
using DemoShop.Admin.Models.Settings;
using Newtonsoft.Json.Linq;

namespace DemoShop.Admin.Services;

public class TravelConsultantService
{
    private readonly string _assistantId;
    private readonly OpenAISettings _openAiSettings;
    private readonly IHttpClientFactory _httpClientFactory;

    public TravelConsultantService(LineBotSettings lineBotSettings, OpenAISettings openAiSettings,
        IHttpClientFactory httpClientFactory)
    {
        _openAiSettings = openAiSettings;
        _httpClientFactory = httpClientFactory;
        _assistantId = lineBotSettings.OpenAIAssistantId;
    }

    public async Task<string> GetSingleResponseFromAssistant(string message)
    {
        var currentRun = await CreateThreadAndRun(message);
        await CheckRunStatus(currentRun.ThreadId, currentRun.RunId);
        var resultMessage = await GetMessageByThreadId(currentRun.ThreadId);
        await RemoveThreadByThreadId(currentRun.ThreadId);
        return resultMessage;
    }

    private async Task<OpenAiAssistantRun> CreateThreadAndRun(string message)
    {
        var client = CreateHttpClient();
        var endpoint = _openAiSettings.AssistantRunAPIUrl;

        var payload = new
        {
            assistant_id = _assistantId,
            thread = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = message
                    }
                }
            },
            temperature = 0.2,
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(responseContent);

        return new OpenAiAssistantRun
        {
            RunId = obj["id"].ToString(),
            ThreadId = obj["thread_id"].ToString()
        };
    }

    private async Task CheckRunStatus(string threadId, string runId)
    {
        var client = CreateHttpClient();
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}/runs/{runId}";
        int retryCount = 0;
        const int maxRetries = 10;
        const int retryDelay = 5;

        while (true)
        {
            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode && retryCount < maxRetries)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(responseContent);
                var status = obj["status"].ToString();

                if (status == "completed")
                {
                    return;
                }
            }
            else if (retryCount >= maxRetries)
            {
                throw new Exception($"Run failed after {maxRetries} attempts");
            }

            retryCount++;
            await Task.Delay(TimeSpan.FromSeconds(retryDelay));
        }
    }

    private async Task<string> GetMessageByThreadId(string threadId)
    {
        var client = CreateHttpClient();
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}/messages";
        var response = await client.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(responseContent);
        return result["data"][0]["content"][0]["text"]["value"].ToString();
    }

    private async Task RemoveThreadByThreadId(string threadId)
    {
        var client = CreateHttpClient();
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}";
        var response = await client.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);
        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
        return client;
    }
}