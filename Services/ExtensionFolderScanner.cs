using System.Text.Json;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services;

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

                    entry.ConfidenceScore += 50;
                }
                catch
                {
                }
            }
        }
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

        entry.Name =
            root.GetProperty("name")
                .GetString() ?? "Unknown";

        entry.Version =
            root.GetProperty("version")
                .GetString() ?? "";

        if (root.TryGetProperty(
            "description",
            out var desc))
        {
            entry.Description =
                desc.GetString() ?? "";
        }

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

        if (root.TryGetProperty(
            "background",
            out var bg))
        {
            if (bg.TryGetProperty(
                "service_worker",
                out var sw))
            {
                entry.HasServiceWorker =
                    true;

                entry.BackgroundScript =
                    sw.GetString();
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

            if (File.Exists(manifestPath))
            {
                DateTime manifestTime =
                    File.GetCreationTimeUtc(
                        manifestPath);

                if (manifestTime.Year >= 2015 &&
                    manifestTime <= DateTime.UtcNow)
                {
                    return manifestTime;
                }
            }

            var dir =
                new DirectoryInfo(versionDir);

            var files =
                dir.GetFiles(
                    "*",
                    SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                return dir.CreationTimeUtc;
            }

            DateTime earliest =
                files.Min(f =>
                    f.CreationTimeUtc);

            if (earliest.Year >= 2015)
            {
                return earliest;
            }

            return dir.CreationTimeUtc;
        }
        catch
        {
            return DateTime.UtcNow;
        }
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