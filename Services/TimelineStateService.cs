using System;
using System.Collections.Generic;
using System.Linq;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{
    public class TimelineStateService
    {
        public event Action? OnChange; //Event hinzufügen, um Änderungen zu signalisieren

        public void SetAll(
    List<BrowserHistoryEntry> history,
    List<BrowserCookieEntry> cookies,
    List<WebDataAutofillEntry> autofill)
        {
            BrowserEntries.Clear();
            BrowserEntries.AddRange(history.OrderBy(e => e.VisitTime));

            BrowserCookies.Clear();
            BrowserCookies.AddRange(cookies.OrderBy(c => c.LastAccessed));

            AutofillEntries.Clear();
            AutofillEntries.AddRange(autofill.OrderBy(a => a.DateLastUsed));

            OnChange?.Invoke(); // EIN sauberer Trigger
        }


        // === HISTORY ===
        public List<BrowserHistoryEntry> BrowserEntries { get; } = new();

        public void SetBrowserEntries(List<BrowserHistoryEntry> entries)
        {
            BrowserEntries.Clear();
            BrowserEntries.AddRange(entries.OrderBy(e => e.VisitTime));

            OnChange?.Invoke();
        }

        public void AddBrowserEntries(List<BrowserHistoryEntry> entries)
        {
            BrowserEntries.AddRange(entries);
            BrowserEntries.Sort((a, b) => a.VisitTime.CompareTo(b.VisitTime));

            OnChange?.Invoke();
        }


        // === COOKIES ===
        public List<BrowserCookieEntry> BrowserCookies { get; } = new();

        public void SetBrowserCookies(List<BrowserCookieEntry> cookies)
        {
            BrowserCookies.Clear();
            BrowserCookies.AddRange(cookies.OrderBy(c => c.LastAccessed));

            OnChange?.Invoke();
        }

        public void AddBrowserCookies(List<BrowserCookieEntry> cookies)
        {
            BrowserCookies.AddRange(cookies);
            BrowserCookies.Sort((a, b) => a.LastAccessed.CompareTo(b.LastAccessed));

            OnChange?.Invoke();
        }


        // === WebDataAutofill ===
        public List<WebDataAutofillEntry> AutofillEntries { get; } = new();

        public void SetAutofillEntries(List<WebDataAutofillEntry> entries)
        {
            AutofillEntries.Clear();
            AutofillEntries.AddRange(entries.OrderBy(e => e.DateLastUsed));

            OnChange?.Invoke();
        }

        public void AddAutofillEntries(List<WebDataAutofillEntry> entries)
        {
            AutofillEntries.AddRange(entries);
            AutofillEntries.Sort((a, b) => a.DateLastUsed.CompareTo(b.DateLastUsed));

            OnChange?.Invoke();
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


