// INFOS FÜR BACHELORARBEIT:
// -> Extentions folder und Preferences werden geparsed.
// und in Browser History, File System Timeline, Chromium Extension Runtime Artefakte
// SERVICES: ExtensionPreferenceScanner, ExtensionFolderScanner, ExtensionRuntimeScanner, ExtensionFilesystemScanner, ExtensionHistoryAnalyzer

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import
{
    public class ExtensionImportService
    {
        public async Task<List<BrowserExtensionEntry>>
            LoadExtensionsAsync(
                string profilePath,
                List<BrowserHistoryEntry>? historyEntries = null)
        {
            return await Task.Run(() =>
            {
                var extensions =
                    new Dictionary<string,
                    BrowserExtensionEntry>();

                // =====================================================
                // PREFERENCES
                // =====================================================

                JsonElement? extensionSettings = null;

                LoadPreferences(
                    profilePath,
                    ref extensionSettings);

                // =====================================================
                // EXTENSIONS FOLDER
                // =====================================================

                ScanExtensionsFolder(
                    profilePath,
                    extensionSettings,
                    extensions);

                // =====================================================
                // UNPACKED / DEV EXTENSIONS FROM PREFERENCES
                // =====================================================

                ScanPreferenceOnlyExtensions(
                    extensionSettings,
                    extensions);

                // =====================================================
                // CHROMIUM RUNTIME ARTEFACTS
                // =====================================================

                ScanRuntimeArtifacts(
                    profilePath,
                    extensions);

                // =====================================================
                // FILE SYSTEM TIMELINE
                // =====================================================

                ScanFilesystemArtifacts(
                    extensions);

                // =====================================================
                // BROWSER HISTORY
                // =====================================================

                if (historyEntries != null)
                {
                    AnalyzeHistory(
                        historyEntries,
                        extensions);
                }

                // =====================================================
                // FINAL RISK ANALYSIS
                // =====================================================

                foreach (var ext in extensions.Values)
                {
                    AnalyzeRisk(ext);
                }

                return extensions.Values.ToList();
            });
        }

        // =========================================================
        // PREFERENCES
        // =========================================================

        private void LoadPreferences(
            string profilePath,
            ref JsonElement? extensionSettings)
        {
            try
            {
                var preferencesPath =
                    Path.Combine(
                        profilePath,
                        "Preferences");

                if (!File.Exists(preferencesPath))
                    return;

                var prefJson =
                    File.ReadAllText(
                        preferencesPath);

                var prefDoc =
                    JsonDocument.Parse(prefJson);

                if (prefDoc.RootElement.TryGetProperty(
                    "extensions",
                    out var extensionsRoot))
                {
                    if (extensionsRoot.TryGetProperty(
                        "settings",
                        out var settings))
                    {
                        extensionSettings =
                            settings;
                    }
                }
            }
            catch
            {
            }
        }

        // =========================================================
        // EXTENSIONS FOLDER
        // =========================================================

        private void ScanExtensionsFolder(
            string profilePath,
            JsonElement? extensionSettings,
            Dictionary<string,
            BrowserExtensionEntry> extensions)
        {
            var extPath =
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

                        entry.SourceTypes
                            .Add("ExtensionsFolder");

                        entry.InstallLocation =
                            versionDir;

                        string manifestPath =
                            Path.Combine(
                                versionDir,
                                "manifest.json");

                        bool manifestExists =
                            File.Exists(
                                manifestPath);

                        if (!manifestExists)
                        {
                            entry.ManifestMissing =
                                true;

                            entry.Name =
                                "[Missing Manifest]";

                            entry.Description =
                                "Manifest missing";

                            continue;
                        }

                        ParseManifest(
                            manifestPath,
                            versionDir,
                            entry);

                        entry.InstallTime =
                            GetBestExtensionTimestamp(
                                versionDir);

                        ApplyPreferenceData(
                            extensionSettings,
                            extensionId,
                            entry);

                        entry.ConfidenceScore += 50;
                    }
                    catch
                    {
                    }
                }
            }
        }

        // =========================================================
        // PREFERENCE ONLY EXTENSIONS
        // =========================================================

        private void ScanPreferenceOnlyExtensions(
            JsonElement? extensionSettings,
            Dictionary<string,
            BrowserExtensionEntry> extensions)
        {
            if (!extensionSettings.HasValue)
                return;

            foreach (var ext in
                     extensionSettings.Value
                     .EnumerateObject())
            {
                try
                {
                    string extensionId =
                        ext.Name;

                    var prefEntry =
                        ext.Value;

                    var entry =
                        GetOrCreate(
                            extensions,
                            extensionId);

                    entry.FoundInPreferences =
                        true;

                    entry.SourceTypes
                        .Add("Preferences");

                    if (prefEntry.TryGetProperty(
                        "path",
                        out var pathProp))
                    {
                        entry.InstallLocation =
                            pathProp.GetString()
                            ?? "";

                        entry.IsUnpacked = true;
                    }

                    if (prefEntry.TryGetProperty(
                        "state",
                        out var state))
                    {
                        entry.IsEnabled =
                            state.GetInt32() == 1;
                    }

                    if (prefEntry.TryGetProperty(
                        "update_url",
                        out var updateUrl))
                    {
                        entry.UpdateUrl =
                            updateUrl.GetString()
                            ?? "";
                    }

                    entry.ConfidenceScore += 25;
                }
                catch
                {
                }
            }
        }

        // =========================================================
        // CHROMIUM RUNTIME ARTEFACTS
        // =========================================================

        private void ScanRuntimeArtifacts(
            string profilePath,
            Dictionary<string,
            BrowserExtensionEntry> extensions)
        {
            string[] runtimeDirs =
            {
                "Local Extension Settings",
                "Sync Extension Settings",
                "Extension Rules",
                "Extension Scripts",
                "IndexedDB",
                "Local Storage"
            };

            foreach (var runtimeDir in
                     runtimeDirs)
            {
                try
                {
                    string fullPath =
                        Path.Combine(
                            profilePath,
                            runtimeDir);

                    if (!Directory.Exists(fullPath))
                        continue;

                    foreach (var dir in
                             Directory.GetDirectories(
                                 fullPath,
                                 "*",
                                 SearchOption.AllDirectories))
                    {
                        string name =
                            Path.GetFileName(dir);

                        if (name.Length < 20)
                            continue;

                        var entry =
                            GetOrCreate(
                                extensions,
                                name);

                        entry.FoundInRuntimeArtifacts =
                            true;

                        entry.RuntimeArtifacts
                            .Add(dir);

                        entry.SourceTypes
                            .Add(runtimeDir);

                        entry.ConfidenceScore += 20;
                    }
                }
                catch
                {
                }
            }
        }

        // =========================================================
        // FILE SYSTEM TIMELINE
        // =========================================================

        private void ScanFilesystemArtifacts(
            Dictionary<string,
            BrowserExtensionEntry> extensions)
        {
            string[] suspiciousRoots =
            {
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop),

                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments),

                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),

                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.UserProfile),
                    "Downloads")
            };

            foreach (var root in suspiciousRoots)
            {
                try
                {
                    if (!Directory.Exists(root))
                        continue;

                    foreach (var dir in
                             Directory.GetDirectories(
                                 root,
                                 "*",
                                 SearchOption.AllDirectories))
                    {
                        bool hasManifest =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "manifest.json"));

                        bool hasExtensionFiles =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "background.js"))
                            ||
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "content.js"))
                            ||
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "service_worker.js"));

                        if (!hasManifest ||
                            !hasExtensionFiles)
                        {
                            continue;
                        }

                        string fakeId =
                            "FS-" +
                            Math.Abs(
                                dir.GetHashCode());

                        var entry =
                            GetOrCreate(
                                extensions,
                                fakeId);

                        entry.Name =
                            Path.GetFileName(dir);

                        entry.InstallLocation =
                            dir;

                        entry.FoundInFilesystem =
                            true;

                        entry.IsUnpacked = true;

                        entry.SourceTypes
                            .Add("Filesystem");

                        entry.DetectedFiles
                            .AddRange(
                                Directory.GetFiles(
                                    dir,
                                    "*.js",
                                    SearchOption.TopDirectoryOnly));

                        entry.ConfidenceScore += 40;
                    }
                }
                catch
                {
                }
            }
        }

        // =========================================================
        // BROWSER HISTORY
        // =========================================================

        private void AnalyzeHistory(
            List<BrowserHistoryEntry> historyEntries,
            Dictionary<string,
            BrowserExtensionEntry> extensions)
        {
            Regex extensionRegex =
                new Regex(
                    @"chrome-extension:\/\/([a-z]{32})",
                    RegexOptions.IgnoreCase);

            foreach (var history in historyEntries)
            {
                try
                {
                    if (history.Url.Contains(
                        "chrome://extensions"))
                    {
                    }

                    var match =
                        extensionRegex.Match(
                            history.Url);

                    if (!match.Success)
                        continue;

                    string extensionId =
                        match.Groups[1].Value;

                    var entry =
                        GetOrCreate(
                            extensions,
                            extensionId);

                    entry.FoundInHistory =
                        true;

                    entry.HistoryIndicators
                        .Add(history.Url);

                    entry.SourceTypes
                        .Add("History");

                    entry.ConfidenceScore += 30;
                }
                catch
                {
                }
            }
        }

        // =========================================================
        // MANIFEST PARSER
        // =========================================================

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

            var rawName =
                root.GetProperty("name")
                    .GetString()
                ?? "Unknown";

            entry.Name =
                ResolveExtensionName(
                    rawName,
                    versionDir,
                    root);

            entry.Version =
                root.GetProperty("version")
                    .GetString()
                ?? "";

            entry.Description =
                root.TryGetProperty(
                    "description",
                    out var desc)
                ? ResolveExtensionName(
                    desc.GetString() ?? "",
                    versionDir,
                    root)
                : "";

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
                                    x.GetString()
                                    ?? ""));
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

        // =========================================================
        // APPLY PREFERENCE DATA
        // =========================================================

        private void ApplyPreferenceData(
            JsonElement? extensionSettings,
            string extensionId,
            BrowserExtensionEntry entry)
        {
            if (!extensionSettings.HasValue)
                return;

            if (!extensionSettings.Value
                .TryGetProperty(
                    extensionId,
                    out var prefEntry))
            {
                return;
            }

            entry.FoundInPreferences =
                true;

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
                    path.GetString()
                    ?? "";
            }

            if (prefEntry.TryGetProperty(
                "update_url",
                out var updateUrl))
            {
                entry.UpdateUrl =
                    updateUrl.GetString()
                    ?? "";
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
        }

        // =========================================================
        // GET OR CREATE
        // =========================================================

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

        // =========================================================
        // FORENSIC EXTENSION TIMESTAMP
        // =========================================================

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
                        File.GetLastWriteTimeUtc(
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
                    return dir.LastWriteTimeUtc;
                }

                DateTime latestWrite =
                    files.Max(f =>
                        f.LastWriteTimeUtc);

                if (latestWrite.Year >= 2015)
                {
                    return latestWrite;
                }

                return dir.LastWriteTimeUtc;
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        // =========================================================
        // RISK ANALYSIS
        // =========================================================

        private void AnalyzeRisk(
            BrowserExtensionEntry entry)
        {
            entry.Findings.Clear();

            var perms =
                entry.AllPermissions;

            void AddFinding(
                string type,
                string desc,
                int weight)
            {
                entry.Findings.Add(
                    new Finding
                    {
                        Type = type,
                        Description = desc,
                        Weight = weight
                    });
            }

            if (perms.Contains("<all_urls>"))
                AddFinding(
                    "AllUrls",
                    "Access to all URLs",
                    40);

            if (perms.Contains("webRequest"))
                AddFinding(
                    "WebRequest",
                    "Can intercept web traffic",
                    50);

            if (perms.Contains("webRequestBlocking"))
                AddFinding(
                    "WebRequestBlocking",
                    "Can block/modify requests",
                    60);

            if (perms.Contains("cookies"))
                AddFinding(
                    "Cookies",
                    "Access to cookies",
                    30);

            if (perms.Contains("history"))
                AddFinding(
                    "History",
                    "Access to browsing history",
                    20);

            if (entry.IsUnpacked)
                AddFinding(
                    "Unpacked",
                    "Unpacked developer extension",
                    35);

            if (entry.FoundInRuntimeArtifacts)
                AddFinding(
                    "RuntimeArtifacts",
                    "Residual runtime artifacts detected",
                    25);

            if (entry.FoundInFilesystem)
                AddFinding(
                    "FilesystemArtifacts",
                    "Extension source files detected",
                    30);

            entry.RiskScore =
                entry.Findings.Sum(
                    f => f.Weight);

            entry.RiskLevel =
                entry.RiskScore switch
                {
                    >= 120 => RiskLevel.Critical,
                    >= 80 => RiskLevel.High,
                    >= 40 => RiskLevel.Medium,
                    _ => RiskLevel.Low
                };
        }

        // =========================================================
        // LOCALIZATION
        // =========================================================

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
    }
}
