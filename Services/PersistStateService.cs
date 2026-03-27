using System;
using System.Collections.Generic;
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

            var data = new PersistedState
            {
                BrowserEntries = state.BrowserEntries,
                BrowserCookies = state.BrowserCookies,
                AutofillEntries = state.AutofillEntries
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task LoadPersistedStateAsync(TimelineStateService state)
        {
            var filePath = GetStateFilePath();

            if (!File.Exists(filePath))
            {
                var empty = new PersistedState();
                var jsonEmpty = JsonSerializer.Serialize(empty, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, jsonEmpty);
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);

            var persisted = JsonSerializer.Deserialize<PersistedState>(jsonContent)
                            ?? new PersistedState();

            // EINMALIGES SETZEN
            state.SetAll(
                persisted.BrowserEntries ?? new List<BrowserHistoryEntry>(),
                persisted.BrowserCookies ?? new List<BrowserCookieEntry>(),
                persisted.AutofillEntries ?? new List<WebDataAutofillEntry>()
            );
        }

        public class PersistedState
        {
            public List<BrowserHistoryEntry>? BrowserEntries { get; set; } = new();
            public List<BrowserCookieEntry>? BrowserCookies { get; set; } = new();
            public List<WebDataAutofillEntry>? AutofillEntries { get; set; } = new();
        }
    }
}