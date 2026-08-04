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
            LoadExtensionsAsync(string profilePath,List<BrowserHistoryEntry>? historyEntries = null, ImportRange? range = null)
        {

            var preferenceScanner = new ExtensionPreferenceScanner();
            var folderScanner = new ExtensionFolderScanner();
            var runtimeScanner = new ExtensionRuntimeScanner();
            var filesystemScanner = new ExtensionFilesystemScanner();
            var historyAnalyzer = new ExtensionHistoryAnalyzer();

            return await Task.Run(() =>
            {
                var extensions =
                    new Dictionary<string,
                    BrowserExtensionEntry>();

                // =====================================================
                // PREFERENCES
                // =====================================================

                JsonElement? extensionSettings = preferenceScanner.LoadExtensionSettings(profilePath);

                // =====================================================
                // EXTENSIONS FOLDER
                // =====================================================

                folderScanner.Scan(profilePath,extensions);

                // =====================================================
                // UNPACKED / DEV EXTENSIONS FROM PREFERENCES
                // =====================================================

                preferenceScanner.ScanPreferenceOnlyExtensions(extensionSettings,extensions);

                // =====================================================
                // CHROMIUM RUNTIME ARTEFACTS
                // =====================================================

                runtimeScanner.Scan(profilePath,extensions);

                // =====================================================
                // FILE SYSTEM TIMELINE
                // =====================================================

                filesystemScanner.Scan(extensions);

                // =====================================================
                // BROWSER HISTORY
                // =====================================================

                if (historyEntries != null)
                {
                    historyAnalyzer.Analyze(historyEntries,extensions);
                }

                // =====================================================
                // FINAL RISK ANALYSIS
                // =====================================================

                foreach (var ext in extensions.Values)
                {
                    NormalizeExtension(ext);
                    AnalyzeRisk(ext);
                }

                var result = extensions.Values.ToList();

                if (range != null)
                {
                    result = result
                        .Where(e =>
                            e.InstallTime >= range.From &&
                            e.InstallTime <= range.To)
                        .ToList();
                }

                return result;
            });
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


        private void NormalizeExtension(
    BrowserExtensionEntry ext)
        {
            // -------------------------------------------------
            // INVALID TIMESTAMP
            // -------------------------------------------------

            if (ext.InstallTime == default ||
                ext.InstallTime.Year < 2015)
            {
                ext.InstallTime = DateTime.UtcNow;

                ext.IsResidualArtifact = true;
            }

            // -------------------------------------------------
            // SOURCE TYPES DEDUP
            // -------------------------------------------------

            ext.SourceTypes =
                ext.SourceTypes
                    .Distinct()
                    .ToList();

            ext.RuntimeArtifacts =
                ext.RuntimeArtifacts
                    .Distinct()
                    .ToList();

            ext.DetectedFiles =
                ext.DetectedFiles
                    .Distinct()
                    .ToList();

            ext.HistoryIndicators =
                ext.HistoryIndicators
                    .Distinct()
                    .ToList();

            // -------------------------------------------------
            // CONTENT FLAGS
            // -------------------------------------------------

            ext.HasBackgroundScript =
                !string.IsNullOrWhiteSpace(
                    ext.BackgroundScript);

            ext.HasContentScripts =
                ext.ContentScripts.Any();

            // -------------------------------------------------
            // FALLBACK NAME
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(ext.Name))
            {
                ext.Name =
                    "[Unknown Extension]";
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

       
    }
}
