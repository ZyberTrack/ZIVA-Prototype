// INFOR FÜR BACHELORARBEIT:
// User Input Types - Search Queries (from Browser History)
// und Autofill (from Web Data) und Favicons.

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import
{
    public class UserInputAggregatorService
    {
        public List<UserInputEntry> Build(
            List<BrowserHistoryEntry> history,
            List<WebDataAutofillEntry> autofill,
            List<FaviconEntry> favicons)
        {
            var list =
                new List<UserInputEntry>();

            // =====================================================
            // SEARCH QUERIES
            // =====================================================

            foreach (var h in history)
            {
                var query =
                    ExtractSearchQuery(
                        h.Url);

                if (string.IsNullOrWhiteSpace(query))
                    continue;

                var normalized =
                    NormalizeTimestamp(
                        h.VisitTime);

                if (normalized == null)
                    continue;

                list.Add(
                    new UserInputEntry
                    {
                        Time =
                            normalized.Value,

                        Value =
                            query,

                        Type =
                            UserInputType
                                .SearchQuery
                    });
            }

            // =====================================================
            // AUTOFILL
            // =====================================================

            foreach (var a in autofill)
            {
                if (string.IsNullOrWhiteSpace(
                    a.Value))
                {
                    continue;
                }

                var normalized =
                    NormalizeTimestamp(
                        a.DateCreated);

                if (normalized == null)
                    continue;

                list.Add(
                    new UserInputEntry
                    {
                        Time =
                            normalized.Value,

                        Value =
                            a.Value,

                        Type =
                            UserInputType
                                .Autofill
                    });
            }

            // =====================================================
            // FAVICONS
            // =====================================================

            foreach (var favicon in favicons)
            {
                var normalized =
                    NormalizeTimestamp(
                        favicon.Time);

                if (normalized == null)
                    continue;

                list.Add(
                    new UserInputEntry
                    {
                        Time =
                            normalized.Value,

                        Value =
                            favicon.PageUrl,

                        Type =
                            UserInputType
                                .Favicon
                    });
            }

            // =====================================================
            // ORDERING
            // =====================================================

            return list
                .OrderByDescending(x => x.Time)
                .ToList();
        }

        private string? ExtractSearchQuery(
            string url)
        {
            try
            {
                if (!url.Contains("q="))
                    return null;

                var uri =
                    new Uri(url);

                var query =
                    HttpUtility.ParseQueryString(
                        uri.Query);

                return query["q"];
            }
            catch
            {
                return null;
            }
        }

        private DateTime? NormalizeTimestamp(
    DateTime timestamp)
        {
            // =====================================================
            // INVALID / DEFAULT / CHROMIUM ZERO
            // =====================================================

            if (timestamp == default)
                return null;

            if (timestamp.Year < 2000)
                return null;

            if (timestamp.Year > 2100)
                return null;

            return timestamp;
        }
    }
}