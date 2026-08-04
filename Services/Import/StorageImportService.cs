// INFOR FÜR BACHELORARBEIT:
// Timestamps werden mit TimestampExtractor.cs geparsed.
// Es werden ISO8601 Strings und große Zahlen (Unix Timestamps) erkannt.
// Es ist wichtig zu verstehen, dass diese Timestamps nicht direkt von Chromium stammen,
// Der StorageScanner extrahiert weitere Storage Artefakte aus den Raw Daten, z.B. aus den Keys und Values der LevelDB Einträge.

using LevelDB;
using System.Text;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import
{
    public class StorageImportService
    {
        public List<StorageEntry> Load(string profilePath, ImportRange? range = null)
        {
            var entries = new List<StorageEntry>();

            string profileName = Path.GetFileName(profilePath);

            Console.WriteLine($"[Storage] Import started: {profileName}");

            LoadLocalStorage(profilePath, profileName, entries);

            LoadSessionStorage(profilePath, profileName, entries);

            LoadIndexedDb(profilePath, profileName, entries);

            Console.WriteLine($"[Storage] TOTAL ENTRIES: {entries.Count}");

            if (range != null)
            {
                entries = entries
                    .Where(x =>
                        x.Time >= range.From &&
                        x.Time <= range.To)
                    .ToList();

                Console.WriteLine(
                    $"[Storage] After time filter: {entries.Count}");
            }

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

                DateTime artifactTime =
                    GetBestArtifactTimestamp(path);

                string safe = CopyLevelDb(path);

                var result = ReadLevelDb(
                    safe,
                    "LocalStorage",
                    profileName,
                    null,
                    artifactTime);

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

                DateTime artifactTime =
                    GetBestArtifactTimestamp(path);

                string safe = CopyLevelDb(path);

                var result = ReadLevelDb(
                    safe,
                    "SessionStorage",
                    profileName,
                    null,
                    artifactTime);

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

                        DateTime artifactTime =
                            GetBestArtifactTimestamp(levelDb);

                        string safe = CopyLevelDb(levelDb);

                        var result = ReadLevelDb(
                            safe,
                            "IndexedDB",
                            profileName,
                            origin,
                            artifactTime);

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
        // TIMESTAMP EXTRACTOR
        // =========================================================

        private readonly TimestampExtractor _timestampExtractor = new TimestampExtractor();

        // =========================================================
        // READ LEVELDB
        // =========================================================
        private List<StorageEntry> ReadLevelDb(
            string dbPath,
            string type,
            string profile,
            string forcedOrigin,
            DateTime artifactTime)
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

                int rawCount = 0;

                for (iterator.SeekToFirst();
                     iterator.IsValid();
                     iterator.Next())
                {
                    rawCount++;
                }

                Console.WriteLine(
                    $"[{type}] RAW LEVELDB ENTRIES: {rawCount}");

                iterator.SeekToFirst();

                int shown = 0;

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

                        // =====================================
                        // TIMESTAMP EXTRACTION
                        // =====================================

                        DateTime extractedTime =
                            artifactTime;

                        var keyTimestamp =
                            _timestampExtractor.Extract(
                                decodedKey);

                        var valueTimestamp =
                            _timestampExtractor.Extract(
                                decodedValue);

                        if (valueTimestamp != null)
                        {
                            extractedTime =
                                valueTimestamp.Value;
                        }
                        else if (keyTimestamp != null)
                        {
                            extractedTime =
                                keyTimestamp.Value;
                        }

                        // =====================================
                        // ORIGIN PARSE
                        // =====================================

                        string origin =
                            forcedOrigin ?? "unknown";

                        string key = decodedKey;

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

                        // =====================================
                        // DEBUG
                        // =====================================

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
                                $"TIME: {extractedTime}");

                            Console.WriteLine(
                                $"BINARY: {isBinary}");

                            Console.WriteLine(
                                "====================================");

                            shown++;
                        }

                        list.Add(new StorageEntry
                        {
                            Time = extractedTime,

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
        // BEST TIMESTAMP
        // =========================================================
        private DateTime GetBestArtifactTimestamp(
            string folderPath)
        {
            try
            {
                var dir = new DirectoryInfo(folderPath);

                var files = dir.GetFiles(
                    "*",
                    SearchOption.AllDirectories);

                if (files.Length == 0)
                    return dir.LastWriteTime;

                // beste Näherung an echte Chromium Aktivität
                return files.Max(f => f.LastWriteTime);
            }
            catch
            {
                return DateTime.Now;
            }
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
