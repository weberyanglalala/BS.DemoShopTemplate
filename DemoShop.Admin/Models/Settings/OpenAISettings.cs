namespace DemoShop.Admin.Models.Settings;

public class OpenAISettings
{
    public const string SettingKey = "OpenAISettings";
    
    public string ApiKey { get; set; }
    public string FileUploadUrl { get; set; }
    public string GetAllFilesUrl { get; set; }
    public string AssistantsUrl { get; set; }
    public string AssistantThreadAPIUrl { get; set; }
    public string AssistantRunAPIUrl { get; set; }
}