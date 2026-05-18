using System.Text.Json;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import;

public class ExtensionPreferenceScanner
{
    public JsonElement? LoadExtensionSettings(
        string profilePath)
    {
        try
        {
            string preferencesPath =
                Path.Combine(
                    profilePath,
                    "Preferences");

            if (!File.Exists(preferencesPath))
                return null;

            var json =
                File.ReadAllText(
                    preferencesPath);

            var doc =
                JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(
                "extensions",
                out var extensions))
            {
                if (extensions.TryGetProperty(
                    "settings",
                    out var settings))
                {
                    return settings;
                }
            }
        }
        catch
        {
        }

        return null;
    }

    public void ScanPreferenceOnlyExtensions(
    JsonElement? extensionSettings,
    Dictionary<string,
    BrowserExtensionEntry> extensions)
    {
        try
        {
            if (!extensionSettings.HasValue)
                return;

            foreach (var ext in
                     extensionSettings.Value
                     .EnumerateObject())
            {
                string extensionId =
                    ext.Name;

                if (!extensions.ContainsKey(
                        extensionId))
                {
                    extensions[extensionId] =
                        new BrowserExtensionEntry
                        {
                            Id = extensionId
                        };
                }

                var entry =
                    extensions[extensionId];

                ApplyPreferenceData(
                    extensionSettings,
                    entry);
            }
        }
        catch
        {
        }
    }

    public void ApplyPreferenceData(
        JsonElement? extensionSettings,
        BrowserExtensionEntry entry)
    {
        try
        {
            if (!extensionSettings.HasValue)
                return;

            if (!extensionSettings.Value.TryGetProperty(
                entry.Id,
                out var prefEntry))
            {
                return;
            }

            entry.FoundInPreferences = true;

            entry.SourceTypes.Add(
                "Preferences");

            if (prefEntry.TryGetProperty(
                "state",
                out var state))
            {
                entry.IsEnabled =
                    state.GetInt32() == 1;
            }

            if (prefEntry.TryGetProperty(
                "path",
                out var path))
            {
                entry.InstallLocation =
                    path.GetString() ?? "";
            }

            if (prefEntry.TryGetProperty(
                "update_url",
                out var updateUrl))
            {
                entry.UpdateUrl =
                    updateUrl.GetString() ?? "";
            }

            if (prefEntry.TryGetProperty(
                "from_webstore",
                out var webStore))
            {
                entry.IsFromWebStore =
                    webStore.GetBoolean();
            }

            entry.IsUnpacked =
                !entry.IsFromWebStore;

            entry.ConfidenceScore += 20;
        }
        catch
        {
        }
    }
}