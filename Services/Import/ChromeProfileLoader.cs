using System;
using System.Collections.Generic;
using System.Linq;
using ZIVA_Prototype.Services.Timeline;

namespace ZIVA_Prototype.Services.Import
{
    public class ChromeProfileLoader
    {
        private readonly HistoryImportService _history;
        private readonly CookieImportService _cookies;
        private readonly WebDataAutofillImportService _autofill;
        private readonly TimelineStateService _state;
        private readonly ExtensionImportService _extensions;
        private readonly StorageImportService _storage;
        private readonly StorageArtifactScanner _artifactScanner;
        private readonly FaviconsImportService _favicons;

        public ChromeProfileLoader(
            HistoryImportService history,
            CookieImportService cookies,
            WebDataAutofillImportService autofill,
            ExtensionImportService extensions,
            StorageImportService storage,
            TimelineStateService state,
            StorageArtifactScanner artifactScanner,
            FaviconsImportService favicons)
        {
            _history = history;
            _cookies = cookies;
            _autofill = autofill;
            _extensions = extensions;
            _storage = storage;
            _state = state;
            _artifactScanner = artifactScanner;
            _favicons = favicons;
        }

        public async Task LoadProfileAsync(string profilePath)
        {
            _state.ClearAll();

            Console.WriteLine($"📂 Lade Profil: {profilePath}");

            // Dateien dynamisch finden
            var historyPath = FindFile(profilePath, "History");
            var cookiesPath = FindFile(profilePath, "Cookies");
            var webDataPath = FindFile(profilePath, "Web Data");
            var faviconsPath = FindFile(profilePath, "Favicons");

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
                Console.WriteLine($"✅ Extensions werden geladen aus: {profilePath}");

                
                var extList = await _extensions.LoadExtensionsAsync(profilePath);
                Console.WriteLine($"➡️ Extensions Count: {extList.Count}");

                _state.SetExtensions(extList);
            }
            else
            {
                Console.WriteLine("⚠️ Keine Web Data-Datei gefunden!");
            }

            // WAL Hinweis (optional, aber stark)
            var walPath = Path.Combine(profilePath, "History-wal");
            if (File.Exists(walPath))
            {
                Console.WriteLine("⚠️ WAL aktiv – Daten könnten unvollständig sein");
            }

            // ===== STORAGE =====
            if (profilePath != null)
            {
                Console.WriteLine($"✅ Storage wird geladen aus: {profilePath}");

                var storageEntries = _storage.Load(profilePath);

                Console.WriteLine($"➡️ Storage Count: {storageEntries.Count}");

                Console.WriteLine("✅ Artifact Scanner startet...");

                var recoveredArtifacts =
                    _artifactScanner.Scan(profilePath);

                Console.WriteLine(
                    $"➡️ Recovered Artifacts: {recoveredArtifacts.Count}");

                storageEntries.AddRange(recoveredArtifacts);


                _state.SetStorageEntries(storageEntries);
            }
            else
            {
                Console.WriteLine("⚠️ Kein Profilpfad für Storage!");
            }

            // ===== FAVICONS =====
            if (faviconsPath != null)
            {
                Console.WriteLine($"✅ Favicons werden geladen aus: {faviconsPath}");

                var faviconList =
                    await _favicons.LoadFromFaviconsAsync(faviconsPath);

                Console.WriteLine(
                    $"➡️ Favicons Count: {faviconList.Count}");

                _state.SetFavicons(faviconList);
            }
            else
            {
                Console.WriteLine("⚠️ Keine Favicons gefunden!");
            }
        }

        // Helfer: findet Datei robust
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