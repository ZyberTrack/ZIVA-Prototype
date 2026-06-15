using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;


namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineSearchService
    {
        public List<SearchResult> Search(
    string query,
    IEnumerable<TimelineArtifact> artifacts)
        {
            query = query.ToLowerInvariant();

            var results = new List<SearchResult>();

            foreach (var artifact in artifacts)
            {
                string searchableText = artifact.Data switch
                {
                    BrowserHistoryEntry h => h.Url,
                    BrowserCookieEntry c => $"{c.Name} {c.Host}",
                    UserInputEntry i => i.Value,
                    BrowserExtensionEntry e => e.Name,
                    StorageEntry s => $"{s.Key} {s.Value}",
                    AnomalyEntry a => a.Description,
                    _ => ""
                };

                if (searchableText.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new SearchResult
                    {
                        Artifact = artifact,
                        DisplayText = searchableText
                    });
                }
            }

            return results;
        }


    }
}
