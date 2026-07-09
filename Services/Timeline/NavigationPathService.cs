using System;
using System.Collections.Generic;
using System.Linq;
using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    internal class NavigationPathService
    {
        public NavigationPathResult BuildNavigationPath(
            object artifact,
            List<DomainEntry> domains,
            List<BrowserCookieEntry> cookies,
            List<UserInputEntry> inputs,
            List<BrowserExtensionEntry> extensions,
            List<StorageEntry> storage,
            List<AnalysisEntry> analysis)
        {
            var result = new NavigationPathResult();

            var startHistory = ResolveHistory(artifact, domains);

            if (startHistory == null)
                return result;

            var allHistory = domains
                .SelectMany(d => d.SubEntries)
                .Distinct()
                .ToList();

            TraverseBackward(
                startHistory,
                result,
                new HashSet<BrowserHistoryEntry>());

            TraverseForward(
                startHistory,
                allHistory,
                result,
                new HashSet<BrowserHistoryEntry>());

            CollectDomains(
                domains,
                result);

            CollectArtifacts(
                cookies,
                inputs,
                extensions,
                storage,
                analysis,
                result);

            return result;
        }

        // =====================================================
        // HISTORY RESOLUTION
        // =====================================================

        private BrowserHistoryEntry? ResolveHistory(
            object artifact,
            List<DomainEntry> domains)
        {
            switch (artifact)
            {
                case BrowserHistoryEntry h:
                    return h;

                case DomainEntry d:
                    return d.SubEntries
                        .OrderByDescending(h => h.VisitTime)
                        .FirstOrDefault();

                case BrowserCookieEntry c:

                    return c.Relations
                        .Where(r => r.History != null)
                        .OrderByDescending(r => r.Time)
                        .Select(r => r.History)
                        .FirstOrDefault();

                case UserInputEntry i:

                    return i.Relations
                        .Where(r => r.History != null)
                        .OrderByDescending(r => r.Time)
                        .Select(r => r.History)
                        .FirstOrDefault();

                case BrowserExtensionEntry e:

                    return e.Relations
                        .Where(r => r.History != null)
                        .OrderByDescending(r => r.Time)
                        .Select(r => r.History)
                        .FirstOrDefault();

                case StorageEntry s:

                    return s.Relations
                        .Where(r => r.History != null)
                        .OrderByDescending(r => r.Time)
                        .Select(r => r.History)
                        .FirstOrDefault();

                default:
                    return null;
            }
        }

        // =====================================================
        // BACKWARD NAVIGATION
        // =====================================================

        private void TraverseBackward(
    BrowserHistoryEntry history,
    NavigationPathResult result,
    HashSet<BrowserHistoryEntry> visited)
        {
            if (!visited.Add(history))
                return;

            result.History.Add(history);

            foreach (var relation in history.Relations)
            {
                if (relation.Type != ArtifactRelationType.HistoryReferrer)
                    continue;

                if (relation.History == null)
                    continue;

                TraverseBackward(
                    relation.History,
                    result,
                    visited);
            }
        }

        // =====================================================
        // FORWARD NAVIGATION
        // =====================================================

        private void TraverseForward(
    BrowserHistoryEntry history,
    IEnumerable<BrowserHistoryEntry> allHistory,
    NavigationPathResult result,
    HashSet<BrowserHistoryEntry> visited)
        {
            if (!visited.Add(history))
                return;

            result.History.Add(history);

            foreach (var next in allHistory)
            {
                bool followsCurrent =
                    next.Relations.Any(r =>
                        r.Type == ArtifactRelationType.HistoryReferrer &&
                        r.History == history);

                if (!followsCurrent)
                    continue;

                TraverseForward(
                    next,
                    allHistory,
                    result,
                    visited);
            }
        }

        // =====================================================
        // DOMAIN COLLECTION
        // =====================================================

        private void CollectDomains(
            List<DomainEntry> domains,
            NavigationPathResult result)
        {
            foreach (var history in result.History)
            {
                var domain =
                    domains.FirstOrDefault(d =>
                        d.SubEntries.Contains(history));

                if (domain != null)
                    result.Domains.Add(domain);
            }
        }

        // =====================================================
        // COLLECT ALL RELATED ARTIFACTS
        // =====================================================

        private void CollectArtifacts(
    List<BrowserCookieEntry> cookies,
    List<UserInputEntry> inputs,
    List<BrowserExtensionEntry> extensions,
    List<StorageEntry> storage,
    List<AnalysisEntry> analysis,
    NavigationPathResult result)
        {
            // ---------------------------------------------
            // Cookies
            // ---------------------------------------------

            result.Cookies.UnionWith(
                cookies.Where(c =>
                    c.Relations.Any(r =>
                        r.History != null &&
                        result.History.Contains(r.History))));

            // ---------------------------------------------
            // User Inputs
            // ---------------------------------------------

            result.Inputs.UnionWith(
                inputs.Where(i =>
                    i.Relations.Any(r =>
                        r.History != null &&
                        result.History.Contains(r.History))));

            // ---------------------------------------------
            // Extensions
            // ---------------------------------------------

            result.Extensions.UnionWith(
                extensions.Where(e =>
                    e.Relations.Any(r =>
                        r.History != null &&
                        result.History.Contains(r.History))));

            // ---------------------------------------------
            // Storage
            // ---------------------------------------------

            result.Storage.UnionWith(
                storage.Where(s =>
                    s.Relations.Any(r =>
                        r.History != null &&
                        result.History.Contains(r.History))));

            // ---------------------------------------------
            // Analyse
            // ---------------------------------------------

            foreach (var a in analysis)
            {
                bool related = false;

                if (a.LinkedHistory.Any(result.History.Contains))
                    related = true;

                if (!related &&
                    a.LinkedCookies.Any(result.Cookies.Contains))
                    related = true;

                if (!related &&
                    a.LinkedInputs.Any(result.Inputs.Contains))
                    related = true;

                if (!related &&
                    a.LinkedExtensions.Any(result.Extensions.Contains))
                    related = true;

                if (!related &&
                    a.LinkedStorage.Any(result.Storage.Contains))
                    related = true;

                if (related)
                    result.Analysis.Add(a);
            }

            // ---------------------------------------------
            // Zweite Runde:
            // Analysen dürfen weitere Artefakte hinzufügen
            // ---------------------------------------------

            foreach (var a in result.Analysis.ToList())
            {
                result.History.UnionWith(a.LinkedHistory);
                result.Cookies.UnionWith(a.LinkedCookies);
                result.Inputs.UnionWith(a.LinkedInputs);
                result.Extensions.UnionWith(a.LinkedExtensions);
                result.Storage.UnionWith(a.LinkedStorage);
            }

            // ---------------------------------------------
            // Domains aus den sichtbaren History-Einträgen
            // ---------------------------------------------

            foreach (var history in result.History)
            {
                foreach (var relation in history.Relations)
                {
                    if (relation.Domain != null)
                        result.Domains.Add(relation.Domain);
                }
            }
        }
    }
}