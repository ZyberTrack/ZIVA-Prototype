using System;
using System.Collections.Generic;
using System.Linq;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{
    public class TimelineStateService
    {
        public event Action? OnChange;

        // =========================================================
        // SET ALL
        // =========================================================
        public void SetAll(
            List<BrowserHistoryEntry> history,
            List<BrowserCookieEntry> cookies,
            List<WebDataAutofillEntry> autofill,
            List<BrowserExtensionEntry> extensions,
            List<StorageEntry> storage)
        {
            BrowserEntries.Clear();
            BrowserEntries.AddRange(history.OrderBy(e => e.VisitTime));

            BrowserCookies.Clear();
            BrowserCookies.AddRange(cookies.OrderBy(c => c.LastAccessed));

            AutofillEntries.Clear();
            AutofillEntries.AddRange(autofill.OrderBy(a => a.DateLastUsed));

            Extensions.Clear();
            Extensions.AddRange(extensions.OrderBy(e => e.InstallTime));

            StorageEntries.Clear();
            StorageEntries.AddRange(storage.OrderBy(s => s.Origin));

            OnChange?.Invoke();
        }

        // =========================================================
        // HISTORY
        // =========================================================
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

        // =========================================================
        // COOKIES
        // =========================================================
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

        // =========================================================
        // AUTOFILL
        // =========================================================
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

        // =========================================================
        // EXTENSIONS 🔥 (NEU)
        // =========================================================
        public List<BrowserExtensionEntry> Extensions { get; } = new();

        public void SetExtensions(List<BrowserExtensionEntry> entries)
        {
            Extensions.Clear();
            Extensions.AddRange(entries.OrderBy(e => e.InstallTime));

            OnChange?.Invoke();
        }

        public void AddExtensions(List<BrowserExtensionEntry> entries)
        {
            Extensions.AddRange(entries);
            Extensions.Sort((a, b) => a.InstallTime.CompareTo(b.InstallTime));

            OnChange?.Invoke();
        }

        // =========================================================
        // STORAGE 🔥 (NEU)
        // =========================================================
        public List<StorageEntry> StorageEntries { get; } = new();

        public void SetStorageEntries(List<StorageEntry> entries)
        {
            StorageEntries.Clear();
            StorageEntries.AddRange(entries.OrderBy(e => e.Origin)); // später evtl. LastModified

            OnChange?.Invoke();
        }

        public void AddStorageEntries(List<StorageEntry> entries)
        {
            StorageEntries.AddRange(entries);

            StorageEntries.Sort((a, b) =>
                string.Compare(a.Origin, b.Origin, StringComparison.Ordinal));

            OnChange?.Invoke();
        }

        // =========================================================
        // CLEAR ALL (FIX!)
        // =========================================================
        public void ClearAll()
        {
            BrowserEntries.Clear();
            BrowserCookies.Clear();
            AutofillEntries.Clear();
            Extensions.Clear();
            StorageEntries.Clear();

            OnChange?.Invoke();
        }
    }
}