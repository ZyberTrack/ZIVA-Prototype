using Microsoft.WindowsAppSDK.Runtime.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineAnalysisService
    {
        public List<AnalysisEntry> Detect(
    List<DomainEntry> summarizedEntries,
    List<BrowserCookieEntry> cookies,
    List<WebDataAutofillEntry> autofillEntries,
    List<UserInputEntry> userInputs,
    List<BrowserExtensionEntry> extensions,
    List<StorageEntry> storageEntries,
    List<string> blacklistDomains,
    List<string> suspiciousDomains)
        {
            var anomalies = new List<AnalysisEntry>();

            if (!summarizedEntries.Any())
                return anomalies;

            var anomalyIndex = new Dictionary<string, AnalysisEntry>();

            var orphanArtifacts = new List<OrphanArtifact>();

            // =====================================================
            // LEGITIME HISTORY ALS BASISANOMALIEN REGISTRIEREN
            // =====================================================

            foreach (var domain in summarizedEntries)
            {
                anomalyIndex[
                    $"history:{domain.VisitTime.Ticks}"] =
                        new AnalysisEntry
                        {
                            Type =
                                AnalysisType.Unknown,

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


            // Entfernt weil für den Scope der Analyse nicht relevant.
            /* 
            DetectOrphanExtensions(
                orphanArtifacts,
                extensions
            );*/

            BuildDeletedHistoryPhases(
                anomalyIndex,
                orphanArtifacts,
                summarizedEntries);

            DetectSensitiveStorage(
                anomalyIndex,
                storageEntries,
                summarizedEntries
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
                        AnalysisType.BlacklistedDomain,
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
                        AnalysisType.SuspiciousRedirect,
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

            anomalies.AddRange(anomalyIndex.Values.Where(x => x.Type != AnalysisType.Unknown));

            ApplyCorrelationSeverityBoost(anomalies);

            ApplyArtifactSeverityBadges(anomalies); //für die Anzeige von Badges in den Artefakten

            return anomalies;
        }

        string GetAnomalyKey(
        AnalysisType type,
        DomainEntry domain,
        DateTime time,
        string? artifactId = null)
        {
            return type switch
            {
                AnalysisType.DeletedHistoryIndicator => $"{type}:active", // Damit nicht jedes Mal eine neue Anomalie erstellt wird.

                _ =>
                    $"{type}:{domain.Domain.ToLower()}"
            };
        }

        private AnalysisCategory GetCategory(AnalysisType type)
        {
            return type switch
            {
                // -----------------------------
                // INFORMATION
                // -----------------------------
                AnalysisType.JwtToken => AnalysisCategory.Information,
                AnalysisType.ApiKey => AnalysisCategory.Information,

                // -----------------------------
                // WARNINGS
                // -----------------------------
                AnalysisType.AuthenticationData => AnalysisCategory.Warning,
                AnalysisType.SensitiveStorageContent => AnalysisCategory.Warning,

                // Falls vorhanden:
                AnalysisType.PlaintextPassword => AnalysisCategory.Warning,
                AnalysisType.SessionToken => AnalysisCategory.Warning,
                AnalysisType.OAuthToken => AnalysisCategory.Warning,

                // -----------------------------
                // ANOMALIES
                // -----------------------------
                AnalysisType.BlacklistedDomain => AnalysisCategory.Anomaly,
                AnalysisType.SuspiciousRedirect => AnalysisCategory.Anomaly,
                AnalysisType.DeletedHistoryIndicator => AnalysisCategory.Anomaly,
                AnalysisType.SuspiciousExtension => AnalysisCategory.Anomaly,
                AnalysisType.TimeManipulation => AnalysisCategory.Anomaly,
                AnalysisType.BurstActivity => AnalysisCategory.Anomaly,
                AnalysisType.SessionHijackIndicator => AnalysisCategory.Anomaly,
                AnalysisType.CorrelatedThreat => AnalysisCategory.Anomaly,

                _ => AnalysisCategory.Information
            };
        }

        // =====================================================
        // Sensitive Data Detection
        // =====================================================


        private void DetectSensitiveStorage(
     Dictionary<string, AnalysisEntry> analysisIndex,
     List<StorageEntry> storageEntries,
     List<DomainEntry> domains)
        {
            foreach (var storage in storageEntries)
            {
                // Zugehörige Domain bestimmen
                DomainEntry? domain =
                    storage.Relations
                        .Select(r => r.Domain)
                        .FirstOrDefault();

                // Fallback über Origin
                if (domain == null &&
                    Uri.TryCreate(storage.Origin, UriKind.Absolute, out var uri))
                {
                    domain = domains.FirstOrDefault(d =>
                        string.Equals(
                            d.Domain,
                            uri.Host,
                            StringComparison.OrdinalIgnoreCase));
                }

                // ---------------- Sensitive Flag ----------------

                if (storage.IsSensitive)
                {
                    CreateStorageAnalysis(
                        analysisIndex,
                        storage,
                        domain,
                        AnalysisCategory.Warning,
                        AnalysisType.SensitiveStorageContent,
                        "Sensitive Storage Content",
                        "Storage entry is marked as sensitive.",
                        3);
                }

                // ---------------- Authentication ----------------

                if (ContainsAuthenticationData(storage))
                {
                    CreateStorageAnalysis(
                        analysisIndex,
                        storage,
                        domain,
                        AnalysisCategory.Warning,
                        AnalysisType.AuthenticationData,
                        "Authentication Data",
                        "Authentication related data detected.",
                        4);
                }

                // ---------------- JWT ----------------

                if (LooksLikeJwt(storage.Value))
                {
                    CreateStorageAnalysis(
                        analysisIndex,
                        storage,
                        domain,
                        AnalysisCategory.Information,
                        AnalysisType.JwtToken,
                        "JWT Token",
                        "JWT token detected.",
                        2);
                }

                // ---------------- API Key ----------------

                if (LooksLikeApiKey(storage.Value))
                {
                    CreateStorageAnalysis(
                        analysisIndex,
                        storage,
                        domain,
                        AnalysisCategory.Information,
                        AnalysisType.ApiKey,
                        "API Key",
                        "Potential API key detected.",
                        2);
                }
            }
        }

        private AnalysisEntry CreateStorageAnalysis(
    Dictionary<string, AnalysisEntry> analysisIndex,
    StorageEntry storage,
    DomainEntry? domain,
    AnalysisCategory category,
    AnalysisType type,
    string title,
    string description,
    int severity)
        {
            string key =
                $"storage:{type}:{storage.Key}:{storage.Time.Ticks}";

            if (analysisIndex.TryGetValue(key, out var existing))
                return existing;

            var analysis = new AnalysisEntry
            {
                Category = category,
                Type = type,

                Title = title,
                Description = description,

                Severity = severity,
                Confidence = 90,

                FirstSeen = storage.Time,
                LastSeen = storage.Time,

                TargetType = AnomalyTargetType.Storage,
                TargetPosition = storage.Position,
                TargetYPercent = 57,

                LinkedDomain = domain,

                LinkedStorage =
                {
                    storage
                },

                Url = domain?.Url ?? storage.Origin,
                Domain = domain?.Domain ?? storage.Origin
            };

            analysis.Evidence.Add($"Key: {storage.Key}");

            analysisIndex[key] = analysis;

            return analysis;
        }

        private bool ContainsAuthenticationData(StorageEntry storage)
        {
            string text =
                $"{storage.Key} {storage.Value}".ToLower();

            string[] words =
            {
        "authorization",
        "bearer",
        "access_token",
        "refresh_token",
        "id_token",
        "jwt",
        "session",
        "cookie",
        "auth",
        "oauth",
        "token"
    };

            return words.Any(text.Contains);
        }

        private bool LooksLikeJwt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(
                value,
                @"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$");
        }

        private bool LooksLikeApiKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Length > 30 &&
                   value.Any(char.IsUpper) &&
                   value.Any(char.IsLower) &&
                   value.Any(char.IsDigit);
        }

        private void DetectSuspiciousExtensions(
    List<OrphanArtifact> orphanArtifacts,
    Dictionary<string, AnalysisEntry> anomalyIndex,
    List<BrowserExtensionEntry> extensions,
    List<DomainEntry> summarizedEntries)
        {
            if (extensions == null)
                return;

            foreach (var ext in extensions)
            {
                // =====================================================
                // UPDATE SOURCE
                // =====================================================

                string updateUrl =
                    ext.UpdateUrl?.Trim() ?? "";

                bool validGoogleUpdate =
                    updateUrl.StartsWith(
                        "https://clients2.google.com/service/update2/",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    updateUrl.StartsWith(
                        "https://clients2.googleusercontent.com/service/update2/",
                        StringComparison.OrdinalIgnoreCase);

                // =====================================================
                // INSTALL LOCATION
                // =====================================================

                string installLocation =
                    ext.InstallLocation ?? "";

                bool suspiciousInstallLocation =
                    !installLocation.Contains(
                        @"\Extensions\",
                        StringComparison.OrdinalIgnoreCase);

                // =====================================================
                // DEVELOPER EXTENSION
                // =====================================================

                bool developerExtension =
                    ext.IsUnpacked;

                // =====================================================
                // LOCALHOST COMMUNICATION
                // =====================================================

                bool localhostCommunication =
                    ext.AllPermissions.Any(p =>
                        p.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                        p.Contains("127.0.0.1"))
                    ||
                    ext.HostPermissions.Any(p =>
                        p.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                        p.Contains("127.0.0.1"))
                    ||
                    ext.ContentScripts.Any(c =>
                        c.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                        c.Contains("127.0.0.1"));

                // =====================================================
                // UPDATE SOURCE
                // =====================================================

                bool suspiciousUpdateSource =
                    developerExtension ||
                    string.IsNullOrWhiteSpace(updateUrl) ||
                    !validGoogleUpdate;

                // =====================================================
                // NOTHING SUSPICIOUS
                // =====================================================

                if (!developerExtension &&
                    !suspiciousUpdateSource &&
                    !suspiciousInstallLocation &&
                    !localhostCommunication)
                {
                    continue;
                }

                // =====================================================
                // DESCRIPTION
                // =====================================================

                List<string> reasons = new();

                if (developerExtension)
                    reasons.Add("loaded in Chrome developer mode");

                if (suspiciousUpdateSource)
                    reasons.Add("uses a non-standard update source");

                if (suspiciousInstallLocation)
                    reasons.Add("is installed outside the default Chrome Extensions directory");

                if (localhostCommunication)
                    reasons.Add("communicates with localhost");

                string description =
                    "Extension " + string.Join(", ", reasons) + ".";

                // =====================================================
                // CREATE
                // =====================================================

                var anomaly =
                    new AnalysisEntry
                    {
                        Category =
                            GetCategory(
                                AnalysisType.SuspiciousExtension),

                        Type =
                            AnalysisType.SuspiciousExtension,

                        Title =
                            string.IsNullOrWhiteSpace(ext.Name)
                                ? ext.Id
                                : ext.Name,

                        Severity = localhostCommunication ? 5 : 4,

                        Confidence = 95,

                        Description = description,

                        FirstSeen = ext.InstallTime,

                        LastSeen = ext.InstallTime,

                        TargetType =
                            AnomalyTargetType.Extension,

                        TargetPosition =
                            ext.Position,

                        TargetYPercent = 64,

                        LinkedExtensions =
                        {
                    ext
                        },

                        Url = ext.InstallLocation,

                        Domain = ext.Name,

                        Color = localhostCommunication
                            ? "#ff2b2b"
                            : "#ff7a2b",

                        Evidence =
                        {
                    $"Extension ID: {ext.Id}",
                    $"Developer Mode: {developerExtension}",
                    $"Update URL: {updateUrl}",
                    $"Install Location: {installLocation}"
                        }
                    };

                if (developerExtension)
                {
                    anomaly.Evidence.Add(
                        "Extension is loaded in Chrome developer mode (unpacked extension).");
                }

                if (suspiciousUpdateSource)
                {
                    anomaly.Evidence.Add(
                        "Non-standard update source detected.");
                }

                if (suspiciousInstallLocation)
                {
                    anomaly.Evidence.Add(
                        "Extension installed outside Chrome Extensions directory.");
                }

                if (localhostCommunication)
                {
                    anomaly.Evidence.Add(
                        "Localhost communication detected.");
                }

                anomalyIndex[$"extension:{ext.Id}"] = anomaly;
            }
        }

        AnalysisEntry AddOrUpdateAnomaly(
        Dictionary<string, AnalysisEntry> index,
        AnalysisType type,
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

            var created = new AnalysisEntry
            {
                Category = GetCategory(type),

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
        type == AnalysisType.BlacklistedDomain,

                IsSuspiciousRedirect =
        type == AnalysisType.SuspiciousRedirect,

                IsDeletedHistoryIndicator =
        type == AnalysisType.DeletedHistoryIndicator
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
                // =====================================================
                // Browserinterne Cookies ignorieren
                // =====================================================

                if (cookie.Category == CookieCategory.BrowserBackground)
                    continue;

                // =====================================================
                // Cookie-Relation suchen
                // =====================================================

                var relation =
                    cookie.Relations
                        .FirstOrDefault(r =>
                            r.Type == ArtifactRelationType.CookieToHistory);

                // =====================================================
                // Keine Relation vorhanden
                // -> möglicher Hinweis auf gelöschten Verlauf
                // =====================================================

                if (relation == null || relation.History == null)
                {
                    orphanArtifacts.Add(
                        new OrphanArtifact
                        {
                            Time = cookie.Created,
                            Cookie = cookie
                        });

                    continue;
                }

                // =====================================================
                // Plausibilitätsprüfung
                // Ein Cookie sollte normalerweise nicht vor dem
                // ersten dokumentierten Seitenbesuch entstehen.
                // Kleine Zeitabweichungen werden toleriert.
                // =====================================================

                if (relation.History.VisitTime >
                    cookie.Created.AddSeconds(toleranceSeconds))
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
    Dictionary<string, AnalysisEntry> anomalyIndex,
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
    Dictionary<string, AnalysisEntry> anomalyIndex,
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
                new AnalysisEntry
                {
                    Category = GetCategory(AnalysisType.DeletedHistoryIndicator),

                    Type =
                        AnalysisType.DeletedHistoryIndicator,

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
    List<AnalysisEntry> anomalies)
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
    List<AnalysisEntry> anomalies)
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
