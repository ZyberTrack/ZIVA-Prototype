using Microsoft.WindowsAppSDK.Runtime.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class ArtifactRelationService
    {
        // =====================================================
        // MAIN ENTRY
        // =====================================================

        public void BuildRelations(
    List<DomainEntry> domains,
    List<BrowserCookieEntry> cookies,
    List<UserInputEntry> inputs,
    List<BrowserExtensionEntry> extensions,
    List<StorageEntry> storage)
        {
            ClearRelations(
                domains,
                cookies,
                inputs,
                extensions,
                storage);

            BuildCookieRelations(
                domains,
                cookies);

            BuildInputRelations(
                domains,
                inputs);

            BuildExtensionRelations(
                domains,
                extensions);

            BuildStorageRelations(
                domains,
                storage);

            BuildHistoryRelations(
                domains);
        }

        // =====================================================
        // RESET
        // =====================================================

        private void ClearRelations(
            List<DomainEntry> domains,
            List<BrowserCookieEntry> cookies,
            List<UserInputEntry> inputs,
            List<BrowserExtensionEntry> extensions,
            List<StorageEntry> storage)
        {
            foreach (var d in domains)
            {
                d.Relations.Clear();

                foreach (var h in d.SubEntries)
                    h.Relations.Clear();
            }

            foreach (var c in cookies)
                c.Relations.Clear();

            foreach (var i in inputs)
                i.Relations.Clear();

            foreach (var e in extensions)
                e.Relations.Clear();

            foreach (var s in storage)
                s.Relations.Clear();
        }

        // =====================================================
        // COOKIES
        // =====================================================

        private void BuildCookieRelations(
            List<DomainEntry> domains,
            List<BrowserCookieEntry> cookies)
        {
            const double MaxRelationHours = 24;

            foreach (var cookie in cookies)
            {
                if (string.IsNullOrWhiteSpace(cookie.Host))
                    continue;

                string cookieDomain =
                    cookie.Host
                        .TrimStart('.')
                        .ToLower();

                var matchingHistory =
                    domains
                        .SelectMany(d => d.SubEntries)
                        .Where(history =>
                        {
                            if (history.VisitTime > cookie.Created)
                                return false;

                            double hours =
                                (cookie.Created - history.VisitTime).TotalHours;

                            if (hours > MaxRelationHours)
                                return false;

                            if (!Uri.TryCreate(
                                    history.Url,
                                    UriKind.Absolute,
                                    out var uri))
                                return false;

                            string historyHost =
                                uri.Host
                                   .TrimStart('.')
                                   .ToLower();

                            return historyHost == cookieDomain
                                || historyHost.EndsWith("." + cookieDomain)
                                || cookieDomain.EndsWith("." + historyHost);
                        })
                        .OrderByDescending(h => h.VisitTime)
                        .FirstOrDefault();

                if (matchingHistory == null)
                    continue;

                //----------------------------------------------------
                // Confidence anhand Zeitdifferenz
                //----------------------------------------------------

                double minutes =
                    (cookie.Created - matchingHistory.VisitTime).TotalMinutes;

                int confidence =
                    minutes <= 1 ? 100 :
                    minutes <= 5 ? 98 :
                    minutes <= 30 ? 95 :
                    minutes <= 120 ? 90 :
                    minutes <= 360 ? 85 :
                    minutes <= 720 ? 80 :
                    70;

                //----------------------------------------------------
                // Relation erzeugen
                //----------------------------------------------------

                var relation = new ArtifactRelationEntry
                {
                    Type = ArtifactRelationType.CookieToHistory,

                    Cookie = cookie,

                    History = matchingHistory,

                    Time = cookie.Created,

                    Confidence = confidence,

                    Reason =
                        $"Cookie matched nearest previous visit to '{cookieDomain}' ({minutes:F0} minutes earlier)."
                };

                cookie.Relations.Add(relation);
                matchingHistory.Relations.Add(relation);
            }
        }

        // =====================================================
        // USER INPUT
        // =====================================================

        private void BuildInputRelations(
     List<DomainEntry> domains,
     List<UserInputEntry> inputs)
        {
            foreach (var input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input.Value))
                    continue;

                BrowserHistoryEntry? matchingHistory = null;

                // =====================================================
                // SEARCH QUERY
                // =====================================================

                if (input.Type == UserInputType.SearchQuery && input.LinkedHistory != null)
                {
                    var relation1 = new ArtifactRelationEntry
                    {
                        Type = ArtifactRelationType.UserInputToHistory,

                        UserInput = input,

                        History = input.LinkedHistory,

                        Time = input.Time,

                        Confidence = 100,

                        Reason = "Search query extracted directly from browser history."
                    };

                    input.Relations.Add(relation1);

                    continue;
                }

                // =====================================================
                // FAVICON
                // =====================================================

                else if (input.Type == UserInputType.Favicon)
                {
                    if (input.LinkedHistory == null)
                        continue;

                    var relation2 = new ArtifactRelationEntry
                    {
                        Type = ArtifactRelationType.FaviconToHistory,

                        UserInput = input,

                        History = input.LinkedHistory,

                        Time = input.Time,

                        Confidence = 100,

                        Reason = "Relation imported from source."
                    };

                    input.Relations.Add(relation2);

                    continue;
                }

                // =====================================================
                // AUTOFILL
                // =====================================================

                else
                {
                    // Falls bereits beim Import gesetzt
                    if (input.LinkedHistory != null)
                    {
                        matchingHistory = input.LinkedHistory;
                    }
                    else
                    {
                        matchingHistory =
                            domains
                                .SelectMany(d => d.SubEntries)
                                .Where(h =>
                                    Math.Abs((h.VisitTime - input.Time).TotalSeconds) <= 10)
                                .OrderBy(h =>
                                    Math.Abs((h.VisitTime - input.Time).TotalSeconds))
                                .FirstOrDefault();
                    }
                }

                if (matchingHistory == null)
                    continue;

                var relation = new ArtifactRelationEntry
                {
                    Type = input.Type switch
                    {
                        UserInputType.Autofill =>
                            ArtifactRelationType.AutofillToHistory,

                        UserInputType.Favicon =>
                            ArtifactRelationType.FaviconToHistory,

                        _ =>
                            ArtifactRelationType.UserInputToHistory
                    },

                    UserInput = input,

                    History = matchingHistory,

                    // Domain nur noch für Rendering
                    Domain = matchingHistory.ParentDomain,

                    Time = input.Time,

                    Confidence = 100,

                    Reason = input.LinkedHistory != null
                        ? "Relation imported directly from source."
                        : "Relation inferred from history."
                };

                input.Relations.Add(relation);
                matchingHistory.Relations.Add(relation);

                if (matchingHistory.ParentDomain != null)
                {
                    matchingHistory.ParentDomain.Relations.Add(relation);
                }
            }
        }


        // =====================================================
        // EXTENSIONS
        // =====================================================

        private void BuildExtensionRelations(
            List<DomainEntry> domains,
            List<BrowserExtensionEntry> extensions)
        {
            const double MaxRelationHours = 24;

            foreach (var ext in extensions)
            {
                var matchingDomain =
                    domains
                        .Where(d =>
                            d.Url.Contains("chrome.google.com/webstore",
                                StringComparison.OrdinalIgnoreCase) ||
                            d.Url.Contains("chromewebstore.google.com",
                                StringComparison.OrdinalIgnoreCase))
                        .Where(d =>
                            d.VisitTime <= ext.InstallTime)
                        .Where(d =>
                            (ext.InstallTime - d.VisitTime).TotalHours <= MaxRelationHours)
                        .OrderBy(d =>
                            ext.InstallTime - d.VisitTime)
                        .FirstOrDefault();

                if (matchingDomain == null)
                    continue;

                // ----------------------------------------------------
                // Passendsten History-Eintrag innerhalb der Domain suchen
                // ----------------------------------------------------

                BrowserHistoryEntry? matchingHistory = null;
                int bestScore = -1;

                foreach (var history in matchingDomain.SubEntries)
                {
                    if (history.VisitTime > ext.InstallTime)
                        continue;

                    if ((ext.InstallTime - history.VisitTime).TotalHours > MaxRelationHours)
                        continue;

                    int score = 0;

                    // Direkte WebStore-Seite bevorzugen
                    if (history.Url.Contains(
                            "chrome.google.com/webstore",
                            StringComparison.OrdinalIgnoreCase) ||
                        history.Url.Contains(
                            "chromewebstore.google.com",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        score += 100;
                    }

                    // Je näher zeitlich, desto besser
                    score += Math.Max(
                        0,
                        60 - (int)(ext.InstallTime - history.VisitTime).TotalMinutes);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        matchingHistory = history;
                    }
                }

                // Ohne passenden History-Eintrag keine belastbare Relation
                if (matchingHistory == null)
                    continue;

                // ----------------------------------------------------
                // Confidence anhand Zeitdifferenz bestimmen
                // ----------------------------------------------------

                double minutes =
                    (ext.InstallTime - matchingHistory.VisitTime).TotalMinutes;

                int confidence =
                    minutes <= 5 ? 100 :
                    minutes <= 30 ? 98 :
                    minutes <= 120 ? 95 :
                    minutes <= 720 ? 90 :      // 12h
                    80;                        // bis 24h

                // ----------------------------------------------------
                // Relation erzeugen
                // ----------------------------------------------------

                var relation = new ArtifactRelationEntry
                {
                    Type = ArtifactRelationType.ExtensionToWebStore,

                    Extension = ext,

                    Domain = matchingDomain,

                    History = matchingHistory,

                    Time = ext.InstallTime,

                    Confidence = confidence,

                    Reason =
                        $"Extension installation matched Chrome Web Store visit ({minutes:F0} minutes earlier)."
                };

                ext.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);
                matchingHistory.Relations.Add(relation);
            }
        }

        // =====================================================
        // STORAGE
        // =====================================================

        private void BuildStorageRelations(
    List<DomainEntry> domains,
    List<StorageEntry> storage)
        {
            foreach (var s in storage)
            {
                if (string.IsNullOrWhiteSpace(s.Origin))
                    continue;

                Uri? uri;

                try
                {
                    uri = new Uri(s.Origin);
                }
                catch
                {
                    continue;
                }

                string host =
                    uri.Host
                       .TrimStart('.')
                       .ToLower();

                // -------------------------------
                // Domain bestimmen
                // -------------------------------

                var matchingDomain =
                    domains
                    .Where(d =>
                        d.Domain
                         .TrimStart('.')
                         .ToLower() == host)
                    .FirstOrDefault();

                if (matchingDomain == null)
                    continue;

                // -------------------------------
                // Passendste History bestimmen
                // -------------------------------

                BrowserHistoryEntry? matchingHistory = null;

                int bestScore = -1;

                foreach (var history in matchingDomain.SubEntries)
                {
                    if (history.VisitTime > s.Time)
                        continue;

                    int score = 0;

                    // gleicher Host
                    if (Uri.TryCreate(history.Url, UriKind.Absolute, out var historyUri))
                    {
                        if (historyUri.Host.Equals(
                                host,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            score += 100;

                            // gleicher URL-Prefix?
                            if (history.Url.StartsWith(
                                    s.Origin,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                score += s.Origin.Length;
                            }
                        }
                    }

                    // zeitliche Nähe (max. 60 Punkte)
                    score += Math.Max(
                        0,
                        60 - (int)(s.Time - history.VisitTime).TotalMinutes);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        matchingHistory = history;
                    }
                }

                // -------------------------------
                // Relation erzeugen
                // -------------------------------

                var relation =
                    new ArtifactRelationEntry
                    {
                        Type = ArtifactRelationType.StorageToHistory,

                        Storage = s,

                        Domain = matchingDomain,

                        History = matchingHistory,

                        Time = s.Time,

                        Confidence =
                            matchingHistory != null
                                ? 95
                                : 80,

                        Reason =
                            matchingHistory != null
                            ? "Storage belongs to most recent visit of origin."
                            : "Storage origin matches visited domain."
                    };

                s.Relations.Add(relation);

                matchingDomain.Relations.Add(relation);

                if (matchingHistory != null)
                    matchingHistory.Relations.Add(relation);
            }
        }

        // =====================================================
        // HISTORY REFERRER
        // =====================================================

        private void BuildHistoryRelations(
            List<DomainEntry> domains)
        {
            foreach (var targetDomain in domains)
            {
                foreach (var history in targetDomain.SubEntries)
                {
                    if (string.IsNullOrWhiteSpace(history.ReferrerUrl))
                        continue;

                    Uri? refUri;

                    try
                    {
                        refUri = new Uri(history.ReferrerUrl);
                    }
                    catch
                    {
                        continue;
                    }

                    string sourceDomain =
                        refUri.Host
                            .TrimStart('.')
                            .ToLower();

                    BrowserHistoryEntry? sourceHistory = null;
                    DomainEntry? sourceDomainEntry = null;

                    foreach (var domain in domains)
                    {
                        sourceHistory = domain.SubEntries.FirstOrDefault(h =>
                            string.Equals(
                                h.Url,
                                history.ReferrerUrl,
                                StringComparison.OrdinalIgnoreCase));

                        if (sourceHistory != null)
                        {
                            sourceDomainEntry = domain;
                            break;
                        }
                    }

                    if (sourceHistory == null || sourceDomainEntry == null)
                        continue;

                    var relation =
                            new ArtifactRelationEntry
                            {
                                Type =
                                    ArtifactRelationType
                                        .HistoryReferrer,

                                Domain =
                                    sourceDomainEntry,

                                History =
                                    sourceHistory,

                                Time =
                                    history.VisitTime,

                                Confidence =
                                    95,

                                Reason =
                                    "Visited via external referrer"
                            };

                    history.Relations.Add(relation);
                    //sourceHistory.Relations.Add(relation);
                    //sourceDomainEntry.Relations.Add(relation);
                }
            }
        }

    }
}