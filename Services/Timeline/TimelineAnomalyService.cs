using Microsoft.WindowsAppSDK.Runtime.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineAnomalyService
    {
        public List<AnomalyEntry> Detect(
        List<DomainEntry> summarizedEntries,
        List<BrowserCookieEntry> cookies,
        List<WebDataAutofillEntry> autofillEntries,
        List<UserInputEntry> userInputs,
        List<BrowserExtensionEntry> extensions,
            List<string> blacklistDomains,
            List<string> suspiciousDomains)
        {
            var anomalies = new List<AnomalyEntry>();

            if (!summarizedEntries.Any())
                return anomalies;

            var anomalyIndex = new Dictionary<string, AnomalyEntry>();

            var orphanArtifacts = new List<OrphanArtifact>();

            // =====================================================
            // LEGITIME HISTORY ALS BASISANOMALIEN REGISTRIEREN
            // =====================================================

            foreach (var domain in summarizedEntries)
            {
                anomalyIndex[
                    $"history:{domain.VisitTime.Ticks}"] =
                        new AnomalyEntry
                        {
                            Type =
                                AnomalyType.Unknown,

                            FirstSeen =
                                domain.VisitTime,

                            LastSeen =
                                domain.VisitTime
                        };
            }

            DetectOrphanCookies(
                orphanArtifacts,
                summarizedEntries,
                cookies
            );

            DetectOrphanAutofill(
                orphanArtifacts,
                summarizedEntries,
                autofillEntries,
                userInputs
            );

            DetectSuspiciousExtensions(
                orphanArtifacts,
                anomalyIndex,
                extensions,
                summarizedEntries
            );

            DetectOrphanExtensions(
                orphanArtifacts,
                extensions
            );

            BuildDeletedHistoryPhases(
                anomalyIndex,
                orphanArtifacts,
                summarizedEntries);

            foreach (var domain in summarizedEntries)
            {
                var artifactTimes = domain.SubEntries.Any()
                    ? domain.SubEntries.Select(s => s.VisitTime)
                    : new[] { domain.VisitTime };

                // BLACKLIST
                if (blacklistDomains.Any(b =>
                    domain.Domain.Contains(b, StringComparison.OrdinalIgnoreCase)))
                {
                    var anomaly = AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.BlacklistedDomain,
                        domain,
                        artifactTimes,
                        severity: 5,
                        description: "Domain befindet sich auf der Blacklist"
                    );

                    anomaly.LinkedHistory.AddRange(domain.SubEntries);
                    anomaly.TargetType = AnomalyTargetType.Domain;

                    continue;
                }

                // SUSPICIOUS
                if (suspiciousDomains.Any(s =>
                    domain.Domain.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    var anomaly = AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.SuspiciousRedirect,
                        domain,
                        artifactTimes,
                        severity: 3,
                        description: "Verdächtige interne oder lokale Domain"
                    );

                    anomaly.LinkedHistory.AddRange(domain.SubEntries);
                    anomaly.TargetType = AnomalyTargetType.Domain;

                    continue;
                }
            }

            anomalies.AddRange(anomalyIndex.Values.Where(x => x.Type != AnomalyType.Unknown));

            ApplyCorrelationSeverityBoost(anomalies);

            ApplyArtifactSeverityBadges(anomalies); //für die Anzeige von Badges in den Artefakten

            return anomalies;
        }

        string GetAnomalyKey(
        AnomalyType type,
        DomainEntry domain,
        DateTime time,
        string? artifactId = null)
        {
            return type switch
            {
                AnomalyType.DeletedHistoryIndicator => $"{type}:active", // Damit nicht jedes Mal eine neue Anomalie erstellt wird.

                _ =>
                    $"{type}:{domain.Domain.ToLower()}"
            };
        }

        private void DetectSuspiciousExtensions(
    List<OrphanArtifact> orphanArtifacts,
    Dictionary<string, AnomalyEntry> anomalyIndex,
    List<BrowserExtensionEntry> extensions,
    List<DomainEntry> summarizedEntries)
        {
            if (extensions == null)
                return;

            foreach (var ext in extensions)
            {
                // =====================================================
                // SIGNALS
                // =====================================================

                bool localhostCommunication =
                    ext.AllPermissions.Any(p =>
                        p.Contains(
                            "localhost",
                            StringComparison.OrdinalIgnoreCase))
                    ||
                    ext.HostPermissions.Any(p =>
                        p.Contains(
                            "localhost",
                            StringComparison.OrdinalIgnoreCase))
                    ||
                    ext.ContentScripts.Any(c =>
                        c.Contains(
                            "localhost",
                            StringComparison.OrdinalIgnoreCase));

                bool dangerousScripting =
                    ext.AllPermissions.Any(p =>
                        p.Contains(
                            "scripting",
                            StringComparison.OrdinalIgnoreCase))
                    ||
                    ext.AllPermissions.Any(p =>
                        p.Contains(
                            "webRequestBlocking",
                            StringComparison.OrdinalIgnoreCase));

                bool runtimeActive =
                    ext.RuntimeArtifacts.Any();


                // =====================================================
                // DELETED HISTORY CORRELATION
                // =====================================================

                bool hasMatchingHistory = ext.Relations.Any(r => r.Type == ArtifactRelationType.ExtensionToWebStore);

                // =====================================================
                // NO REAL SIGNAL
                // =====================================================

                if (!localhostCommunication
                    &&
                    !ext.IsUnpacked
                    &&
                    ext.IsFromWebStore
                    &&
                    !dangerousScripting)
                {
                    continue;
                }

                // =====================================================
                // SEVERITY
                // =====================================================

                int severity = 2;

                if (dangerousScripting)
                    severity = 3;

                if (!ext.IsFromWebStore)
                    severity = 3;

                if (ext.IsUnpacked)
                    severity = 4;

                if (localhostCommunication)
                    severity = 5;

                // =====================================================
                // DESCRIPTION
                // =====================================================

                string description =
                    localhostCommunication
                        ? "Extension communicates with localhost infrastructure"
                        : ext.IsUnpacked
                            ? "Developer / unpacked extension detected"
                            : !ext.IsFromWebStore
                                ? "Extension not installed from Chrome Web Store"
                                : dangerousScripting
                                    ? "Extension has scripting capabilities"
                                    : "Suspicious extension detected";

                // =====================================================
                // CREATE
                // =====================================================

                var anomaly =
                    new AnomalyEntry
                    {
                        Type =
                            AnomalyType.SuspiciousExtension,

                        Title =
                            string.IsNullOrWhiteSpace(ext.Name)
                                ? ext.Id
                                : ext.Name,

                        Severity =
                            severity,

                        Confidence =
                            localhostCommunication
                                ? 95
                                : 75,

                        Description =
                            description,

                        FirstSeen =
                            ext.InstallTime,

                        LastSeen =
                            runtimeActive
                                ? ext.LastRuntimeActivity
                                : ext.InstallTime,

                        TargetType =
                            AnomalyTargetType.Extension,

                        TargetPosition =
                            ext.Position,

                        TargetYPercent =
                            64,

                        LinkedExtensions =
                        {
                    ext
                        },

                        Url =
                            ext.InstallLocation,

                        Domain =
                            ext.Name,

                        Color =
                            severity switch
                            {
                                5 => "#ff2b2b",
                                4 => "#ff7a2b",
                                3 => "#ffb52b",
                                2 => "#ffe12b",
                                _ => "#6cff6c"
                            },

                        Evidence =
                        {
                    $"Extension ID: {ext.Id}",
                    $"Source: {(ext.IsFromWebStore ? "WebStore" : "Non-WebStore")}",
                    $"Unpacked: {ext.IsUnpacked}",
                    $"Runtime Active: {runtimeActive}"
                        }
                    };

                // =====================================================
                // LOCALHOST
                // =====================================================

                if (localhostCommunication)
                {
                    anomaly.Evidence.Add(
                        "Localhost communication detected");
                }

                // =====================================================
                // SCRIPTING
                // =====================================================

                if (dangerousScripting)
                {
                    anomaly.Evidence.Add(
                        "Dangerous scripting capability");

                    foreach (var permission in ext.AllPermissions
                        .Where(p =>
                            p.Contains(
                                "scripting",
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            p.Contains(
                                "webRequestBlocking",
                                StringComparison.OrdinalIgnoreCase)))
                    {
                        anomaly.Evidence.Add(
                            $"Permission: {permission}");
                    }
                }

                // =====================================================
                // RUNTIME
                // =====================================================

                if (runtimeActive)
                {
                    anomaly.Evidence.Add(
                        "Runtime artifacts detected");
                }

                // =====================================================
                // STORE
                // =====================================================

                anomalyIndex[$"extension:{ext.Id}"] = anomaly;

                
            }
        }

        AnomalyEntry AddOrUpdateAnomaly(
        Dictionary<string, AnomalyEntry> index,
        AnomalyType type,
        DomainEntry domain,
        IEnumerable<DateTime> artifactTimes,
        int severity,
        string description,
        string? artifactId = null)
        {
            var first = artifactTimes.Min();
            var last = artifactTimes.Max();

            var key = GetAnomalyKey(type, domain, first, artifactId);

            if (index.TryGetValue(key, out var existing))
            {
                existing.Count++;
                existing.FirstSeen = existing.FirstSeen < first ? existing.FirstSeen : first;
                existing.LastSeen = existing.LastSeen > last ? existing.LastSeen : last;
                existing.Severity = Math.Max(existing.Severity, severity);
                existing.Evidence = existing.Evidence.Distinct().ToList();
                return existing;
            }

            var created = new AnomalyEntry
            {
                FirstSeen = first,
                LastSeen = last,

                LinkedDomain = domain,

                Type = type,

                Severity = severity,

                Description = description,

                Count = 1,

                Url = domain.Url,
                Domain = domain.Domain,

                IsBlacklistMatch =
        type == AnomalyType.BlacklistedDomain,

                IsSuspiciousRedirect =
        type == AnomalyType.SuspiciousRedirect,

                IsDeletedHistoryIndicator =
        type == AnomalyType.DeletedHistoryIndicator
            };

            index[key] = created;

            return created;
        }

        private void DetectOrphanExtensions(
    List<OrphanArtifact> orphanArtifacts,
    List<BrowserExtensionEntry> extensions)
        {
            foreach (var ext in extensions)
            {
                bool hasWebStoreRelation =
                    ext.Relations.Any(r =>
                        r.Type ==
                        ArtifactRelationType
                            .ExtensionToWebStore);

                // nur unpacked / non-webstore relevant
                if (ext.IsFromWebStore &&
                    hasWebStoreRelation)
                {
                    continue;
                }

                // wenn kein passender Store Visit
                orphanArtifacts.Add(
                    new OrphanArtifact
                    {
                        Time = ext.InstallTime,
                        Extension = ext
                    });
            }
        }


        private void DetectOrphanCookies(
     List<OrphanArtifact> orphanArtifacts,
     List<DomainEntry> summarizedEntries,
     List<BrowserCookieEntry> cookies,
     int toleranceSeconds = 60)
        {
            if (cookies == null)
                return;

            foreach (var cookie in cookies)
            {
                // IGNORE BACKGROUND
                if (cookie.Category ==
                    CookieCategory.BrowserBackground)
                {
                    continue;
                }

                bool hasRelation =
                    cookie.Relations.Any(r =>
                        r.Type ==
                        ArtifactRelationType
                            .CookieToHistory);

                if (hasRelation)
                    continue;

                orphanArtifacts.Add(
                    new OrphanArtifact
                    {
                        Time = cookie.Created,
                        Cookie = cookie
                    });
            }
        }

        private void DetectOrphanAutofill(
    List<OrphanArtifact> orphanArtifacts,
    List<DomainEntry> summarizedEntries,
    List<WebDataAutofillEntry> autofillEntries,
    List<UserInputEntry> userInputs,
    int toleranceSeconds = 120)
        {
            if (autofillEntries == null)
                return;

            foreach (var input in userInputs
                .Where(x => x.Type ==
                            UserInputType.Autofill))
            {
                bool hasRelation =
                    input.Relations.Any(r =>
                        r.Type ==
                        ArtifactRelationType
                            .AutofillToHistory);

                if (hasRelation)
                    continue;

                var autofill =
                    autofillEntries.FirstOrDefault(a =>
                        a.Value == input.Value);

                orphanArtifacts.Add(
                    new OrphanArtifact
                    {
                        Time = input.Time,
                        Autofill = autofill,
                        Input = input
                    });
            }
        }

        private void BuildDeletedHistoryPhases(
    Dictionary<string, AnomalyEntry> anomalyIndex,
    List<OrphanArtifact> orphanArtifacts,
    List<DomainEntry> summarizedEntries)
        {
            if (!orphanArtifacts.Any())
                return;

            var ordered =
                orphanArtifacts
                    .OrderBy(x => x.Time)
                    .ToList();

            int phaseIndex = 0;

            var currentPhase =
                new List<OrphanArtifact>();

            DateTime? currentEnd =
                null;

            foreach (var artifact in ordered)
            {
                var nextHistory =
                    summarizedEntries
                        .Where(h =>
                            h.VisitTime > artifact.Time)
                        .OrderBy(h => h.VisitTime)
                        .FirstOrDefault();

                if (currentEnd != null
                    &&
                    artifact.Time > currentEnd)
                {
                    CreateDeletedHistoryPhase(
                        anomalyIndex,
                        currentPhase,
                        phaseIndex++);

                    currentPhase.Clear();
                }

                currentPhase.Add(artifact);

                if (nextHistory != null)
                {
                    currentEnd =
                        nextHistory.VisitTime;
                }
            }

            if (currentPhase.Any())
            {
                CreateDeletedHistoryPhase(
                    anomalyIndex,
                    currentPhase,
                    phaseIndex);
            }
        }

        private void CreateDeletedHistoryPhase(
    Dictionary<string, AnomalyEntry> anomalyIndex,
    List<OrphanArtifact> artifacts,
    int phaseIndex)
        {
            if (!artifacts.Any())
                return;

            var first =
                artifacts.Min(x => x.Time);

            var last =
                artifacts.Max(x => x.Time);

            var anomaly =
                new AnomalyEntry
                {
                    Type =
                        AnomalyType.DeletedHistoryIndicator,

                    Title =
                        $"Deleted History Phase #{phaseIndex + 1}",

                    Description =
                        "Artefakte ohne passenden Verlaufseintrag",

                    Severity =
                        4,

                    Confidence =
                        90,

                    FirstSeen =
                        first,

                    LastSeen =
                        last,

                    Count =
                        artifacts.Count,

                    Color =
                        "#ff7a2b"
                };

            foreach (var artifact in artifacts)
            {
                if (artifact.Cookie != null)
                {
                    anomaly.LinkedCookies.Add(
                        artifact.Cookie);

                    anomaly.Evidence.Add(
                        $"Cookie: {artifact.Cookie.Name}");
                }

                if (artifact.Autofill != null)
                {
                    anomaly.Evidence.Add(
                        $"Autofill: {artifact.Autofill.Name}");
                }

                if (artifact.Extension != null)
                {
                    anomaly.LinkedExtensions.Add(
                        artifact.Extension);

                    anomaly.Evidence.Add(
                        $"Extension: {artifact.Extension.Name}");
                }

                if (artifact.Input != null)
                {
                    anomaly.LinkedInputs.Add(
                        artifact.Input);
                }
            }

            anomalyIndex[
                $"deleted-history:{phaseIndex}"] =
                    anomaly;
        }

        private void ApplyCorrelationSeverityBoost(
    List<AnomalyEntry> anomalies)
        {
            // =====================================================
            // ORIGINAL SEVERITIES
            // =====================================================

            var originalSeverity =
                anomalies.ToDictionary(
                    x => x,
                    x => x.Severity);

            // =====================================================
            // EXTENSION COUNTS
            // =====================================================

            var extensionCounts =
                new Dictionary<BrowserExtensionEntry, int>();

            foreach (var anomaly in anomalies)
            {
                foreach (var ext in anomaly.LinkedExtensions)
                {
                    if (!extensionCounts.ContainsKey(ext))
                        extensionCounts[ext] = 0;

                    extensionCounts[ext]++;
                }
            }

            // =====================================================
            // COOKIE COUNTS
            // =====================================================

            var cookieCounts =
                new Dictionary<BrowserCookieEntry, int>();

            foreach (var anomaly in anomalies)
            {
                foreach (var cookie in anomaly.LinkedCookies)
                {
                    if (!cookieCounts.ContainsKey(cookie))
                        cookieCounts[cookie] = 0;

                    cookieCounts[cookie]++;
                }
            }

            // =====================================================
            // INPUT COUNTS
            // =====================================================

            var inputCounts =
                new Dictionary<UserInputEntry, int>();

            foreach (var anomaly in anomalies)
            {
                foreach (var input in anomaly.LinkedInputs)
                {
                    if (!inputCounts.ContainsKey(input))
                        inputCounts[input] = 0;

                    inputCounts[input]++;
                }
            }

            // =====================================================
            // APPLY BOOSTS
            // =====================================================

            foreach (var anomaly in anomalies)
            {
                bool shouldBoost = false;

                // EXTENSIONS
                if (anomaly.LinkedExtensions.Any(e =>
                    extensionCounts.TryGetValue(e, out var count)
                    && count >= 3))
                {
                    shouldBoost = true;
                }

                // COOKIES
                if (anomaly.LinkedCookies.Any(c =>
                    cookieCounts.TryGetValue(c, out var count)
                    && count >= 3))
                {
                    shouldBoost = true;
                }

                // INPUTS
                if (anomaly.LinkedInputs.Any(i =>
                    inputCounts.TryGetValue(i, out var count)
                    && count >= 3))
                {
                    shouldBoost = true;
                }

                if (!shouldBoost)
                    continue;

                anomaly.IsCorrelated = true;

                anomaly.CorrelationBoost = 1;

            }
        }

        private void ApplyArtifactSeverityBadges(
    List<AnomalyEntry> anomalies)
        {
            foreach (var anomaly in anomalies)
            {
                // =====================================================
                // COOKIES
                // =====================================================

                foreach (var cookie in anomaly.LinkedCookies)
                {
                    int effectiveSeverity =
                        anomaly.Severity +
                        anomaly.CorrelationBoost;

                    cookie.HighestAnomalySeverity =
                        Math.Max(
                            cookie.HighestAnomalySeverity,
                            Math.Min(5, effectiveSeverity));
                }

                // =====================================================
                // EXTENSIONS
                // =====================================================

                foreach (var ext in anomaly.LinkedExtensions)
                {
                    int effectiveSeverity =
                        Math.Min(
                            5,
                            anomaly.Severity +
                            anomaly.CorrelationBoost);

                    ext.HighestAnomalySeverity =
                        Math.Max(
                            ext.HighestAnomalySeverity,
                            effectiveSeverity);
                }

                // =====================================================
                // INPUTS
                // =====================================================

                foreach (var input in anomaly.LinkedInputs)
                {
                    int effectiveSeverity =
                        Math.Min(
                            5,
                            anomaly.Severity +
                            anomaly.CorrelationBoost);

                    input.HighestAnomalySeverity =
                        Math.Max(
                            input.HighestAnomalySeverity,
                            effectiveSeverity);
                }

                // =====================================================
                // STORAGE
                // =====================================================

                foreach (var storage in anomaly.LinkedStorage)
                {
                    int effectiveSeverity =
                        Math.Min(
                            5,
                            anomaly.Severity +
                            anomaly.CorrelationBoost);

                    storage.HighestAnomalySeverity =
                        Math.Max(
                            storage.HighestAnomalySeverity,
                            effectiveSeverity);
                }

                // =====================================================
                // HISTORY
                // =====================================================

                foreach (var history in anomaly.LinkedHistory)
                {
                    int effectiveSeverity =
                        Math.Min(
                            5,
                            anomaly.Severity +
                            anomaly.CorrelationBoost);

                    history.HighestAnomalySeverity =
                        Math.Max(
                            history.HighestAnomalySeverity,
                            effectiveSeverity);
                }

                // =====================================================
                // DOMAINS
                // =====================================================

                if (anomaly.LinkedDomain != null)
                {
                    int effectiveSeverity =
                        Math.Min(
                            5,
                            anomaly.Severity +
                            anomaly.CorrelationBoost);

                    anomaly.LinkedDomain.HighestAnomalySeverity =
                        Math.Max(
                            anomaly.LinkedDomain.HighestAnomalySeverity,
                            effectiveSeverity);
                }
            }
        }
    }
}
