using System.Text;
using System.Text.RegularExpressions;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import
{
    public class StorageArtifactScanner
    {
        private readonly TimestampExtractor _timestampExtractor = new TimestampExtractor();

        // =========================================================
        // FILE TYPES
        // =========================================================
        private readonly string[] TargetExtensions =
        {
            ".ldb",
            ".log",
            ".sst"
        };

        // =========================================================
        // REGEX
        // =========================================================
        private readonly Regex UrlRegex =
            new Regex(
                @"https?:\/\/[^\s""'<>]+",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Regex JwtRegex =
            new Regex(
                @"eyJ[A-Za-z0-9_\-]+\.[A-Za-z0-9._\-]+\.[A-Za-z0-9._\-]+",
                RegexOptions.Compiled);

        private readonly Regex EmailRegex =
            new Regex(
                @"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}",
                RegexOptions.Compiled);

        // =========================================================
        // MAIN SCAN
        // =========================================================
        public List<StorageEntry> Scan(string profilePath, ImportRange? range = null)
        {
            var results = new List<StorageEntry>();

            string profile =
                Path.GetFileName(profilePath);

            Console.WriteLine(
                $"[ArtifactScanner] Scanning profile: {profile}");

            try
            {
                var files = Directory.GetFiles(
                    profilePath,
                    "*.*",
                    SearchOption.AllDirectories);

                Console.WriteLine(
                    $"[ArtifactScanner] Files found: {files.Length}");

                foreach (var file in files)
                {
                    try
                    {
                        if (!IsInterestingFile(file))
                            continue;

                        Console.WriteLine(
                            $"[ArtifactScanner] Scanning: {file}");

                        var entries =
                            ScanFile(file, profile);

                        results.AddRange(entries);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[ArtifactScanner] File error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[ArtifactScanner] Scan error: {ex.Message}");
            }

            Console.WriteLine(
    $"[ArtifactScanner] TOTAL ARTIFACTS: {results.Count}");

            if (range != null)
            {
                results = results
                    .Where(x =>
                        x.Time >= range.From &&
                        x.Time <= range.To)
                    .ToList();

                Console.WriteLine(
                    $"[ArtifactScanner] After time filter: {results.Count}");
            }

            return results;
        }

        // =========================================================
        // FILE SCAN
        // =========================================================
        private List<StorageEntry> ScanFile(
            string filePath,
            string profile)
        {
            var results = new List<StorageEntry>();

            DateTime artifactTime =
                GetBestArtifactTimestamp(filePath);

            DateTime extractedTime = artifactTime;

            byte[] data = File.ReadAllBytes(filePath);

            var strings = ExtractStrings(data);

            Console.WriteLine(
                $"[ArtifactScanner] Extracted Strings: {strings.Count}");

            foreach (var str in strings)
            {
                var possibleTimestamp = _timestampExtractor.Extract(str);

                if (possibleTimestamp != null)
                {
                    extractedTime = possibleTimestamp.Value;
                }

                try
                {
                    // =====================================
                    // JWT
                    // =====================================
                    foreach (Match match in JwtRegex.Matches(str))
                    {
                        results.Add(new StorageEntry
                        {
                            Time = extractedTime,

                            Profile = profile,

                            Type = "RecoveredJWT",

                            Origin = filePath,

                            Key = "JWT",

                            Value = match.Value,

                            IsSensitive = true
                        });
                    }

                    // =====================================
                    // URL
                    // =====================================
                    foreach (Match match in UrlRegex.Matches(str))
                    {
                        results.Add(new StorageEntry
                        {
                            Time = extractedTime,

                            Profile = profile,

                            Type = "RecoveredURL",

                            Origin = filePath,

                            Key = "URL",

                            Value = match.Value,

                            IsSensitive = false
                        });
                    }

                    // =====================================
                    // EMAIL
                    // =====================================
                    foreach (Match match in EmailRegex.Matches(str))
                    {
                        results.Add(new StorageEntry
                        {
                            Time = extractedTime,

                            Profile = profile,

                            Type = "RecoveredEmail",

                            Origin = filePath,

                            Key = "EMAIL",

                            Value = match.Value,

                            IsSensitive = false
                        });
                    }

                    // =====================================
                    // FIREBASE
                    // =====================================
                    if (str.Contains("firebase",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new StorageEntry
                        {
                            Time = extractedTime,

                            Profile = profile,

                            Type = "RecoveredFirebase",

                            Origin = filePath,

                            Key = "FIREBASE",

                            Value = TrimValue(str),

                            IsSensitive = true
                        });
                    }

                    // =====================================
                    // API KEY
                    // =====================================
                    if (str.Contains("api_key",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new StorageEntry
                        {
                            Time = extractedTime,

                            Profile = profile,

                            Type = "RecoveredApiKey",

                            Origin = filePath,

                            Key = "API_KEY",

                            Value = TrimValue(str),

                            IsSensitive = true
                        });
                    }

                    // =====================================
                    // AUTH
                    // =====================================
                    if (str.Contains("authorization",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new StorageEntry
                        {
                            Time = extractedTime,

                            Profile = profile,

                            Type = "RecoveredAuth",

                            Origin = filePath,

                            Key = "AUTH",

                            Value = TrimValue(str),

                            IsSensitive = true
                        });
                    }
                }
                catch
                {
                    // ignore broken string
                }
            }

            return RemoveDuplicates(results);
        }

        // =========================================================
        // STRING EXTRACTION
        // =========================================================
        private List<string> ExtractStrings(byte[] data)
        {
            var results = new List<string>();

            var current = new StringBuilder();

            foreach (byte b in data)
            {
                // printable ASCII
                if (b >= 32 && b <= 126)
                {
                    current.Append((char)b);
                }
                else
                {
                    if (current.Length >= 6)
                    {
                        results.Add(current.ToString());
                    }

                    current.Clear();
                }
            }

            // last string
            if (current.Length >= 6)
            {
                results.Add(current.ToString());
            }

            return results;
        }

        // =========================================================
        // INTERESTING FILE
        // =========================================================
        private bool IsInterestingFile(string file)
        {
            string name =
                Path.GetFileName(file);

            // manifest files
            if (name.StartsWith("MANIFEST"))
                return true;

            if (name == "LOG")
                return true;

            if (name == "LOG.old")
                return true;

            string ext =
                Path.GetExtension(file).ToLower();

            return TargetExtensions.Contains(ext);
        }

        // =========================================================
        // DEDUPLICATION
        // =========================================================
        private List<StorageEntry> RemoveDuplicates(
            List<StorageEntry> entries)
        {
            return entries
                .GroupBy(x =>
                    $"{x.Key}|{x.Value}|{x.Origin}")
                .Select(g => g.First())
                .ToList();
        }

        // =========================================================
        // VALUE LIMITER
        // =========================================================
        private string TrimValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            value = value.Trim();

            if (value.Length > 300)
                return value.Substring(0, 300);

            return value;
        }

        // =========================================================
        // BEST TIMESTAMP
        // =========================================================
        private DateTime GetBestArtifactTimestamp(
            string filePath)
        {
            try
            {
                var file = new FileInfo(filePath);

                // beste Näherung an echte Chromium Aktivität
                return file.LastWriteTime;
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}