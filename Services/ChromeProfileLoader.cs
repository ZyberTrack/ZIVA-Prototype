using System;
using System.Collections.Generic;
using System.Linq;

namespace ZIVA_Prototype.Services
{
    public class ChromeProfileLoader
    {
        private readonly HistoryImportService _history;
        private readonly CookieImportService _cookies;
        private readonly WebDataAutofillImportService _autofill;
        private readonly TimelineStateService _state;
        private readonly ExtensionImportService _extensions;

        public ChromeProfileLoader(
            HistoryImportService history,
            CookieImportService cookies,
            WebDataAutofillImportService autofill,
            ExtensionImportService extensions,
            TimelineStateService state)
        {
            _history = history;
            _cookies = cookies;
            _autofill = autofill;
            _extensions = extensions;
            _state = state;
        }

        public async Task LoadProfileAsync(string profilePath)
        {
            _state.ClearAll();

            Console.WriteLine($"📂 Lade Profil: {profilePath}");

            // 🔥 Dateien dynamisch finden
            var historyPath = FindFile(profilePath, "History");
            var cookiesPath = FindFile(profilePath, "Cookies");
            var webDataPath = FindFile(profilePath, "Web Data");

            // ===== HISTORY =====
            if (historyPath != null)
            {
                Console.WriteLine($"✅ History gefunden: {historyPath}");

                var history = await _history.LoadFromHistoryDatabaseAsync(historyPath);
                Console.WriteLine($"➡️ History Count: {history.Count}");

                _state.SetBrowserEntries(history);
            }
            else
            {
                Console.WriteLine("⚠️ Keine History-Datei gefunden!");
            }

            // ===== COOKIES =====
            if (cookiesPath != null)
            {
                Console.WriteLine($"✅ Cookies gefunden: {cookiesPath}");

                var cookies = await _cookies.LoadFromCookieDatabaseAsync(cookiesPath);
                Console.WriteLine($"➡️ Cookies Count: {cookies.Count}");

                _state.SetBrowserCookies(cookies);
            }
            else
            {
                Console.WriteLine("⚠️ Keine Cookies-Datei gefunden!");
            }

            // ===== AUTOFILL =====
            if (webDataPath != null)
            {
                Console.WriteLine($"✅ Web Data gefunden: {webDataPath}");

                var autofill = await _autofill.LoadAutofillAsync(webDataPath);
                Console.WriteLine($"➡️ Autofill Count: {autofill.Count}");

                _state.SetAutofillEntries(autofill);
            }

            // ===== EXTENSIONS =====
            if (profilePath != null)
            {
                Console.WriteLine($"✅ Web Data gefunden: {profilePath}");

                
                var extList = await _extensions.LoadExtensionsAsync(profilePath);
                Console.WriteLine($"➡️ Extensions Count: {extList.Count}");

                _state.SetExtensions(extList);
            }
            else
            {
                Console.WriteLine("⚠️ Keine Web Data-Datei gefunden!");
            }

            // 🔥 WAL Hinweis (optional, aber stark)
            var walPath = Path.Combine(profilePath, "History-wal");
            if (File.Exists(walPath))
            {
                Console.WriteLine("⚠️ WAL aktiv – Daten könnten unvollständig sein");
            }
        }

        // 🔥 Helfer: findet Datei robust
        private string? FindFile(string folder, string fileName)
        {
            // Direkt im Ordner
            var direct = Path.Combine(folder, fileName);
            if (File.Exists(direct))
                return direct;

            // Falls User falschen Ordner gewählt hat → Unterordner durchsuchen
            var files = Directory.GetFiles(folder, fileName, SearchOption.AllDirectories);

            return files.FirstOrDefault();
        }
    }
}