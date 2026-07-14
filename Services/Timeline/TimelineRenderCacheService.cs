using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineRenderCacheService
    {
        public List<BrowserCookieEntry> VisibleCookies { get; private set; } = new();

        public List<UserInputEntry> VisibleInputs { get; private set; } = new();

        public List<BrowserExtensionEntry> VisibleExtensions { get; private set; } = new();

        public List<StorageEntry> VisibleStorage { get; private set; } = new();

        public List<DomainEntry> VisibleDomains { get; private set; } = new();

        public List<BrowserHistoryEntry> VisibleHistory { get; private set; } = new();

        public List<AnalysisEntry> VisibleAnomalies { get; private set; } = new();


        public void BuildVisibleCaches(
    List<BrowserHistoryEntry> history,
    List<BrowserCookieEntry> cookies,
    List<UserInputEntry> inputs,
    List<BrowserExtensionEntry> extensions,
    List<StorageEntry> storage,
    List<DomainEntry> domains,
    List<AnalysisEntry> anomalies,
    int viewportWidth)
        {
            VisibleHistory = history
                .Where(h => IsVisible(h.Position, viewportWidth))
                .ToList();

            VisibleCookies = cookies
                .Where(c => IsVisible(c.Position, viewportWidth))
                .ToList();

            VisibleInputs = inputs
                .Where(i => IsVisible(i.Position, viewportWidth))
                .ToList();

            VisibleExtensions = extensions
                .Where(e => IsVisible(e.Position, viewportWidth))
                .ToList();

            VisibleStorage = storage
                .Where(s => IsVisible(s.Position, viewportWidth))
                .ToList();

            var visibleHistorySet = VisibleHistory.ToHashSet();

            VisibleDomains = domains
                .Where(d =>
                    visibleHistorySet.Overlaps(d.SubEntries) ||
                    IsVisible(d.Position, viewportWidth))
                .ToList();

            VisibleAnomalies = anomalies
                .Where(a => IsVisible(a.Position, viewportWidth))
                .ToList();
        }



        public void AddNavigationPath(NavigationPathResult path)
        {
            VisibleHistory = VisibleHistory
                .Union(path.History)
                .Distinct()
                .ToList();

            VisibleDomains = VisibleDomains
                .Union(path.Domains)
                .Distinct()
                .ToList();

            VisibleCookies = VisibleCookies
                .Union(path.Cookies)
                .Distinct()
                .ToList();

            VisibleInputs = VisibleInputs
                .Union(path.Inputs)
                .Distinct()
                .ToList();

            VisibleExtensions = VisibleExtensions
                .Union(path.Extensions)
                .Distinct()
                .ToList();

            VisibleStorage = VisibleStorage
                .Union(path.Storage)
                .Distinct()
                .ToList();

            VisibleAnomalies = VisibleAnomalies
                .Union(path.Analysis)
                .Distinct()
                .ToList();
        }
        public void ApplyNavigationPath(NavigationPathResult path)
        {
            VisibleHistory = VisibleHistory
                .Where(path.History.Contains)
                .ToList();

            VisibleDomains = VisibleDomains
                .Where(path.Domains.Contains)
                .ToList();

            VisibleCookies = VisibleCookies
                .Where(path.Cookies.Contains)
                .ToList();

            VisibleInputs = VisibleInputs
                .Where(path.Inputs.Contains)
                .ToList();

            VisibleExtensions = VisibleExtensions
                .Where(path.Extensions.Contains)
                .ToList();

            VisibleStorage = VisibleStorage
                .Where(path.Storage.Contains)
                .ToList();

            VisibleAnomalies = VisibleAnomalies
                .Where(path.Analysis.Contains)
                .ToList();
        }

        private bool IsVisible(int position, int viewportWidth)
        {
            return position > -50 && position < viewportWidth - 300;
        }
    }
}
