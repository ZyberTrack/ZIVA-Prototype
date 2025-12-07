using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{

    public class PersistStateService
    {
        private const string StateFileName = "browserState.json";

        private static string GetStateFilePath()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var folder = Path.Combine(documents, "ZIVA_Prototype");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(folder, StateFileName);
        }

        public async Task PersistStateAsync(TimelineStateService state)
        {
            var filePath = GetStateFilePath();
            var data = new
            {
                BrowserEntries = state.BrowserEntries,
                BrowserCookies = state.BrowserCookies,
                AutofillEntries = state.AutofillEntries
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task LoadPersistedStateAsync(TimelineStateService state)
        {
            var filePath = GetStateFilePath();
            if (!File.Exists(filePath))
            {
                // Datei erstellen, falls nicht vorhanden
                var emptyData = new
                {
                    BrowserEntries = new List<BrowserHistoryEntry>(),
                    BrowserCookies = new List<BrowserCookieEntry>(),
                    AutofillEntries = new List<WebDataAutofillEntry>()
                };
                var json = JsonSerializer.Serialize(emptyData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var persisted = JsonSerializer.Deserialize<PersistedState>(jsonContent);
            if (persisted != null)
            {
                state.SetBrowserEntries(persisted.BrowserEntries ?? new List<BrowserHistoryEntry>());
                state.SetBrowserCookies(persisted.BrowserCookies ?? new List<BrowserCookieEntry>());
                state.SetAutofillEntries(persisted.AutofillEntries ?? new List<WebDataAutofillEntry>());
            }
        }


        public class PersistedState
        {
            public List<BrowserHistoryEntry>? BrowserEntries { get; set; }
            public List<BrowserCookieEntry>? BrowserCookies { get; set; }
            public List<WebDataAutofillEntry>? AutofillEntries { get; set; }
        }

    }
}
