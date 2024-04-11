using DemoShop.Admin.Helpers;
using DemoShop.Admin.Models.Settings;
using DemoShop.Admin.Services;
using Microsoft.Extensions.Options;

namespace DemoShop.Admin.Configurations;

public static class ConfigureWebServices
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.Configure<OpenAISettings>(configuration.GetSection(OpenAISettings.SettingKey))
            .AddSingleton(setting => setting.GetRequiredService<IOptions<OpenAISettings>>().Value);
        services.AddScoped<UserMangerService>();
        return services;
    }
}