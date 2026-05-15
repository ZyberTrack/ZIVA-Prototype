using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ZIVA_Prototype.Services.Import;
using ZIVA_Prototype.Services.Timeline;

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

            // ⬇ History / User Input
            builder.Services.AddScoped<HistoryImportService>();
            builder.Services.AddScoped<WebDataAutofillImportService>();
            builder.Services.AddScoped<FaviconsImportService>();
            builder.Services.AddScoped<UserInputAggregatorService>();
            //builder.Services.AddScoped<UserInputTimelineBuilder>();

            // ⬇ Cookies / Storage
            builder.Services.AddScoped<CookieImportService>();
            builder.Services.AddScoped<StorageImportService>();

            builder.Services.AddSingleton<StorageArtifactScanner>();

            // ⬇ Chromium Profiles
            builder.Services.AddScoped<ChromeProfileLoader>();

            // ⬇ Extension DFIR Pipeline
            builder.Services.AddScoped<ExtensionImportService>();

            builder.Services.AddScoped<ExtensionPreferenceScanner>();
            builder.Services.AddScoped<ExtensionFolderScanner>();
            builder.Services.AddScoped<ExtensionRuntimeScanner>();
            builder.Services.AddScoped<ExtensionFilesystemScanner>();
            builder.Services.AddScoped<ExtensionHistoryAnalyzer>();

            // ⬇ Timeline / State
            builder.Services.AddSingleton<TimelineStateService>();
            builder.Services.AddSingleton<PersistStateService>();
            builder.Services.AddSingleton<TimelineAnomalyService>();
            builder.Services.AddSingleton<TimelineColorService>();


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
