using System.Net.Http.Headers;
using DemoShop.Admin.Models.Settings;
using Microsoft.AspNetCore.Mvc;

namespace DemoShop.Admin.WebApi;

[Route("api/[controller]/[action]")]
[ApiController]
public class FileUploadController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAISettings _openAiSettings;

    public FileUploadController(IConfiguration configuration, IHttpClientFactory httpClientFactory, OpenAISettings openAiSettings)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _openAiSettings = openAiSettings;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFileToOpenAi(IFormFile file)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);

        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StringContent("assistants"), "purpose");
            await using (var fileStream = file.OpenReadStream())
            {
                content.Add(new StreamContent(fileStream), "file", file.FileName);
                var response = await client.PostAsync(_openAiSettings.FileUploadUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return Ok(new
                    {
                        IsSuccess = true,
                        Body = responseContent,
                        Message = "File uploaded successfully."
                    });
                }

                return Ok(new
                {
                    IsSuccess = false,
                    Message = "Failed to upload file."
                });
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFilesFromOpenAI()
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _openAiSettings.ApiKey);
        var response = await client.GetAsync(_openAiSettings.GetAllFilesUrl);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return Ok(new
            {
                IsSuccess = true,
                Body = responseContent,
                Message = "Files retrieved successfully."
            });
        }
        else
        {
            return Ok(new
            {
                IsSuccess = false,
                Message = "Failed to retrieve files."
            });
        }
    }
}