using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineAnomalyService
    {
        public List<AnomalyEntry> Detect(
            List<DomainEntry> summarizedEntries,
            List<BrowserCookieEntry> cookies,
            List<WebDataAutofillEntry> autofillEntries,
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
                autofillEntries
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
                    AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.BlacklistedDomain,
                        domain,
                        artifactTimes,
                        severity: 5,
                        description: "Domain befindet sich auf der Blacklist"
                    );

                    continue;
                }

                // SUSPICIOUS
                if (suspiciousDomains.Any(s =>
                    domain.Domain.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.SuspiciousRedirect,
                        domain,
                        artifactTimes,
                        severity: 3,
                        description: "Verdächtige interne oder lokale Domain"
                    );
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


        void AddOrUpdateAnomaly(
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
                return;
            }

            index[key] = new AnomalyEntry
            {
                FirstSeen = first,
                LastSeen = last,
                LinkedDomain = domain,
                Type = type,
                Severity = severity,
                Description = description,
                Count = 1
            };
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

                    AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.DeletedHistoryIndicator,
                        linkedDomain ?? new DomainEntry { Domain = cookieDomain },
                        new[] { cookie.Created },
                        severity: 4,
                        description: $"Cookie für Domain '{cookieDomain}' ohne passenden Verlaufseintrag"
                    );
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

                    AddOrUpdateAnomaly(
                        anomalyIndex,
                        AnomalyType.DeletedHistoryIndicator,
                        linkedDomain ?? new DomainEntry { Domain = "[unknown]" },
                        new[] { autofill.DateCreated },
                        severity: 5,
                        description: $"Autofill '{autofill.Name}' ohne zugehörigen Seitenaufruf"
                    );
                }
            }
        }


    }
}
