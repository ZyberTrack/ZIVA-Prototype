using LevelDB;
using System.Text;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{
    public class StorageImportService
    {
        public List<StorageEntry> Load(string profilePath)
        {
            var entries = new List<StorageEntry>();

            string profileName = Path.GetFileName(profilePath);

            Console.WriteLine($"[Storage] Import started: {profileName}");

            LoadLocalStorage(profilePath, profileName, entries);

            LoadSessionStorage(profilePath, profileName, entries);

            LoadIndexedDb(profilePath, profileName, entries);

            Console.WriteLine($"[Storage] TOTAL ENTRIES: {entries.Count}");

            return entries;
        }

        // =========================================================
        // LOCAL STORAGE
        // =========================================================
        private void LoadLocalStorage(
            string profilePath,
            string profileName,
            List<StorageEntry> entries)
        {
            string path = Path.Combine(
                profilePath,
                "Local Storage",
                "leveldb");

            if (!Directory.Exists(path))
            {
                Console.WriteLine("[LocalStorage] Not found");
                return;
            }

            try
            {
                Console.WriteLine("[LocalStorage] Found");

                string safe = CopyLevelDb(path);

                var result = ReadLevelDb(
                    safe,
                    "LocalStorage",
                    profileName,
                    null);

                Console.WriteLine(
                    $"[LocalStorage] Parsed entries: {result.Count}");

                entries.AddRange(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[LocalStorage] ERROR: {ex.Message}");
            }
        }

        // =========================================================
        // SESSION STORAGE
        // =========================================================
        private void LoadSessionStorage(
            string profilePath,
            string profileName,
            List<StorageEntry> entries)
        {
            string path = Path.Combine(
                profilePath,
                "Session Storage");

            if (!Directory.Exists(path))
            {
                Console.WriteLine("[SessionStorage] Not found");
                return;
            }

            try
            {
                Console.WriteLine("[SessionStorage] Found");

                string safe = CopyLevelDb(path);

                var result = ReadLevelDb(
                    safe,
                    "SessionStorage",
                    profileName,
                    null);

                Console.WriteLine(
                    $"[SessionStorage] Parsed entries: {result.Count}");

                entries.AddRange(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[SessionStorage] ERROR: {ex.Message}");
            }
        }

        // =========================================================
        // INDEXED DB
        // =========================================================
        private void LoadIndexedDb(
            string profilePath,
            string profileName,
            List<StorageEntry> entries)
        {
            string indexedPath = Path.Combine(
                profilePath,
                "IndexedDB");

            if (!Directory.Exists(indexedPath))
            {
                Console.WriteLine("[IndexedDB] Not found");
                return;
            }

            try
            {
                var folders = Directory.GetDirectories(indexedPath);

                Console.WriteLine(
                    $"[IndexedDB] Databases found: {folders.Length}");

                foreach (var folder in folders)
                {
                    try
                    {
                        var levelDb = Directory
                            .GetDirectories(folder, "*.leveldb")
                            .FirstOrDefault();

                        if (levelDb == null)
                        {
                            Console.WriteLine(
                                $"[IndexedDB] No .leveldb in {folder}");

                            continue;
                        }

                        string origin =
                            ExtractOriginFromFolder(folder);

                        Console.WriteLine(
                            $"[IndexedDB] Reading: {origin}");

                        string safe = CopyLevelDb(levelDb);

                        var result = ReadLevelDb(
                            safe,
                            "IndexedDB",
                            profileName,
                            origin);

                        Console.WriteLine(
                            $"[IndexedDB] {origin} -> {result.Count}");

                        entries.AddRange(result);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[IndexedDB] Folder ERROR: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[IndexedDB] ERROR: {ex.Message}");
            }
        }

        // =========================================================
        // READ LEVELDB
        // =========================================================
        private List<StorageEntry> ReadLevelDb(
            string dbPath,
            string type,
            string profile,
            string forcedOrigin)
        {
            var list = new List<StorageEntry>();

            try
            {
                var options = new Options
                {
                    CreateIfMissing = false
                };

                using var db = new DB(options, dbPath);

                using var iterator = db.CreateIterator();

                // =========================================
                // RAW COUNT DEBUG
                // =========================================
                int rawCount = 0;

                for (iterator.SeekToFirst();
                     iterator.IsValid();
                     iterator.Next())
                {
                    rawCount++;
                }

                Console.WriteLine(
                    $"[{type}] RAW LEVELDB ENTRIES: {rawCount}");

                // rewind
                iterator.SeekToFirst();

                int shown = 0;

                // =========================================
                // MAIN PARSE LOOP
                // =========================================
                for (; iterator.IsValid(); iterator.Next())
                {
                    try
                    {
                        byte[] keyBytes = iterator.Key();
                        byte[] valueBytes = iterator.Value();

                        string decodedKey = Decode(keyBytes);
                        string decodedValue = Decode(valueBytes);

                        bool isBinary =
                            LooksBinary(valueBytes);

                        string origin =
                            forcedOrigin ?? "unknown";

                        string key = decodedKey;

                        // =================================
                        // FLEXIBLE ORIGIN PARSE
                        // =================================
                        if (string.IsNullOrEmpty(forcedOrigin))
                        {
                            var parts =
                                decodedKey.Split('\0');

                            if (parts.Length >= 2)
                            {
                                origin = parts[0];
                                key = parts[1];
                            }
                        }

                        // =================================
                        // DEBUG OUTPUT
                        // =================================
                        if (shown < 20)
                        {
                            Console.WriteLine(
                                "====================================");

                            Console.WriteLine(
                                $"TYPE: {type}");

                            Console.WriteLine(
                                $"KEY: {decodedKey}");

                            Console.WriteLine(
                                $"VALUE: {decodedValue}");

                            Console.WriteLine(
                                $"BINARY: {isBinary}");

                            Console.WriteLine(
                                "====================================");

                            shown++;
                        }

                        // =================================
                        // NO FILTERING FOR NOW
                        // =================================

                        list.Add(new StorageEntry
                        {
                            Time = DateTime.Now,

                            Profile = profile,

                            Type = type,

                            Origin = origin,

                            Key = key,

                            Value = decodedValue,

                            RawKeyHex =
                                Convert.ToHexString(keyBytes),

                            RawValueHex =
                                Convert.ToHexString(valueBytes),

                            IsBinary = isBinary,

                            IsSensitive =
                                IsLikelyToken(
                                    decodedKey,
                                    decodedValue)
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[{type}] Entry parse error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[{type}] DB OPEN ERROR: {ex.Message}");
            }

            return list;
        }

        // =========================================================
        // ORIGIN EXTRACTION
        // =========================================================
        private string ExtractOriginFromFolder(
            string folderPath)
        {
            try
            {
                string name =
                    Path.GetFileName(folderPath);

                // example:
                // https_discord.com_0

                var parts = name.Split('_');

                if (parts.Length < 2)
                    return name;

                string scheme = parts[0];

                string domain = parts[1];

                return $"{scheme}://{domain}";
            }
            catch
            {
                return folderPath;
            }
        }

        // =========================================================
        // DECODE
        // =========================================================
        private string Decode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "";

            try
            {
                if (LooksBinary(data))
                    return Convert.ToBase64String(data);

                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return Convert.ToBase64String(data);
            }
        }

        // =========================================================
        // BINARY DETECTION
        // =========================================================
        private bool LooksBinary(byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;

            int nonPrintable = 0;

            foreach (byte b in data)
            {
                if (b < 9 || (b > 13 && b < 32))
                    nonPrintable++;
            }

            double ratio =
                (double)nonPrintable / data.Length;

            return ratio > 0.30;
        }

        // =========================================================
        // TOKEN DETECTION
        // =========================================================
        private bool IsLikelyToken(
            string key,
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string k = key.ToLower();

            return
                k.Contains("token") ||
                k.Contains("auth") ||
                k.Contains("session") ||
                IsJwt(value);
        }

        private bool IsJwt(string value)
        {
            return
                value.Count(c => c == '.') == 2 &&
                value.Length > 40;
        }

        // =========================================================
        // COPY LEVELDB
        // =========================================================
        private string CopyLevelDb(string originalPath)
        {
            string tempPath = Path.Combine(
                Path.GetTempPath(),
                "leveldb_" + Guid.NewGuid());

            DirectoryCopy(
                originalPath,
                tempPath,
                true);

            return tempPath;
        }

        // =========================================================
        // DIRECTORY COPY
        // =========================================================
        private void DirectoryCopy(
            string sourceDir,
            string destDir,
            bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException(
                    $"Source not found: {sourceDir}");

            Directory.CreateDirectory(destDir);

            foreach (var file in dir.GetFiles())
            {
                string target = Path.Combine(
                    destDir,
                    file.Name);

                file.CopyTo(target, true);
            }

            if (!copySubDirs)
                return;

            foreach (var subDir in dir.GetDirectories())
            {
                string newDest = Path.Combine(
                    destDir,
                    subDir.Name);

                DirectoryCopy(
                    subDir.FullName,
                    newDest,
                    true);
            }
        }
    }
}