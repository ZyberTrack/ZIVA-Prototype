using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import;

public class ExtensionRuntimeScanner
{
    public void Scan(
        string profilePath,
        Dictionary<string,
        BrowserExtensionEntry> extensions)
    {
        string[] runtimeDirs =
        {
            "Local Extension Settings",
            "Sync Extension Settings",
            "Extension Rules",
            "Extension Scripts",
            "IndexedDB",
            "Local Storage"
        };

        foreach (var runtimeDir in runtimeDirs)
        {
            try
            {
                string fullPath =
                    Path.Combine(
                        profilePath,
                        runtimeDir);

                if (!Directory.Exists(fullPath))
                    continue;

                foreach (var dir in GetRuntimeExtensionDirectories(fullPath)) // Local Extension Settings/<id> Extension Scripts/< id > IndexedDB / chrome - extension_<id>
                {
                    string? name = ExtractExtensionId(dir); // IndexedDB\chrome-extension_xxx\leveldb --> xxx is the extension id

                    // =====================================================
                    // FALLBACK: ARTEFAKTNAME
                    // =====================================================

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        string folderName =
                            Path.GetFileName(dir);

                        // =====================================================
                        // GENERISCHE RUNTIME ORDNER IGNORIEREN
                        // =====================================================

                        if (IsGenericRuntimeFolder(folderName))
                        {
                            continue;
                        }

                        if (LooksLikeExtensionArtifact(dir))
                        {
                            name = SanitizeArtifactName(dir);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var entry =
                        GetOrCreate(
                            extensions,
                            name);

                    // =====================================================
                    // CONFIDENCE
                    // =====================================================

                    if (IsValidExtensionId(name))
                    {
                        entry.ConfidenceScore += 40;
                    }
                    else
                    {
                        entry.ConfidenceScore += 10;
                    }

                    if (LooksLikeExtensionArtifact(dir))
                    {
                        entry.ConfidenceScore += 15;
                    }

                    entry.FoundInRuntimeArtifacts = true;

                    if (!entry.FoundInExtensionsFolder && !entry.FoundInPreferences)
                    {
                        entry.IsResidualArtifact = true;
                    }

                    if (!entry.RuntimeArtifacts.Contains(dir))
                    {
                        entry.RuntimeArtifacts.Add(dir);

                        // =====================================================
                        // SIMPLE RUNTIME KEYWORD PARSING
                        // =====================================================

                        TryParseRuntimeKeywords(
                            dir,
                            entry);

                        if (dir.Contains(".leveldb",StringComparison.OrdinalIgnoreCase))
                        {
                            entry.RuntimeArtifacts
                                .Add("[LevelDB]");
                        }

                        if (dir.Contains(
                                ".blob",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            entry.RuntimeArtifacts
                                .Add("[BlobStorage]");
                        }
                    }

                    if (!entry.SourceTypes.Contains(runtimeDir))
                    {
                        entry.SourceTypes
                            .Add(runtimeDir);
                    }

                    // =====================================================
                    // TIMESTAMP
                    // =====================================================

                    try
                    {
                        DateTime artifactTime =
                            Directory.GetLastWriteTimeUtc(
                                dir);

                        if (artifactTime.Year >= 2015)
                        {
                            if (entry.InstallTime == default ||
                                artifactTime < entry.InstallTime)
                            {
                                ExtensionTimestampHelper
                                    .UpdateInstallTime(
                                        entry,
                                        artifactTime);
                            }
                        }
                        if (artifactTime > entry.LastRuntimeActivity)
                        {
                            entry.LastRuntimeActivity =
                                artifactTime;
                        }
                    }
                    catch
                    {
                    }

                    // =====================================================
                    // CONFIDENCE
                    // =====================================================

                    entry.ConfidenceScore += 20;
                }
            }
            catch
            {
            }
        }
    }

    private IEnumerable<string>
 GetRuntimeExtensionDirectories(
     string runtimeRoot)
    {
        var results =
            new List<string>();

        foreach (var dir in
                 Directory.GetDirectories(
                     runtimeRoot,
                     "*",
                     SearchOption.AllDirectories))
        {
            try
            {
                if (!LooksLikeExtensionArtifact(dir))
                    continue;

                results.Add(dir);
            }
            catch
            {
            }
        }

        return results;
    }

    private void TryParseRuntimeKeywords(
    string path,
    BrowserExtensionEntry entry)
    {
        try
        {
            // =====================================================
            // NUR KLEINE DATEIEN
            // =====================================================

            var files =
                Directory.GetFiles(
                    path,
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Where(f =>
                    f.EndsWith(".ldb",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    f.EndsWith(".log",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    f.EndsWith(".blob",
                        StringComparison.OrdinalIgnoreCase))
                .Take(5);

            foreach (var file in files)
            {
                try
                {
                    // =====================================================
                    // KLEINE DATEIEN LIMIT
                    // =====================================================

                    var info =
                        new FileInfo(file);

                    if (info.Length >
                        1024 * 1024 * 5)
                    {
                        continue;
                    }

                    byte[] data = File.ReadAllBytes(file);

                    string text =
                        System.Text.Encoding.UTF8
                            .GetString(data);

                    // =====================================================
                    // LOCALHOST
                    // =====================================================

                    if (text.Contains(
                            "localhost",
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        text.Contains(
                            "127.0.0.1",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (!entry.HostPermissions.Contains(
                            "localhost"))
                        {
                            entry.HostPermissions.Add(
                                "localhost");
                        }
                    }

                    // =====================================================
                    // SCRIPTING
                    // =====================================================

                    string[] dangerous =
                    {
                    "scripting",
                    "webrequestblocking",
                    "webrequest",
                    "debugger",
                    "nativemessaging"
                };

                    foreach (var keyword in dangerous)
                    {
                        if (text.Contains(
                                keyword,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            if (!entry.AllPermissions.Contains(
                                    keyword))
                            {
                                entry.AllPermissions.Add(
                                    keyword);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    private BrowserExtensionEntry GetOrCreate(
    Dictionary<string,
    BrowserExtensionEntry> extensions,
    string id)
    {
        string normalized =
            NormalizeExtensionArtifactId(id);

        // =====================================================
        // EXISTIERENDE EXTENSION SUCHEN
        // =====================================================

        var existing =
            extensions.Values
                .FirstOrDefault(x =>
                    NormalizeExtensionArtifactId(
                        x.Id)
                    == normalized);

        if (existing != null)
        {
            return existing;
        }

        // =====================================================
        // NEUE EXTENSION
        // =====================================================

        extensions[id] =
            new BrowserExtensionEntry
            {
                Id = id
            };

        return extensions[id];
    }

    private string? ExtractExtensionId(
    string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        path =
            path.ToLowerInvariant();

        // =====================================================
        // GESAMTEN PFAD DURCHSUCHEN
        // =====================================================

        var matches =
            System.Text.RegularExpressions.Regex
                .Matches(
                    path,
                    @"[a-p]{32}");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            string candidate =
                match.Value;

            if (IsValidExtensionId(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private string NormalizeExtensionArtifactId(
    string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        raw =
            raw.ToLowerInvariant();

        // =====================================================
        // PFAD AUFTEILEN
        // =====================================================

        var parts =
            raw.Split(
                Path.DirectorySeparatorChar,
                StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            // =====================================================
            // DIREKTE CHROMIUM ID
            // =====================================================

            if (IsValidExtensionId(part))
                return part;

            // =====================================================
            // chrome-extension_<id>
            // =====================================================

            if (part.Contains("chrome-extension_"))
            {
                int start =
                    part.IndexOf(
                        "chrome-extension_");

                string candidate =
                    part.Substring(
                        start +
                        "chrome-extension_".Length);

                // =====================================================
                // ALLES ABSCHNEIDEN NACH:
                // . _
                // =====================================================

                char[] splitChars =
                {
                '.',
                '_'
            };

                foreach (char c in splitChars)
                {
                    candidate = candidate.Trim('-','_','.','#');

                    int idx =
                        candidate.IndexOf(c);

                    if (idx > 0)
                    {
                        candidate =
                            candidate.Substring(
                                0,
                                idx);
                    }
                }

                if (IsValidExtensionId(candidate))
                    return candidate;
            }
        }

        // =====================================================
        // FALLBACK
        // =====================================================

        // =====================================================
        // TECHNISCHE SUFFIXE ENTFERNEN
        // =====================================================

        string[] blacklist =
        {
            ".indexeddb.leveldb",
            ".indexeddb.blob",
            ".indexeddb",
            ".leveldb",
            ".blob",
            "leveldb",
            "blob",
            "indexeddb",
            "_0",
            "_1"
        };

        foreach (var item in blacklist)
        {
            raw =
                raw.Replace(
                    item,
                    "",
                    StringComparison.OrdinalIgnoreCase);
        }

        raw =
            raw.Trim(
                '-',
                '_',
                '.',
                '#',
                ' ');

        return raw;
    }

    private string SanitizeArtifactName(
        string path)
    {
        string name =
            Path.GetFileName(path);

        name =
            NormalizeExtensionArtifactId(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            name =
                $"UNKNOWN-{Math.Abs(path.GetHashCode())}";
        }

        return name;
    }

    private bool LooksLikeExtensionArtifact(
        string path)
    {
        path =
            path.ToLowerInvariant();

        return
            path.Contains("chrome-extension")
            ||
            path.Contains("extension")
            ||
            path.Contains("manifest")
            ||
            path.Contains("service_worker")
            ||
            path.Contains("background")
            ||
            path.Contains("indexeddb")
            ||
            path.Contains("leveldb")
            ||
            path.Contains("local storage")
            ||
            path.Contains("extension rules")
            ||
            path.Contains("extension scripts")
            ||
            path.Contains("webpack")
            ||
            path.Contains("vite")
            ||
            path.Contains("localhost")
            ||
            path.Contains("127.0.0.1");
    }

    private bool IsGenericRuntimeFolder(
    string name)
    {
        name =
            name.ToLowerInvariant();

        string[] generic =
        {
        "leveldb",
        "blob",
        "indexeddb",
        "local storage",
        "cache",
        "code cache",
        "shared_proto_db",
        "service worker",
        "session storage",
        "databases",
        "metadata"
    };

        return generic.Contains(name);
    }

    private bool IsValidExtensionId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        if (id.Length != 32)
            return false;

        return id.All(c =>
            c >= 'a' &&
            c <= 'p');
    }
}