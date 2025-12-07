using System;
using System.Collections.Generic;
using System.Linq;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
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

        public void AddBrowserEntries(List<BrowserHistoryEntry> entries)
        {
            BrowserEntries.AddRange(entries);
            BrowserEntries.Sort((a, b) => a.VisitTime.CompareTo(b.VisitTime));
        }


        // === COOKIES ===
        public List<BrowserCookieEntry> BrowserCookies { get; } = new();

        public void SetBrowserCookies(List<BrowserCookieEntry> cookies)
        {
            BrowserCookies.Clear();
            BrowserCookies.AddRange(cookies.OrderBy(c => c.LastAccessed));
        }

        public void AddBrowserCookies(List<BrowserCookieEntry> cookies)
        {
            BrowserCookies.AddRange(cookies);
            BrowserCookies.Sort((a, b) => a.LastAccessed.CompareTo(b.LastAccessed));
        }


        // === WebDataAutofill ===
        public List<WebDataAutofillEntry> AutofillEntries { get; } = new();

        public void SetAutofillEntries(List<WebDataAutofillEntry> entries)
        {
            AutofillEntries.Clear();
            AutofillEntries.AddRange(entries.OrderBy(e => e.DateLastUsed));
        }

        public void AddAutofillEntries(List<WebDataAutofillEntry> entries)
        {
            AutofillEntries.AddRange(entries);
            AutofillEntries.Sort((a, b) => a.DateLastUsed.CompareTo(b.DateLastUsed));
        }

        // === Alles löschen ===
        public void ClearAll()
        {
            BrowserEntries.Clear();
            BrowserCookies.Clear();
            AutofillEntries.Clear();
        }
    }
}


