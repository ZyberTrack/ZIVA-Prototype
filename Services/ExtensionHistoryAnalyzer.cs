using System.Text.RegularExpressions;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services;

public class ExtensionHistoryAnalyzer
{
    public void Analyze(
        List<BrowserHistoryEntry> historyEntries,
        Dictionary<string,
        BrowserExtensionEntry> extensions)
    {
        Regex regex =
            new Regex(
                @"chrome-extension:\/\/([a-z]{32})",
                RegexOptions.IgnoreCase);

        foreach (var history in historyEntries)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    history.Url))
                {
                    continue;
                }

                var match =
                    regex.Match(history.Url);

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

                // =====================================================
                // TIMESTAMP
                // =====================================================

                try
                {
                    if (history.VisitTime.Year >= 2015)
                    {
                        if (entry.InstallTime == default ||
                            history.VisitTime <
                            entry.InstallTime)
                        {
                            ExtensionTimestampHelper
                                .UpdateInstallTime(
                                    entry,
                                    history.VisitTime);
                        }
                    }
                }
                catch
                {
                }

                // =====================================================
                // CONFIDENCE
                // =====================================================

                entry.ConfidenceScore += 30;
            }
            catch
            {
            }
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