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
            foreach (var cookie in cookies)
            {
                if (string.IsNullOrWhiteSpace(cookie.Host))
                    continue;

                string cookieDomain =
                    cookie.Host
                        .TrimStart('.')
                        .ToLower();

                var matchingDomain =
                    domains.FirstOrDefault(d =>
                    {
                        string domain =
                            d.Domain
                                .TrimStart('.')
                                .ToLower();

                        return domain == cookieDomain
                            || domain.EndsWith("." + cookieDomain)
                            || cookieDomain.EndsWith("." + domain);
                    });

                if (matchingDomain == null)
                    continue;

                BrowserHistoryEntry? matchingHistory = null;
                int bestScore = int.MinValue;

                foreach (var history in matchingDomain.SubEntries)
                {
                    // Seiten, die nach der Cookie-Erstellung besucht wurden,
                    // können das Cookie nicht erzeugt haben.
                    if (history.VisitTime > cookie.Created)
                        continue;

                    int score = 0;

                    //----------------------------------------------------
                    // 1. Host
                    //----------------------------------------------------

                    if (Uri.TryCreate(history.Url, UriKind.Absolute, out var uri))
                    {
                        if (uri.Host.Equals(cookieDomain,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            score += 100;
                        }

                        //------------------------------------------------
                        // 2. Cookie-Pfad
                        //------------------------------------------------

                        if (!string.IsNullOrWhiteSpace(cookie.Path)
                            && cookie.Path != "/")
                        {
                            if (uri.AbsolutePath.StartsWith(
                                cookie.Path,
                                StringComparison.OrdinalIgnoreCase))
                            {
                                score += 60;
                            }
                        }

                        //------------------------------------------------
                        // 3. Typische Auth-/Login-Seiten
                        //------------------------------------------------

                        string path = uri.AbsolutePath.ToLower();

                        if (path.Contains("/login"))
                            score += 25;

                        if (path.Contains("/signin"))
                            score += 25;

                        if (path.Contains("/auth"))
                            score += 25;

                        if (path.Contains("/account"))
                            score += 15;
                    }

                    //----------------------------------------------------
                    // 4. LastAccessed
                    //----------------------------------------------------

                    if (cookie.LastAccessed > DateTime.MinValue)
                    {
                        score += Math.Max(
                            0,
                            20 - (int)Math.Abs(
                                (cookie.LastAccessed - history.VisitTime)
                                .TotalMinutes));
                    }

                    //----------------------------------------------------
                    // 5. Created
                    //----------------------------------------------------

                    score += Math.Max(
                        0,
                        20 - (int)Math.Abs(
                            (cookie.Created - history.VisitTime)
                            .TotalMinutes));

                    //----------------------------------------------------

                    if (score > bestScore)
                    {
                        bestScore = score;
                        matchingHistory = history;
                    }
                }

                var relation = new ArtifactRelationEntry
                {
                    Type = ArtifactRelationType.CookieToHistory,

                    Cookie = cookie,

                    Domain = matchingDomain,

                    History = matchingHistory,

                    Time = cookie.Created,

                    Confidence = Math.Clamp(bestScore, 0, 100),

                    Reason =
                        $"Cookie correlated using host, path and temporal proximity (Score: {bestScore})."
                };

                cookie.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);

                if (matchingHistory != null)
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
                DomainEntry? matchingDomain = null;

                // =====================================================
                // SEARCH QUERY
                // =====================================================

                if (input.Type == UserInputType.SearchQuery && input.LinkedHistory != null)
                {
                    matchingHistory = input.LinkedHistory;

                    matchingDomain = domains.FirstOrDefault(d =>
                        d.SubEntries.Contains(matchingHistory));

                    if (matchingDomain == null)
                        continue;
                }

                // =====================================================
                // FAVICON
                // =====================================================

                else if (input.Type == UserInputType.Favicon)
                {
                    string faviconUrl =
                        input.Value.Trim().ToLower();

                    matchingDomain =
                        domains
                        .Where(d =>
                            !string.IsNullOrWhiteSpace(d.Url))
                        .Where(d =>
                            d.Url.ToLower().Contains(faviconUrl) ||
                            faviconUrl.Contains(d.Domain.ToLower()))
                        .FirstOrDefault();

                    if (matchingDomain != null)
                    {
                        matchingHistory =
                            matchingDomain.SubEntries
                            .Where(h => h.VisitTime <= input.Time)
                            .OrderByDescending(h => h.VisitTime)
                            .FirstOrDefault();
                    }
                }

                // =====================================================
                // AUTOFILL
                // =====================================================

                else
                {
                    matchingDomain =
                        domains
                        .OrderBy(d =>
                            Math.Abs((input.Time - d.VisitTime).TotalSeconds))
                        .FirstOrDefault(d =>
                            Math.Abs((input.Time - d.VisitTime).TotalMinutes) <= 5);

                    if (matchingDomain != null)
                    {
                        matchingHistory =
                            matchingDomain.SubEntries
                            .Where(h => h.VisitTime <= input.Time)
                            .OrderByDescending(h => h.VisitTime)
                            .FirstOrDefault();
                    }
                }

                if (matchingDomain == null)
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
                    Domain = matchingDomain,
                    History = matchingHistory,
                    Time = input.Time,

                    Confidence = matchingHistory != null ? 100 : 75,

                    Reason = matchingHistory != null
                        ? "User input belongs to originating history entry."
                        : "User input belongs to visited domain."
                };

                input.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);

                if (matchingHistory != null)
                    matchingHistory.Relations.Add(relation);
            }
        }

        // =====================================================
        // EXTENSIONS
        // =====================================================

        private void BuildExtensionRelations(
    List<DomainEntry> domains,
    List<BrowserExtensionEntry> extensions)
        {
            foreach (var ext in extensions)
            {
                var matchingDomain =
                    domains
                    .Where(d =>
                        d.Url.Contains("chrome.google.com/webstore")
                        || d.Url.Contains("chromewebstore.google.com"))
                    .Where(d =>
                        d.VisitTime <= ext.InstallTime)
                    .OrderBy(d =>
                        Math.Abs(
                            (ext.InstallTime - d.VisitTime)
                            .TotalSeconds))
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

                    int score = 0;

                    if (history.Url.Contains(
                            "chrome.google.com/webstore",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        history.Url.Contains(
                            "chromewebstore.google.com",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        score += 100;
                    }

                    score += Math.Max(
                        0,
                        60 - (int)(ext.InstallTime - history.VisitTime).TotalMinutes);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        matchingHistory = history;
                    }
                }

                // ----------------------------------------------------
                // Relation erzeugen
                // ----------------------------------------------------

                var relation =
                    new ArtifactRelationEntry
                    {
                        Type =
                            ArtifactRelationType.ExtensionToWebStore,

                        Extension = ext,

                        Domain = matchingDomain,

                        History = matchingHistory,

                        Time = ext.InstallTime,

                        Confidence =
                            matchingHistory != null
                                ? 98
                                : 90,

                        Reason =
                            matchingHistory != null
                                ? "Extension installation matches closest WebStore visit."
                                : "Extension installation matches WebStore domain."
                    };

                ext.Relations.Add(relation);

                matchingDomain.Relations.Add(relation);

                if (matchingHistory != null)
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