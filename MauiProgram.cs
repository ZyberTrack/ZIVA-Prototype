using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ZIVA_Prototype.Services;

namespace ZIVA_Prototype
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            // ⬇ Hier deine Services registrieren:
            builder.Services.AddScoped<HistoryImportService>();
            builder.Services.AddScoped<WebDataAutofillImportService>();
            builder.Services.AddScoped<CookieImportService>();
            builder.Services.AddScoped<ChromeProfileLoader>();
            builder.Services.AddScoped<UserInputAggregatorService>();

            builder.Services.AddSingleton<TimelineStateService>();
            builder.Services.AddSingleton<PersistStateService>();
            

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
