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

            DetectOrphanCookies(
                anomalyIndex,
                summarizedEntries,
                cookies
            );

            DetectOrphanAutofill(
                anomalyIndex,
                summarizedEntries,
                autofillEntries,
                userInputs
            );

            DetectSuspiciousExtensions(
                anomalyIndex,
                extensions
            );

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

            anomalies.AddRange(anomalyIndex.Values);

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
                AnomalyType.DeletedHistoryIndicator =>
                    $"{type}:{domain.Domain.ToLower()}:{time:yyyyMMddHHmmss}:{artifactId}",

                _ =>
                    $"{type}:{domain.Domain.ToLower()}"
            };
        }

        private void DetectSuspiciousExtensions(
    Dictionary<string, AnomalyEntry> anomalyIndex,
    List<BrowserExtensionEntry> extensions)
        {
            if (extensions == null)
                return;

            foreach (var ext in extensions)
            {
                // =====================================================
                // SUSPICIOUS ID
                // =====================================================

                bool suspiciousId =
                    string.IsNullOrWhiteSpace(ext.Id)
                    ||
                    ext.Id.Length < 32
                    ||
                    !ext.Id.All(c => c >= 'a' && c <= 'p');

                if (suspiciousId)
                {
                    var extensionAnomaly =
                        new AnomalyEntry
                        {
                            Type =
                                AnomalyType.SuspiciousExtension,

                            Title =
                                ext.Name,

                            Severity =
                                ext.InstallLocation.Contains(
                                    "localhost",
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                ext.Id.Contains(
                                    "localhost",
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                ext.Name.Contains(
                                    "localhost",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? 5
                                    : ext.IsUnpacked
                                        ? 4
                                        : 2,

                            Confidence =
                                90,

                            Description =
                                ext.InstallLocation.Contains(
                                    "localhost",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? "Localhost developer extension detected"
                                    : ext.IsUnpacked
                                        ? "Developer / unpacked extension detected"
                                        : "Extension not installed from Chrome Web Store",

                            FirstSeen =
                                ext.InstallTime,

                            LastSeen =
                                ext.LastRuntimeActivity != default
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
                                ext.InstallLocation.Contains(
                                    "localhost",
                                    StringComparison.OrdinalIgnoreCase)
                                    ? "#ff2b2b"
                                    : ext.IsUnpacked
                                        ? "#ff7a2b"
                                        : "#ffe12b",

                            Tags =
                            {
                                "extension",
                                "suspicious-extension",
                                "persistence",
                                "runtime-artifact"
                            },

                            Evidence =
                            {
                                $"Extension ID: {ext.Id}",
                                $"Source: {(ext.IsFromWebStore ? "WebStore" : "Non-WebStore")}",
                                $"Install Path: {ext.InstallLocation}",
                                $"Runtime Artifacts: {ext.RuntimeArtifacts.Count}",
                                "Invalid Chromium extension ID"
                            }
                        };

                    anomalyIndex[
                        $"suspicious_ext_id:{ext.Id}"] =
                            extensionAnomaly;

                    continue;
                }

                // =====================================================
                // NON WEBSTORE
                // =====================================================

                var anomaly =
                    new AnomalyEntry
                    {
                        Type =
                            AnomalyType.SuspiciousExtension,

                        Title =
                            ext.Name,

                        Severity =
                            ext.IsUnpacked ? 3 : 2,

                        Confidence =
                            ext.IsUnpacked ? 80 : 60,

                        Description =
                            ext.IsUnpacked
                                ? "Developer / unpacked extension detected"
                                : "Extension not installed from Chrome Web Store",

                        FirstSeen =
                            ext.InstallTime,

                        LastSeen =
                            ext.LastRuntimeActivity != default
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
                            ext.IsUnpacked
                                ? "#ffb52b"
                                : "#ffe12b",

                        Tags =
                        {
                            "extension",
                            "browser-extension",
                            "developer-extension"
                        },

                        Evidence =
                        {
                            $"Extension ID: {ext.Id}",
                            $"Install Path: {ext.InstallLocation}",
                            $"Permissions: {ext.AllPermissions.Count}",
                            $"Runtime Artifacts: {ext.RuntimeArtifacts.Count}",
                            ext.IsUnpacked
                                ? "Unpacked developer extension"
                                : "Non WebStore extension"
                        }
                    };
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
            Dictionary<string, AnomalyEntry> anomalyIndex,
            List<DomainEntry> summarizedEntries,
            List<BrowserCookieEntry> cookies,
            int toleranceSeconds = 60)
        {
            if (cookies == null) return;

            foreach (var cookie in cookies)
            {
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
                    var linkedDomain = FindClosestDomain(
                        summarizedEntries,
                        cookie.Created
                    );

                    var anomaly = AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.DeletedHistoryIndicator,
                        linkedDomain ?? new DomainEntry { Domain = cookieDomain },
                        new[] { cookie.Created },
                        severity: 4,
                        description: $"Cookie für Domain '{cookieDomain}' ohne passenden Verlaufseintrag"
                    );

                    anomaly.Evidence.Add($"Cookie: {cookie.Name}");

                    anomaly.Evidence.Add($"Domain: {cookie.Host}");

                    anomaly.LinkedCookies.Add(cookie);
                    anomaly.TargetType = AnomalyTargetType.Cookie;
                    anomaly.TargetPosition = cookie.Position;
                    anomaly.TargetYPercent = 43;
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
            Dictionary<string, AnomalyEntry> anomalyIndex,
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
                    var linkedDomain = FindClosestDomain(
                        summarizedEntries,
                        autofill.DateCreated
                    );

                    var anomaly = AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.DeletedHistoryIndicator,
                        linkedDomain ?? new DomainEntry { Domain = "[unknown]" },
                        new[] { autofill.DateCreated },
                        severity: 5,
                        description: $"Autofill '{autofill.Name}' ohne zugehörigen Seitenaufruf"
                    );

                    var linkedInput =
                        userInputs.FirstOrDefault(x =>
                            x.Type == UserInputType.Autofill
                            &&
                            x.Value == autofill.Value
                            &&
                            Math.Abs(
                                (x.Time - autofill.DateCreated)
                                .TotalSeconds) < 5);

                                        if (linkedInput != null)
                                        {
                                            anomaly.LinkedInputs.Add(
                                                linkedInput);
                                        }

                    anomaly.TargetType = AnomalyTargetType.Autofill;
                    anomaly.TargetPosition = autofill.Position;
                    anomaly.TargetYPercent = 50;
                }
            }
        }
    }
}
