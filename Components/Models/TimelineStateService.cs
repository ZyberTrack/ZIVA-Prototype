using System;
using System.Collections.Generic;
using System.Linq;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Components.Models
{
    public class TimelineStateService
    {
        // === HISTORY ===
        public List<BrowserHistoryEntry> BrowserEntries { get; } = new();

        public void SetBrowserEntries(List<BrowserHistoryEntry> entries)
        {
            BrowserEntries.Clear();
            BrowserEntries.AddRange(entries.OrderBy(e => e.VisitTime));
        }


        // === COOKIES ===
        public List<BrowserCookieEntry> BrowserCookies { get; } = new();

        public void SetBrowserCookies(List<BrowserCookieEntry> cookies)
        {
            BrowserCookies.Clear();
            BrowserCookies.AddRange(cookies.OrderBy(c => c.LastAccessed));
        }
    }
}

