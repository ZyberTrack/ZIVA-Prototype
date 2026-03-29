using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{
    public class ExtensionImportService
    {
        public async Task<List<BrowserExtensionEntry>> LoadExtensionsAsync(string profilePath)
        {
            return await Task.Run(() =>
            {
                var list = new List<BrowserExtensionEntry>();

                var extPath = Path.Combine(profilePath, "Extensions");

                if (!Directory.Exists(extPath))
                    return list;

                foreach (var extDir in Directory.GetDirectories(extPath))
                {
                    var extensionId = Path.GetFileName(extDir);

                    foreach (var versionDir in Directory.GetDirectories(extDir))
                    {
                        var manifestPath = Path.Combine(versionDir, "manifest.json");

                        if (!File.Exists(manifestPath))
                            continue;

                        try
                        {
                            var json = File.ReadAllText(manifestPath);
                            var doc = JsonDocument.Parse(json);

                            var root = doc.RootElement;

                            var rawName = root.GetProperty("name").GetString() ?? "Unknown";

                            var entry = new BrowserExtensionEntry
                            {
                                Id = extensionId,
                                Name = ResolveExtensionName(rawName, versionDir, root),
                                Version = root.GetProperty("version").GetString() ?? "",
                                Description = root.TryGetProperty("description", out var desc)
    ? ResolveExtensionName(desc.GetString() ?? "", versionDir, root)
    : ""
                            };

                            // Permissions
                            if (root.TryGetProperty("permissions", out var perms))
                            {
                                entry.Permissions = perms.EnumerateArray()
                                    .Select(p => p.GetString() ?? "")
                                    .ToList();
                            }

                            // Host Permissions (Manifest v3)
                            if (root.TryGetProperty("host_permissions", out var hostPerms))
                            {
                                entry.HostPermissions = hostPerms.EnumerateArray()
                                    .Select(p => p.GetString() ?? "")
                                    .ToList();
                            }

                            // Content Scripts
                            if (root.TryGetProperty("content_scripts", out var scripts))
                            {
                                foreach (var script in scripts.EnumerateArray())
                                {
                                    if (script.TryGetProperty("js", out var jsFiles))
                                    {
                                        entry.ContentScripts.AddRange(
                                            jsFiles.EnumerateArray().Select(x => x.GetString() ?? "")
                                        );
                                    }
                                }
                            }

                            // Background Script
                            if (root.TryGetProperty("background", out var bg))
                            {
                                if (bg.TryGetProperty("service_worker", out var sw))
                                    entry.BackgroundScript = sw.GetString();
                            }

                            // Installzeit (Folder Creation als Näherung)
                            entry.InstallTime = Directory.GetCreationTime(versionDir);

                            list.Add(entry);
                        }
                        catch
                        {
                            // nicht crashen → wichtig für Forensik
                            continue;
                        }
                    }
                }

                return list;
            });
        }

        private string ResolveExtensionName(string rawName, string versionDir, JsonElement root)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return "Unknown";

            if (!rawName.StartsWith("__MSG_"))
                return rawName;

            var key = rawName.Replace("__MSG_", "").Replace("__", "");

            var localesPath = Path.Combine(versionDir, "_locales");
            if (!Directory.Exists(localesPath))
                return rawName;

            // 🔥 1. preferred locale aus manifest
            string? defaultLocale = null;

            if (root.TryGetProperty("default_locale", out var localeProp))
                defaultLocale = localeProp.GetString();

            string? localeDir = null;

            // 🔥 2. zuerst default_locale versuchen
            if (!string.IsNullOrEmpty(defaultLocale))
            {
                var path = Path.Combine(localesPath, defaultLocale);
                if (Directory.Exists(path))
                    localeDir = path;
            }

            // 🔥 3. fallback: irgendein locale nehmen
            if (localeDir == null)
            {
                localeDir = Directory.GetDirectories(localesPath).FirstOrDefault();
            }

            if (localeDir == null)
                return rawName;

            var messagesFile = Path.Combine(localeDir, "messages.json");

            if (!File.Exists(messagesFile))
                return rawName;

            try
            {
                var json = File.ReadAllText(messagesFile);
                var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty(key, out var entry))
                {
                    if (entry.TryGetProperty("message", out var msg))
                        return msg.GetString() ?? rawName;
                }
            }
            catch { }

            return rawName;
        }
    }
}
