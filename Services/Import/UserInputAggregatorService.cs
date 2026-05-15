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

                if (!string.IsNullOrWhiteSpace(
                    query))
                {
                    list.Add(
                        new UserInputEntry
                        {
                            Time =
                                h.VisitTime,

                            Value =
                                query,

                            Type =
                                UserInputType
                                    .SearchQuery
                        });
                }
            }

            // =====================================================
            // AUTOFILL
            // =====================================================

            foreach (var a in autofill)
            {
                if (!string.IsNullOrWhiteSpace(
                    a.Value))
                {
                    list.Add(
                        new UserInputEntry
                        {
                            Time =
                                a.DateCreated,

                            Value =
                                a.Value,

                            Type =
                                UserInputType
                                    .Autofill
                        });
                }
            }

            // =====================================================
            // FAVICONS
            // =====================================================

            foreach (var favicon in favicons)
            {
                list.Add(
                    new UserInputEntry
                    {
                        Time =
                            favicon.Time,

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
    }
}