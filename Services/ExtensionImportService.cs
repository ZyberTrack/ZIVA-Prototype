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

                            // Optional Permissions
                            if (root.TryGetProperty("optional_permissions", out var optPerms))
                            {
                                entry.OptionalPermissions = optPerms.EnumerateArray()
                                    .Select(p => p.GetString() ?? "")
                                    .ToList();
                            }

                            // Optional Host Permissions
                            if (root.TryGetProperty("optional_host_permissions", out var optHostPerms))
                            {
                                entry.OptionalHostPermissions = optHostPerms.EnumerateArray()
                                    .Select(p => p.GetString() ?? "")
                                    .ToList();
                            }

                            // Externally Connectable (matches extrahieren)
                            if (root.TryGetProperty("externally_connectable", out var extConn))
                            {
                                if (extConn.TryGetProperty("matches", out var matches))
                                {
                                    entry.ExternallyConnectableMatches = matches.EnumerateArray()
                                        .Select(x => x.GetString() ?? "")
                                        .ToList();
                                }
                            }

                            // Web Accessible Resources (Manifest v3 Struktur!)
                            if (root.TryGetProperty("web_accessible_resources", out var war))
                            {
                                foreach (var item in war.EnumerateArray())
                                {
                                    if (item.TryGetProperty("resources", out var res))
                                    {
                                        entry.WebAccessibleResources.AddRange(
                                            res.EnumerateArray().Select(x => x.GetString() ?? "")
                                        );
                                    }
                                }
                            }

                            // Commands
                            if (root.TryGetProperty("commands", out var commands))
                            {
                                entry.Commands = commands.EnumerateObject()
                                    .Select(c => c.Name)
                                    .ToList();
                            }

                            AnalyzeRisk(entry); // Risk Score Berechnen
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

        private void AnalyzeRisk(BrowserExtensionEntry entry)
        {
            entry.Findings.Clear();

            var perms = entry.AllPermissions;

            void AddFinding(string type, string desc, int weight)
            {
                entry.Findings.Add(new Finding
                {
                    Type = type,
                    Description = desc,
                    Weight = weight
                });
            }

            // 🔥 HIGH RISK PERMISSIONS
            if (perms.Contains("<all_urls>"))
                AddFinding("AllUrls", "Access to all URLs", 40);

            if (perms.Contains("webRequest"))
                AddFinding("WebRequest", "Can intercept web traffic", 50);

            if (perms.Contains("webRequestBlocking"))
                AddFinding("WebRequestBlocking", "Can block/modify requests", 60);

            if (perms.Contains("cookies"))
                AddFinding("Cookies", "Access to cookies", 30);

            if (perms.Contains("history"))
                AddFinding("History", "Access to browsing history", 20);

            if (perms.Contains("tabs"))
                AddFinding("Tabs", "Access to tabs", 15);

            // 🔥 COMBINATIONS
            if (perms.Contains("webRequest") && perms.Contains("<all_urls>"))
                AddFinding("FullInterception", "Full traffic interception capability", 50);

            // 🔥 CONTENT SCRIPT INJECTION
            if (entry.ContentScripts.Any() && perms.Contains("<all_urls>"))
                AddFinding("ScriptInjection", "Injects scripts into all websites", 40);

            // 🔥 BACKGROUND
            if (!string.IsNullOrEmpty(entry.BackgroundScript))
                AddFinding("Background", "Persistent background execution", 10);

            // 🔥 EXTERNAL COMMUNICATION
            if (entry.ExternallyConnectableMatches.Any())
                AddFinding("ExternalConnect", "Externally connectable", 25);

            // 🔥 DATA EXPOSURE
            if (entry.WebAccessibleResources.Any())
                AddFinding("WebResources", "Resources exposed to websites", 20);

            // 🔢 SCORE
            entry.RiskScore = entry.Findings.Sum(f => f.Weight);

            // 🎯 LEVEL (Enum!)
            entry.RiskLevel = entry.RiskScore switch
            {
                >= 120 => RiskLevel.Critical,
                >= 80 => RiskLevel.High,
                >= 40 => RiskLevel.Medium,
                _ => RiskLevel.Low
            };
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
