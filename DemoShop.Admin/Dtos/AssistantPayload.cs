namespace DemoShop.Admin.Dtos;

public class AssistantPayload
{
    public string Name { get; set; }
    public string Instructions { get; set; }
    public string Model { get; set; } = "gpt-3.5-turbo-0125"; // 設定默認值
    public Tool[] Tools { get; set; } = new Tool[] { new Tool { Type = "retrieval" } }; // 設定默認值
    public string[] FileIds { get; set; }
}

public class Tool
{
    public string Type { get; set; }
}