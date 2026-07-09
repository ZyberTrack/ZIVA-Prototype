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
            List<FaviconEntry> favicons,
            List<UserInputEntry>? existingInputs = null)
        {
            var list =
                new List<UserInputEntry>();

            var existingLookup =
                existingInputs?
                .ToDictionary(
                    x => $"{x.Type}|{x.Time.Ticks}|{x.Value}",
                    x => x.Id)
                ?? new Dictionary<string, Guid>();

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

                var item = new UserInputEntry
                {
                    Time = normalized.Value,
                    Value = query,
                    Type = UserInputType.SearchQuery,

                    LinkedHistory = h
                };

                string key =
                    $"{item.Type}|{item.Time.Ticks}|{item.Value}";

                if (existingLookup.TryGetValue(key, out var id))
                {
                    item.Id = id;
                }

                list.Add(item);
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

                var item = new UserInputEntry
                {
                    Time = normalized.Value,
                    Value = a.Value,
                    Type = UserInputType.Autofill
                };

                string key =
                    $"{item.Type}|{item.Time.Ticks}|{item.Value}";

                if (existingLookup.TryGetValue(key, out var id))
                {
                    item.Id = id;
                }

                list.Add(item);
            }

            // =====================================================
            // FAVICONS
            // =====================================================

            foreach (var favicon in favicons)
            {
                var normalized =
                    NormalizeTimestamp(favicon.Time);

                if (normalized == null)
                    continue;

                BrowserHistoryEntry? linkedHistory =
                    history.FirstOrDefault(h =>
                        string.Equals(
                            h.Url.TrimEnd('/'),
                            favicon.PageUrl.TrimEnd('/'),
                            StringComparison.OrdinalIgnoreCase));

                var item = new UserInputEntry
                {
                    Time = normalized.Value,
                    Value = favicon.PageUrl,
                    Type = UserInputType.Favicon,

                    LinkedHistory = linkedHistory
                };


                string key =
                    $"{item.Type}|{item.Time.Ticks}|{item.Value}";

                if (existingLookup.TryGetValue(key, out var id))
                {
                    item.Id = id;
                }

                list.Add(item);
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