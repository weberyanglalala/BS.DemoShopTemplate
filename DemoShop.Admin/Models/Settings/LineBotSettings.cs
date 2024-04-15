namespace DemoShop.Admin.Models.Settings;

public class LineBotSettings
{
    public const string SettingKey = "LineBotSettings";

    public string AdminUserId { get; set; }
    public string ChannelAccessToken { get; set; }
    public string OpenAIAssistantId { get; set; }
}