using System;
using System.Collections.Generic;
using System.Linq;
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
                d.Relations.Clear();

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
                    domains
                    .Where(d =>
                    {
                        var domain =
                            d.Domain
                             .TrimStart('.')
                             .ToLower();

                        return
                            domain.Contains(cookieDomain)
                            ||
                            cookieDomain.Contains(domain);
                    })
                    .OrderBy(d =>
                        Math.Abs(
                            (cookie.Created - d.VisitTime)
                            .TotalSeconds))
                    .FirstOrDefault();

                if (matchingDomain == null)
                    continue;

                // 10 Minuten Window
                if (Math.Abs(
                    (cookie.Created -
                     matchingDomain.VisitTime)
                    .TotalMinutes) > 10)
                    continue;

                var relation =
                    new ArtifactRelationEntry
                    {
                        Type =
                            ArtifactRelationType
                                .CookieToHistory,

                        Cookie =
                            cookie,

                        Domain =
                            matchingDomain,

                        Time =
                            cookie.Created,

                        Confidence =
                            90,

                        Reason =
                            "Cookie domain matches history domain"
                    };

                cookie.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);
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

                DomainEntry? matchingDomain = null;

                // =====================================================
                // FAVICON MATCHING
                // =====================================================

                if (input.Type == UserInputType.Favicon)
                {
                    string faviconUrl =
                        input.Value?
                        .Trim()
                        .ToLower() ?? "";

                    matchingDomain =
                        domains
                        .Where(d =>
                            !string.IsNullOrWhiteSpace(d.Url))
                        .Where(d =>
                            d.Url.ToLower().Contains(faviconUrl)
                            ||
                            faviconUrl.Contains(
                                d.Domain.ToLower()))
                        .OrderBy(d =>
                            Math.Abs(
                                (input.Time - d.VisitTime)
                                .TotalSeconds))
                        .FirstOrDefault();

                    // fallback auf Zeit
                    matchingDomain ??=
                        domains
                        .OrderBy(d =>
                            Math.Abs(
                                (input.Time - d.VisitTime)
                                .TotalSeconds))
                        .FirstOrDefault(d =>
                            Math.Abs(
                                (input.Time - d.VisitTime)
                                .TotalMinutes) <= 5);
                }
                else
                {
                    matchingDomain =
                        domains
                        .OrderBy(d =>
                            Math.Abs(
                                (input.Time - d.VisitTime)
                                .TotalSeconds))
                        .FirstOrDefault(d =>
                            Math.Abs(
                                (input.Time - d.VisitTime)
                                .TotalMinutes) <= 5);
                }

                if (matchingDomain == null)
                    continue;

                var relation =
                    new ArtifactRelationEntry
                    {
                        Type =
                            input.Type switch
                            {
                                UserInputType.Autofill =>
                                    ArtifactRelationType
                                        .AutofillToHistory,

                                UserInputType.Favicon =>
                                    ArtifactRelationType
                                        .FaviconToHistory,

                                _ =>
                                    ArtifactRelationType
                                        .UserInputToHistory
                            },

                        UserInput = input,

                        Domain = matchingDomain,

                        Time = input.Time,

                        Confidence = 70,

                        Reason =
                            "User input timestamp correlates with history"
                    };

                input.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);
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
                            ||
                            d.Url.Contains("chromewebstore.google.com"))
                        .Where(d =>
                            d.VisitTime <= ext.InstallTime)
                        .OrderBy(d =>
                            Math.Abs(
                                (ext.InstallTime - d.VisitTime)
                                .TotalSeconds))
                        .FirstOrDefault();

                if (matchingDomain == null)
                    continue;

                if (matchingDomain.VisitTime >
                    ext.InstallTime)
                    continue;

                if ((ext.InstallTime -
                     matchingDomain.VisitTime)
                    .TotalMinutes > 10)
                    continue;

                var relation =
                    new ArtifactRelationEntry
                    {
                        Type =
                            ArtifactRelationType
                                .ExtensionToWebStore,

                        Extension = ext,

                        Domain = matchingDomain,

                        Time = ext.InstallTime,

                        Confidence = 95,

                        Reason =
                            "Extension install preceded by WebStore visit"
                    };

                ext.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);
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

                var matchingDomain =
                    domains
                    .Where(d =>
                        s.Origin.Contains(d.Domain))
                    .OrderBy(d =>
                        Math.Abs(
                            (s.Time - d.VisitTime)
                            .TotalSeconds))
                    .FirstOrDefault();

                if (matchingDomain == null)
                    continue;

                var relation =
                    new ArtifactRelationEntry
                    {
                        Type =
                            ArtifactRelationType
                                .StorageToHistory,

                        Storage = s,

                        Domain = matchingDomain,

                        Time = s.Time,

                        Confidence = 85,

                        Reason =
                            "Storage origin matches history domain"
                    };

                s.Relations.Add(relation);
                matchingDomain.Relations.Add(relation);
            }
        }
    }
}