using System.Net.Http.Headers;
using System.Text;
using DemoShop.Admin.Dtos;
using DemoShop.Admin.Models.Settings;
using Newtonsoft.Json.Linq;

namespace DemoShop.Admin.Services;

public class TravelConsultantService
{
    private readonly string _assistantId; // 儲存 Assistant ID
    private readonly OpenAISettings _openAiSettings; // 儲存 OpenAI 設定
    private readonly IHttpClientFactory _httpClientFactory; // 用於建立 HttpClient 實例

    public TravelConsultantService(LineBotSettings lineBotSettings, OpenAISettings openAiSettings,
        IHttpClientFactory httpClientFactory)
    {
        _openAiSettings = openAiSettings; // 注入 OpenAI 設定
        _httpClientFactory = httpClientFactory; // 注入 HttpClientFactory
        _assistantId = lineBotSettings.OpenAIAssistantId; // 從 LineBotSettings 取得 Assistant ID
    }

    public async Task<string> GetSingleResponseFromAssistant(string message)
    {
        var currentRun = await CreateThreadAndRun(message); // 建立新的對話執行緒並執行
        await CheckRunStatus(currentRun.ThreadId, currentRun.RunId); // 檢查執行狀態直到完成
        var resultMessage = await GetMessageByThreadId(currentRun.ThreadId); // 取得執行緒的回應訊息
        await RemoveThreadByThreadId(currentRun.ThreadId); // 移除執行緒
        return resultMessage; // 回傳結果訊息
    }

    private async Task<OpenAiAssistantRun> CreateThreadAndRun(string message)
    {
        var client = CreateHttpClient(); // 建立 HttpClient
        var endpoint = _openAiSettings.AssistantRunAPIUrl; // 取得 Assistant Run API 端點

        var payload = new // 建立請求內容
        {
            assistant_id = _assistantId, // 設定 Assistant ID
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
            temperature = 0.2, // 設定生成溫度
        };

        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload); // 序列化請求內容
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"); // 建立 StringContent
        var response = await client.PostAsync(endpoint, content); // 發送 POST 請求

        response.EnsureSuccessStatusCode(); // 確保回應成功
        var responseContent = await response.Content.ReadAsStringAsync(); // 讀取回應內容
        var obj = JObject.Parse(responseContent); // 解析 JSON

        return new OpenAiAssistantRun // 回傳 OpenAiAssistantRun 物件
        {
            RunId = obj["id"].ToString(), // 設定 RunId
            ThreadId = obj["thread_id"].ToString() // 設定 ThreadId
        };
    }

    private async Task CheckRunStatus(string threadId, string runId)
    {
        var client = CreateHttpClient(); // 建立 HttpClient 
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}/runs/{runId}"; // 建立端點 URL
        int retryCount = 0; // 重試次數
        const int maxRetries = 10; // 最大重試次數
        const int retryDelay = 5; // 重試延遲秒數

        while (true)
        {
            var response = await client.GetAsync(endpoint); // 發送 GET 請求

            if (response.IsSuccessStatusCode && retryCount < maxRetries) // 如果回應成功且重試次數小於最大值
            {
                var responseContent = await response.Content.ReadAsStringAsync(); // 讀取回應內容
                var obj = JObject.Parse(responseContent); // 解析 JSON
                var status = obj["status"].ToString(); // 取得執行狀態

                if (status == "completed") // 如果執行完成
                {
                    return; // 結束方法
                }
            }
            else if (retryCount >= maxRetries) // 如果重試次數達到最大值
            {
                throw new Exception($"Run failed after {maxRetries} attempts"); // 拋出例外
            }

            retryCount++; // 增加重試次數
            await Task.Delay(TimeSpan.FromSeconds(retryDelay)); // 延遲指定秒數
        }
    }

    private async Task<string> GetMessageByThreadId(string threadId)
    {
        var client = CreateHttpClient(); // 建立 HttpClient
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}/messages"; // 建立端點 URL
        var response = await client.GetAsync(endpoint); // 發送 GET 請求

        response.EnsureSuccessStatusCode(); // 確保回應成功
        var responseContent = await response.Content.ReadAsStringAsync(); // 讀取回應內容
        var result = JObject.Parse(responseContent); // 解析 JSON
        return result["data"][0]["content"][0]["text"]["value"].ToString(); // 回傳訊息內容
    }

    private async Task RemoveThreadByThreadId(string threadId)
    {
        var client = CreateHttpClient(); // 建立 HttpClient
        var endpoint = $"{_openAiSettings.AssistantThreadAPIUrl}/{threadId}"; // 建立端點 URL
        var response = await client.DeleteAsync(endpoint); // 發送 DELETE 請求
        response.EnsureSuccessStatusCode(); // 確保回應成功
    }

    private HttpClient CreateHttpClient()
    {
        var client = _httpClientFactory.CreateClient(); // 使用 HttpClientFactory 建立 HttpClient
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey); // 設定 Authorization Header
        client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1"); // 設定自訂 Header
        return client; // 回傳 HttpClient 實例
    }
}
