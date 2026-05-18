using System.Text.Json;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import;

public class ExtensionFolderScanner
{
    public void Scan(
        string profilePath,
        Dictionary<string,
        BrowserExtensionEntry> extensions)
    {
        string extPath =
            Path.Combine(
                profilePath,
                "Extensions");

        if (!Directory.Exists(extPath))
            return;

        foreach (var extDir in
                 Directory.GetDirectories(extPath))
        {
            string extensionId =
                Path.GetFileName(extDir);

            foreach (var versionDir in
                     Directory.GetDirectories(extDir))
            {
                try
                {
                    var entry =
                        GetOrCreate(
                            extensions,
                            extensionId);

                    entry.FoundInExtensionsFolder =
                        true;

                    entry.SourceTypes.Add(
                        "ExtensionsFolder");

                    entry.InstallLocation =
                        versionDir;

                    // =====================================================
                    // TIMESTAMP MERGE
                    // =====================================================

                    DateTime fsTime =
                        GetBestExtensionTimestamp(
                            versionDir);

                    ExtensionTimestampHelper
                        .UpdateInstallTime(
                            entry,
                            fsTime);

                    try
                    {
                        var latestWrite =
                            Directory.GetFiles(
                                versionDir,
                                "*",
                                SearchOption.AllDirectories)
                            .Select(File.GetLastWriteTimeUtc)
                            .DefaultIfEmpty(DateTime.MinValue)
                            .Max();

                        if (latestWrite >
                            entry.LastFilesystemActivity)
                        {
                            entry.LastFilesystemActivity =
                                latestWrite;
                        }
                    }
                    catch
                    {
                    }

                    string manifestPath =
                        Path.Combine(
                            versionDir,
                            "manifest.json");

                    if (!File.Exists(manifestPath))
                    {
                        entry.ManifestMissing =
                            true;

                        continue;
                    }

                    ParseManifest(
                        manifestPath,
                        versionDir,
                        entry);

                    // =====================================================
                    // FALLBACK WEBSTORE DETECTION
                    // =====================================================

                    if (!entry.IsUnpacked &&
                        entry.FoundInExtensionsFolder &&
                        IsValidExtensionId(entry.Id))
                    {
                        entry.IsFromWebStore = true;
                    }

                    entry.ConfidenceScore += 50;
                }
                catch
                {
                }
            }
        }
    }

    private bool IsValidExtensionId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (id.Length != 32)
            return false;

        return id.All(c =>
            c >= 'a' &&
            c <= 'p');
    }

    private void ParseManifest(
    string manifestPath,
    string versionDir,
    BrowserExtensionEntry entry)
    {
        var json =
            File.ReadAllText(
                manifestPath);

        var doc =
            JsonDocument.Parse(json);

        var root =
            doc.RootElement;

        // =====================================================
        // NAME
        // =====================================================

        var rawName =
            root.GetProperty("name")
                .GetString()
            ?? "Unknown";

        entry.Name =
            ResolveExtensionName(
                rawName,
                versionDir,
                root);

        // =====================================================
        // VERSION
        // =====================================================

        if (root.TryGetProperty(
            "version",
            out var version))
        {
            entry.Version =
                version.GetString()
                ?? "";
        }

        // =====================================================
        // DESCRIPTION
        // =====================================================

        if (root.TryGetProperty(
            "description",
            out var desc))
        {
            entry.Description =
                ResolveExtensionName(
                    desc.GetString() ?? "",
                    versionDir,
                    root);
        }

        // =====================================================
        // UPDATE URL
        // =====================================================

        if (root.TryGetProperty(
            "update_url",
            out var updateUrl))
        {
            entry.UpdateUrl =
                updateUrl.GetString() ?? "";

            if (!string.IsNullOrWhiteSpace(
                    entry.UpdateUrl) &&

                entry.UpdateUrl.Contains(
                    "google.com/service/update2/crx"))
            {
                entry.IsFromWebStore = true;
            }
        }

        // =====================================================
        // PERMISSIONS
        // =====================================================

        if (root.TryGetProperty(
            "permissions",
            out var perms))
        {
            entry.Permissions =
                perms.EnumerateArray()
                    .Select(x =>
                        x.GetString() ?? "")
                    .ToList();
        }

        // =====================================================
        // HOST PERMISSIONS
        // =====================================================

        if (root.TryGetProperty(
            "host_permissions",
            out var hostPerms))
        {
            entry.HostPermissions =
                hostPerms.EnumerateArray()
                    .Select(x =>
                        x.GetString() ?? "")
                    .ToList();
        }

        // =====================================================
        // CONTENT SCRIPTS
        // =====================================================

        if (root.TryGetProperty(
            "content_scripts",
            out var scripts))
        {
            entry.HasContentScripts =
                true;

            foreach (var script in
                     scripts.EnumerateArray())
            {
                if (script.TryGetProperty(
                    "js",
                    out var jsFiles))
                {
                    entry.ContentScripts
                        .AddRange(
                            jsFiles
                            .EnumerateArray()
                            .Select(x =>
                                x.GetString() ?? ""));
                }
            }
        }

        // =====================================================
        // BACKGROUND
        // =====================================================

        if (root.TryGetProperty(
            "background",
            out var bg))
        {
            entry.HasBackgroundScript =
                true;

            // MV3
            if (bg.TryGetProperty(
                "service_worker",
                out var sw))
            {
                entry.HasServiceWorker =
                    true;

                entry.BackgroundScript =
                    sw.GetString();
            }

            // MV2
            if (bg.TryGetProperty(
                "scripts",
                out var scriptsProp))
            {
                entry.BackgroundScript =
                    string.Join(
                        ", ",
                        scriptsProp
                        .EnumerateArray()
                        .Select(x =>
                            x.GetString()));
            }
        }

        // =====================================================
        // WEB ACCESSIBLE RESOURCES
        // =====================================================

        if (root.TryGetProperty(
            "web_accessible_resources",
            out var resources))
        {
            foreach (var r in
                     resources.EnumerateArray())
            {
                if (r.TryGetProperty(
                    "resources",
                    out var res))
                {
                    entry.WebAccessibleResources
                        .AddRange(
                            res.EnumerateArray()
                            .Select(x =>
                                x.GetString() ?? ""));
                }
            }
        }

        // =====================================================
        // COMMANDS
        // =====================================================

        if (root.TryGetProperty(
            "commands",
            out var commands))
        {
            foreach (var cmd in
                     commands.EnumerateObject())
            {
                entry.Commands.Add(
                    cmd.Name);
            }
        }
    }


    private DateTime GetBestExtensionTimestamp(
    string versionDir)
    {
        try
        {
            string manifestPath =
                Path.Combine(
                    versionDir,
                    "manifest.json");

            if (!File.Exists(manifestPath))
            {
                return DateTime.UtcNow;
            }

            // =====================================================
            // MANIFEST WRITE TIME
            // BEST INSTALL / UPDATE INDICATOR
            // =====================================================

            DateTime manifestTime =
                File.GetLastWriteTimeUtc(
                    manifestPath);

            if (manifestTime.Year >= 2015 &&
                manifestTime <= DateTime.UtcNow)
            {
                return manifestTime;
            }
        }
        catch
        {
        }

        return DateTime.UtcNow;
    }

    private string ResolveExtensionName(
    string rawName,
    string versionDir,
    JsonElement root)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(
                rawName))
            {
                return "Unknown";
            }

            if (!rawName.StartsWith(
                "__MSG_"))
            {
                return rawName;
            }

            var key =
                rawName.Replace(
                    "__MSG_",
                    "")
                .Replace(
                    "__",
                    "");

            var localesPath =
                Path.Combine(
                    versionDir,
                    "_locales");

            if (!Directory.Exists(
                localesPath))
            {
                return rawName;
            }

            string? defaultLocale =
                null;

            if (root.TryGetProperty(
                "default_locale",
                out var localeProp))
            {
                defaultLocale =
                    localeProp.GetString();
            }

            string? localeDir =
                null;

            if (!string.IsNullOrEmpty(
                defaultLocale))
            {
                var path =
                    Path.Combine(
                        localesPath,
                        defaultLocale);

                if (Directory.Exists(path))
                {
                    localeDir = path;
                }
            }

            if (localeDir == null)
            {
                localeDir =
                    Directory.GetDirectories(
                        localesPath)
                    .FirstOrDefault();
            }

            if (localeDir == null)
            {
                return rawName;
            }

            var messagesFile =
                Path.Combine(
                    localeDir,
                    "messages.json");

            if (!File.Exists(
                messagesFile))
            {
                return rawName;
            }

            var json =
                File.ReadAllText(
                    messagesFile);

            var doc =
                JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(
                key,
                out var entry))
            {
                if (entry.TryGetProperty(
                    "message",
                    out var msg))
                {
                    return msg.GetString()
                        ?? rawName;
                }
            }
        }
        catch
        {
        }

        return rawName;
    }


    private BrowserExtensionEntry GetOrCreate(
        Dictionary<string,
        BrowserExtensionEntry> extensions,
        string id)
    {
        if (!extensions.ContainsKey(id))
        {
            extensions[id] =
                new BrowserExtensionEntry
                {
                    Id = id
                };
        }

        return extensions[id];
    }
}