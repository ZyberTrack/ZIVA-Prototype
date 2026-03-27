using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{

    public class UserInputAggregatorService
    {
        public List<UserInputEntry> Build(
            List<BrowserHistoryEntry> history,
            List<WebDataAutofillEntry> autofill)
        {
            var list = new List<UserInputEntry>();

            // 🔍 SEARCH QUERIES aus History
            foreach (var h in history)
            {
                var query = ExtractSearchQuery(h.Url);
                if (!string.IsNullOrWhiteSpace(query))
                {
                    list.Add(new UserInputEntry
                    {
                        Time = h.VisitTime,
                        Value = query,
                        Type = UserInputType.SearchQuery
                    });
                }
            }

            // ✍️ AUTOFILL
            foreach (var a in autofill)
            {
                if (!string.IsNullOrWhiteSpace(a.Value))
                {
                    list.Add(new UserInputEntry
                    {
                        Time = a.DateCreated,
                        Value = a.Value,
                        Type = UserInputType.Autofill
                    });
                }
            }

            return list.OrderBy(x => x.Time).ToList();
        }

        private string? ExtractSearchQuery(string url)
        {
            try
            {
                if (!url.Contains("q=")) return null;

                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);

                return query["q"];
            }
            catch
            {
                return null;
            }
        }
    }
}
