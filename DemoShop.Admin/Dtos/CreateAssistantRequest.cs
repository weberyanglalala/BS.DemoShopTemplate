namespace DemoShop.Admin.Dtos;

public class CreateAssistantRequest
{
    public string Name { get; set; }
    public string Instructions { get; set; }
    public string[] FileIds { get; set; }
}