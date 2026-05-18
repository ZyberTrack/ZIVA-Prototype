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

                bool hasMatchingHistory = summarizedEntries.Any(h =>
            (
            h.Url.Contains(
                "chrome.google.com/webstore",
                StringComparison.OrdinalIgnoreCase)
            ||
            h.Url.Contains(
                "chromewebstore.google.com",
                StringComparison.OrdinalIgnoreCase)
        )
        &&
        h.VisitTime <= ext.InstallTime
        &&
        (ext.InstallTime - h.VisitTime)
            .TotalSeconds <= 300);

                if (!hasMatchingHistory)
                {
                    orphanArtifacts.Add(
                        new OrphanArtifact
                        {
                            Time = ext.InstallTime,
                            Extension = ext
                        });
                }

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


        string NormalizeDomain(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            input = input.Trim().ToLower();

            if (input.StartsWith("."))
                input = input[1..];

            // URLs erlauben
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
                return uri.Host;

            return input;
        }

        private void DetectOrphanCookies(
            List<OrphanArtifact> orphanArtifacts,
            List<DomainEntry> summarizedEntries,
            List<BrowserCookieEntry> cookies,
            int toleranceSeconds = 60)
        {
            if (cookies == null) return;

            foreach (var cookie in cookies)
            {


                // IGNORE BROWSER BACKGROUND TRAFFIC
                if (cookie.Category ==
                    CookieCategory.BrowserBackground)
                {
                    continue;
                }

                string cookieDomain = NormalizeDomain(cookie.Host);

                bool hasMatchingHistory = summarizedEntries.Any(domain =>
                {
                    string domainNorm = NormalizeDomain(domain.Domain);

                    if (!domainNorm.EndsWith(cookieDomain) &&
                        !cookieDomain.EndsWith(domainNorm))
                        return false;

                    return Math.Abs((domain.VisitTime - cookie.Created).TotalSeconds)
                           <= toleranceSeconds;
                });

                if (!hasMatchingHistory)
                {
                    orphanArtifacts.Add(
                        new OrphanArtifact
                        {
                            Time = cookie.Created,
                            Cookie = cookie
                        });
                }
            }
        }


        private DomainEntry? FindClosestDomain(
            List<DomainEntry> summarizedEntries,
            DateTime time)
        {
            return summarizedEntries
                .OrderBy(d => Math.Abs((d.VisitTime - time).TotalSeconds))
                .FirstOrDefault();
        }

        private void DetectOrphanAutofill(
            List<OrphanArtifact> orphanArtifacts,
            List<DomainEntry> summarizedEntries,
            List<WebDataAutofillEntry> autofillEntries,
            List<UserInputEntry> userInputs,
            int toleranceSeconds = 120)
        {
            if (autofillEntries == null) return;

            foreach (var autofill in autofillEntries)
            {
                bool hasMatchingDomain = summarizedEntries.Any(domain =>
                    Math.Abs((domain.VisitTime - autofill.DateCreated).TotalSeconds)
                    <= toleranceSeconds
                );

                if (!hasMatchingDomain)
                {
                    var linkedInput =
                        userInputs.FirstOrDefault(x =>
                            x.Type == UserInputType.Autofill
                            &&
                            x.Value == autofill.Value
                            &&
                            Math.Abs(
                                (x.Time - autofill.DateCreated)
                                .TotalSeconds) < 5);

                    orphanArtifacts.Add(
                        new OrphanArtifact
                        {
                            Time = autofill.DateCreated,
                            Autofill = autofill,
                            Input = linkedInput
                        });
                }
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
    }
}
