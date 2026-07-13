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
            bool analysisFilterActive,

            bool artifactFilterActive,
            bool showHistory,
            bool showCookies,
            bool showInputs,
            bool showExtensions,
            bool showStorage,

            int viewportWidth,
            string? activeDomain = null)
        {
            if (!string.IsNullOrWhiteSpace(activeDomain))
            {
                domains = domains
                    .Where(d => d.Domain == activeDomain)
                    .ToList();

                if (domains.Any())
                {
                    history = domains
                        .SelectMany(d => d.SubEntries)
                        .ToList();

                    cookies = cookies
                        .Where(c => c.Relations.Any(r => r.Domain?.Domain == activeDomain))
                        .ToList();

                    inputs = inputs
                        .Where(i => i.Relations.Any(r => r.Domain?.Domain == activeDomain))
                        .ToList();

                    extensions = extensions
                        .Where(e => e.Relations.Any(r => r.Domain?.Domain == activeDomain))
                        .ToList();

                    storage = storage
                        .Where(s => s.Relations.Any(r => r.Domain?.Domain == activeDomain))
                        .ToList();

                    // Für schnelle Contains()-Abfragen
                    var historySet = history.ToHashSet();
                    var cookieSet = cookies.ToHashSet();
                    var inputSet = inputs.ToHashSet();
                    var extensionSet = extensions.ToHashSet();
                    var storageSet = storage.ToHashSet();

                    // Langfristig auf ungefilterte Listen zugreifen, um umgewollte Filterung zu vermeiden
                    anomalies = anomalies
                        .Where(a =>
                            a.LinkedDomain?.Domain == activeDomain ||

                            a.LinkedHistory.Any(h =>
                                domains.Any(d => d.SubEntries.Contains(h))) ||

                            a.LinkedCookies.Any(c =>
                                c.Relations.Any(r => r.Domain?.Domain == activeDomain)) ||

                            a.LinkedInputs.Any(i =>
                                i.Relations.Any(r => r.Domain?.Domain == activeDomain)) ||

                            a.LinkedExtensions.Any(e =>
                                e.Relations.Any(r => r.Domain?.Domain == activeDomain)) ||

                            a.LinkedStorage.Any(s =>
                                s.Relations.Any(r => r.Domain?.Domain == activeDomain))
                        )
                        .ToList();
                }
            }

            // --------------------------------------------------
            // Analysefilter auf Artefakte anwenden
            // --------------------------------------------------

            if (analysisFilterActive)
            {
                var historySet = anomalies
                    .SelectMany(a => a.LinkedHistory)
                    .ToHashSet();

                var cookieSet = anomalies
                    .SelectMany(a => a.LinkedCookies)
                    .ToHashSet();

                var inputSet = anomalies
                    .SelectMany(a => a.LinkedInputs)
                    .ToHashSet();

                var extensionSet = anomalies
                    .SelectMany(a => a.LinkedExtensions)
                    .ToHashSet();

                var storageSet = anomalies
                    .SelectMany(a => a.LinkedStorage)
                    .ToHashSet();

                var domainSet = anomalies
                    .Where(a => a.LinkedDomain != null)
                    .Select(a => a.LinkedDomain!)
                    .ToHashSet();

                history = history
                    .Where(historySet.Contains)
                    .ToList();

                cookies = cookies
                    .Where(cookieSet.Contains)
                    .ToList();

                inputs = inputs
                    .Where(inputSet.Contains)
                    .ToList();

                extensions = extensions
                    .Where(extensionSet.Contains)
                    .ToList();

                storage = storage
                    .Where(storageSet.Contains)
                    .ToList();

                domains = domains
                    .Where(domainSet.Contains)
                    .ToList();
            }

            // --------------------------------------------------
            // Artefaktfilter
            // --------------------------------------------------

            if (artifactFilterActive)
            {
                if (!showHistory)
                    history = new();

                if (!showCookies)
                    cookies = new();

                if (!showInputs)
                    inputs = new();

                if (!showExtensions)
                    extensions = new();

                if (!showStorage)
                    storage = new();

                // Passende Anomalien ebenfalls einschränken
                anomalies = anomalies
                    .Where(a =>
                        (showHistory && a.LinkedHistory.Any()) ||
                        (showCookies && a.LinkedCookies.Any()) ||
                        (showInputs && a.LinkedInputs.Any()) ||
                        (showExtensions && a.LinkedExtensions.Any()) ||
                        (showStorage && a.LinkedStorage.Any()))
                    .ToList();
            }

            //--------------------------------------------------

            VisibleCookies = cookies
                .Where(c => IsVisible(c.Position, viewportWidth))
                .ToList();

            VisibleHistory = history
                .Where(h => IsVisible(h.Position, viewportWidth))
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
