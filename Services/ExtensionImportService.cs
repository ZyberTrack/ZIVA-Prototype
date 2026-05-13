using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ZIVA_Prototype.Components.Models;
using System.Linq;

namespace ZIVA_Prototype.Services
{
    public class ExtensionImportService
    {
        public async Task<List<BrowserExtensionEntry>> LoadExtensionsAsync(string profilePath)
        {
            return await Task.Run(() =>
            {
                var list = new List<BrowserExtensionEntry>();

                // =====================================================
                // PREFERENCES
                // =====================================================

                JsonElement? extensionSettings = null;

                var preferencesPath =
                    Path.Combine(profilePath, "Preferences");

                if (File.Exists(preferencesPath))
                {
                    try
                    {
                        var prefJson =
                            File.ReadAllText(preferencesPath);

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
                                extensionSettings = settings;
                            }
                        }
                    }
                    catch
                    {
                        // niemals gesamten Import abbrechen
                    }
                }

                // =====================================================
                // EXTENSIONS FOLDER
                // =====================================================

                var extPath =
                    Path.Combine(profilePath, "Extensions");

                if (!Directory.Exists(extPath))
                    return list;

                foreach (var extDir in
                         Directory.GetDirectories(extPath))
                {
                    var extensionId =
                        Path.GetFileName(extDir);

                    foreach (var versionDir in
                             Directory.GetDirectories(extDir))
                    {
                        var manifestPath =
                            Path.Combine(
                                versionDir,
                                "manifest.json");

                        if (!File.Exists(manifestPath))
                            continue;

                        try
                        {
                            var json =
                                File.ReadAllText(manifestPath);

                            var doc =
                                JsonDocument.Parse(json);

                            var root =
                                doc.RootElement;

                            var rawName =
                                root.GetProperty("name")
                                    .GetString() ?? "Unknown";

                            var entry =
                                new BrowserExtensionEntry
                                {
                                    Id = extensionId,

                                    Name =
                                        ResolveExtensionName(
                                            rawName,
                                            versionDir,
                                            root),

                                    Version =
                                        root.GetProperty("version")
                                            .GetString() ?? "",

                                    Description =
                                        root.TryGetProperty(
                                            "description",
                                            out var desc)
                                        ? ResolveExtensionName(
                                            desc.GetString() ?? "",
                                            versionDir,
                                            root)
                                        : ""
                                };

                            // =====================================================
                            // PREFERENCES DATA
                            // =====================================================

                            if (extensionSettings.HasValue)
                            {
                                if (extensionSettings.Value
                                    .TryGetProperty(
                                        extensionId,
                                        out var prefEntry))
                                {
                                    // ENABLED STATE

                                    if (prefEntry.TryGetProperty(
                                        "state",
                                        out var state))
                                    {
                                        entry.IsEnabled =
                                            state.GetInt32() == 1;
                                    }

                                    // INSTALL PATH

                                    if (prefEntry.TryGetProperty(
                                        "path",
                                        out var path))
                                    {
                                        entry.InstallLocation =
                                            path.GetString() ?? "";
                                    }

                                    // UPDATE URL

                                    if (prefEntry.TryGetProperty(
                                        "update_url",
                                        out var updateUrl))
                                    {
                                        entry.UpdateUrl =
                                            updateUrl.GetString() ?? "";
                                    }

                                    // FROM WEBSTORE

                                    if (prefEntry.TryGetProperty(
                                        "from_webstore",
                                        out var webStore))
                                    {
                                        entry.IsFromWebStore =
                                            webStore.GetBoolean();
                                    }

                                    // UNPACKED EXTENSION

                                    entry.IsUnpacked =
                                        !entry.IsFromWebStore;
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
                                        .Select(p =>
                                            p.GetString() ?? "")
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
                                        .Select(p =>
                                            p.GetString() ?? "")
                                        .ToList();
                            }

                            // =====================================================
                            // CONTENT SCRIPTS
                            // =====================================================

                            if (root.TryGetProperty(
                                "content_scripts",
                                out var scripts))
                            {
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
                                if (bg.TryGetProperty(
                                    "service_worker",
                                    out var sw))
                                {
                                    entry.BackgroundScript =
                                        sw.GetString();
                                }
                            }

                            // =====================================================
                            // FORENSIC INSTALL TIME
                            // =====================================================

                            entry.InstallTime =
                                GetBestExtensionTimestamp(
                                    versionDir);

                            // =====================================================
                            // OPTIONAL PERMISSIONS
                            // =====================================================

                            if (root.TryGetProperty(
                                "optional_permissions",
                                out var optPerms))
                            {
                                entry.OptionalPermissions =
                                    optPerms.EnumerateArray()
                                        .Select(p =>
                                            p.GetString() ?? "")
                                        .ToList();
                            }

                            // =====================================================
                            // OPTIONAL HOST PERMISSIONS
                            // =====================================================

                            if (root.TryGetProperty(
                                "optional_host_permissions",
                                out var optHostPerms))
                            {
                                entry.OptionalHostPermissions =
                                    optHostPerms.EnumerateArray()
                                        .Select(p =>
                                            p.GetString() ?? "")
                                        .ToList();
                            }

                            // =====================================================
                            // EXTERNALLY CONNECTABLE
                            // =====================================================

                            if (root.TryGetProperty(
                                "externally_connectable",
                                out var extConn))
                            {
                                if (extConn.TryGetProperty(
                                    "matches",
                                    out var matches))
                                {
                                    entry
                                        .ExternallyConnectableMatches =
                                            matches.EnumerateArray()
                                                .Select(x =>
                                                    x.GetString() ?? "")
                                                .ToList();
                                }
                            }

                            // =====================================================
                            // WEB ACCESSIBLE RESOURCES
                            // =====================================================

                            if (root.TryGetProperty(
                                "web_accessible_resources",
                                out var war))
                            {
                                foreach (var item in
                                         war.EnumerateArray())
                                {
                                    if (item.TryGetProperty(
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
                                entry.Commands =
                                    commands.EnumerateObject()
                                        .Select(c => c.Name)
                                        .ToList();
                            }

                            AnalyzeRisk(entry);

                            list.Add(entry);
                        }
                        catch
                        {
                            // wichtig für Forensik:
                            // niemals gesamten Import abbrechen
                            continue;
                        }
                    }
                }

                return list;
            });
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

        private void AnalyzeRisk(
            BrowserExtensionEntry entry)
        {
            entry.Findings.Clear();

            var perms = entry.AllPermissions;

            void AddFinding(
                string type,
                string desc,
                int weight)
            {
                entry.Findings.Add(new Finding
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

            if (perms.Contains("tabs"))
                AddFinding(
                    "Tabs",
                    "Access to tabs",
                    15);

            if (perms.Contains("webRequest")
                && perms.Contains("<all_urls>"))
            {
                AddFinding(
                    "FullInterception",
                    "Full traffic interception capability",
                    50);
            }

            if (entry.ContentScripts.Any()
                && perms.Contains("<all_urls>"))
            {
                AddFinding(
                    "ScriptInjection",
                    "Injects scripts into all websites",
                    40);
            }

            if (!string.IsNullOrEmpty(
                entry.BackgroundScript))
            {
                AddFinding(
                    "Background",
                    "Persistent background execution",
                    10);
            }

            if (entry.ExternallyConnectableMatches.Any())
            {
                AddFinding(
                    "ExternalConnect",
                    "Externally connectable",
                    25);
            }

            if (entry.WebAccessibleResources.Any())
            {
                AddFinding(
                    "WebResources",
                    "Resources exposed to websites",
                    20);
            }

            if (entry.IsUnpacked)
            {
                AddFinding(
                    "Unpacked",
                    "Unpacked developer extension",
                    35);
            }

            if (!entry.IsEnabled)
            {
                AddFinding(
                    "Disabled",
                    "Extension currently disabled",
                    5);
            }

            entry.RiskScore =
                entry.Findings.Sum(f => f.Weight);

            entry.RiskLevel =
                entry.RiskScore switch
                {
                    >= 120 => RiskLevel.Critical,
                    >= 80 => RiskLevel.High,
                    >= 40 => RiskLevel.Medium,
                    _ => RiskLevel.Low
                };
        }

        private string ResolveExtensionName(
            string rawName,
            string versionDir,
            JsonElement root)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return "Unknown";

            if (!rawName.StartsWith("__MSG_"))
                return rawName;

            var key =
                rawName.Replace("__MSG_", "")
                       .Replace("__", "");

            var localesPath =
                Path.Combine(versionDir, "_locales");

            if (!Directory.Exists(localesPath))
                return rawName;

            string? defaultLocale = null;

            if (root.TryGetProperty(
                "default_locale",
                out var localeProp))
            {
                defaultLocale =
                    localeProp.GetString();
            }

            string? localeDir = null;

            if (!string.IsNullOrEmpty(defaultLocale))
            {
                var path =
                    Path.Combine(
                        localesPath,
                        defaultLocale);

                if (Directory.Exists(path))
                    localeDir = path;
            }

            if (localeDir == null)
            {
                localeDir =
                    Directory.GetDirectories(
                        localesPath)
                    .FirstOrDefault();
            }

            if (localeDir == null)
                return rawName;

            var messagesFile =
                Path.Combine(
                    localeDir,
                    "messages.json");

            if (!File.Exists(messagesFile))
                return rawName;

            try
            {
                var json =
                    File.ReadAllText(messagesFile);

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