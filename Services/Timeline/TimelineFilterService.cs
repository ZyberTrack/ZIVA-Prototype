using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineFilterService
    {

        private readonly NavigationPathService _navigationService;

        public TimelineFilterService(NavigationPathService navigationService)
        {
            _navigationService = navigationService;
        }

        public TimelineFilterResult ApplyNavigationSelection(
    TimelineFilterResult current,
    bool showNavigationPath,
    bool includeNavigationPaths,
    object? navigationRootArtifact)
        {
            if (!showNavigationPath &&
                !includeNavigationPaths)
            {
                return current;
            }

            var result = new TimelineFilterResult(current);

            //------------------------------------------------------
            // Einzelner Navigation Path
            //------------------------------------------------------

            if (showNavigationPath &&
                navigationRootArtifact != null)
            {
                var path =
                    _navigationService.BuildNavigationPath(
                        navigationRootArtifact,
                        current.Domains,
                        current.Cookies,
                        current.Inputs,
                        current.Extensions,
                        current.Storage,
                        current.Analysis);

                result.History = path.History.Distinct().ToList();
                result.Cookies = path.Cookies.Distinct().ToList();
                result.Inputs = path.Inputs.Distinct().ToList();
                result.Extensions = path.Extensions.Distinct().ToList();
                result.Storage = path.Storage.Distinct().ToList();
                result.Domains = path.Domains.Distinct().ToList();
                result.Analysis = path.Analysis.Distinct().ToList();

                return result;
            }

            //------------------------------------------------------
            // Alle Navigation Paths
            //------------------------------------------------------

            if (includeNavigationPaths)
            {
                var history = new HashSet<BrowserHistoryEntry>();
                var cookies = new HashSet<BrowserCookieEntry>();
                var inputs = new HashSet<UserInputEntry>();
                var extensions = new HashSet<BrowserExtensionEntry>();
                var storage = new HashSet<StorageEntry>();
                var domains = new HashSet<DomainEntry>();
                var analysis = new HashSet<AnalysisEntry>();

                foreach (var entry in current.History)
                {
                    var path =
                        _navigationService.BuildNavigationPath(
                            entry,
                            current.Domains,
                            current.Cookies,
                            current.Inputs,
                            current.Extensions,
                            current.Storage,
                            current.Analysis);

                    history.UnionWith(path.History);
                    cookies.UnionWith(path.Cookies);
                    inputs.UnionWith(path.Inputs);
                    extensions.UnionWith(path.Extensions);
                    storage.UnionWith(path.Storage);
                    domains.UnionWith(path.Domains);
                    analysis.UnionWith(path.Analysis);
                }

                result.History = history.ToList();
                result.Cookies = cookies.ToList();
                result.Inputs = inputs.ToList();
                result.Extensions = extensions.ToList();
                result.Storage = storage.ToList();
                result.Domains = domains.ToList();
                result.Analysis = analysis.ToList();
            }

            return result;
        }

        public TimelineFilterResult ApplyDomainSelection(
     TimelineFilterResult current,
     string? selectedDomain)
        {
            if (string.IsNullOrWhiteSpace(selectedDomain))
                return current;

            var result = new TimelineFilterResult(current);

            //------------------------------------------------------
            // Gewählte Domain
            //------------------------------------------------------

            result.Domains = current.Domains
                .Where(d =>
                    d.Domain.Equals(
                        selectedDomain,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            //------------------------------------------------------
            // History dieser Domain
            //------------------------------------------------------

            result.History = result.Domains
                .SelectMany(d => d.SubEntries)
                .Distinct()
                .ToList();

            var historySet = result.History.ToHashSet();

            //------------------------------------------------------
            // Alle verknüpften Cookies
            //------------------------------------------------------

            result.Cookies = current.Cookies
                .Where(c =>
                    c.Relations.Any(r =>
                        r.History != null &&
                        historySet.Contains(r.History)))
                .ToList();

            //------------------------------------------------------
            // Alle verknüpften Inputs
            //------------------------------------------------------

            result.Inputs = current.Inputs
                .Where(i =>
                    i.Relations.Any(r =>
                        r.History != null &&
                        historySet.Contains(r.History)))
                .ToList();

            //------------------------------------------------------
            // Alle verknüpften Extensions
            //------------------------------------------------------

            result.Extensions = current.Extensions
                .Where(e =>
                    e.Relations.Any(r =>
                        r.History != null &&
                        historySet.Contains(r.History)))
                .ToList();

            //------------------------------------------------------
            // Alle verknüpften Storage Artefakte
            //------------------------------------------------------

            result.Storage = current.Storage
                .Where(s =>
                    s.Relations.Any(r =>
                        r.History != null &&
                        historySet.Contains(r.History)))
                .ToList();

            //------------------------------------------------------
            // Analyse
            //------------------------------------------------------

            result.Analysis = current.Analysis
                .Where(a =>
                    a.LinkedHistory.Any(historySet.Contains) ||

                    a.LinkedCookies.Any(result.Cookies.Contains) ||

                    a.LinkedInputs.Any(result.Inputs.Contains) ||

                    a.LinkedExtensions.Any(result.Extensions.Contains) ||

                    a.LinkedStorage.Any(result.Storage.Contains))
                .ToList();

            return result;
        }

        public TimelineFilterResult ApplyAnalysisSelection(
    TimelineFilterResult current,
    bool filterActive,
    bool showInformation,
    bool showWarnings,
    bool showAnomalies)
        {
            if (!filterActive)
                return current;

            var result = new TimelineFilterResult(current);

            //------------------------------------------------------
            // Analyse nach Kategorie filtern
            //------------------------------------------------------

            result.Analysis = current.Analysis
                .Where(a =>
                       (showInformation && a.Category == AnalysisCategory.Information)
                    || (showWarnings && a.Category == AnalysisCategory.Warning)
                    || (showAnomalies && a.Category == AnalysisCategory.Anomaly))
                .ToList();

            //------------------------------------------------------
            // Artefakte auf diese Analysen einschränken
            //------------------------------------------------------

            result.History = current.History
                .Where(h => result.Analysis.Any(a => a.LinkedHistory.Contains(h)))
                .ToList();

            result.Cookies = current.Cookies
                .Where(c => result.Analysis.Any(a => a.LinkedCookies.Contains(c)))
                .ToList();

            result.Inputs = current.Inputs
                .Where(i => result.Analysis.Any(a => a.LinkedInputs.Contains(i)))
                .ToList();

            result.Extensions = current.Extensions
                .Where(e => result.Analysis.Any(a => a.LinkedExtensions.Contains(e)))
                .ToList();

            result.Storage = current.Storage
                .Where(s => result.Analysis.Any(a => a.LinkedStorage.Contains(s)))
                .ToList();

            result.Domains = current.Domains
                .Where(d => result.History.Any(h => d.SubEntries.Contains(h)))
                .ToList();

            return result;
        }

        public TimelineFilterResult ApplyArtifactSelection(
    TimelineFilterResult current,
    bool filterActive,
    bool keepHistory,
    bool showHistory,
    bool showCookies,
    bool showInputs,
    bool showExtensions,
    bool showStorage)
        {
            if (!filterActive)
                return current;

            var result = new TimelineFilterResult(current);

            //------------------------------------------------------
            // History
            //------------------------------------------------------

            if (!showHistory && !keepHistory)
                result.History.Clear();

            //------------------------------------------------------
            // Restliche Artefakte
            //------------------------------------------------------

            if (!showCookies)
                result.Cookies.Clear();

            if (!showInputs)
                result.Inputs.Clear();

            if (!showExtensions)
                result.Extensions.Clear();

            if (!showStorage)
                result.Storage.Clear();

            //------------------------------------------------------
            // Analyse an sichtbare Artefakte koppeln
            //------------------------------------------------------

            result.Analysis = current.Analysis
                .Where(a =>
                {
                    bool linked = false;

                    if (result.History.Any())
                        linked |= a.LinkedHistory.Any(result.History.Contains);

                    if (result.Cookies.Any())
                        linked |= a.LinkedCookies.Any(result.Cookies.Contains);

                    if (result.Inputs.Any())
                        linked |= a.LinkedInputs.Any(result.Inputs.Contains);

                    if (result.Extensions.Any())
                        linked |= a.LinkedExtensions.Any(result.Extensions.Contains);

                    if (result.Storage.Any())
                        linked |= a.LinkedStorage.Any(result.Storage.Contains);

                    return linked;
                })
                .ToList();

            return result;
        }
    }
}
