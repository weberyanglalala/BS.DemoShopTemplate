using System.Text.Json.Serialization;

namespace DemoShop.Admin.Dtos;

public class CreateThreadAndRunRequest
{
    [JsonPropertyName("assistant_id")]
    public string AssistantId { get; set; }

    [JsonPropertyName("thread")]
    public ThreadDto Thread { get; set; }
}

public class ThreadDto
{
    [JsonPropertyName("messages")]
    public List<MessageDto> Messages { get; set; }
}

public class MessageDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}