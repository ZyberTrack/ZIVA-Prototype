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

                    if (name == null)
                        continue;


                    if (!IsValidExtensionId(name))
                        continue;

                    var entry =
                        GetOrCreate(
                            extensions,
                            name);

                    entry.FoundInRuntimeArtifacts = true;

                    if (!entry.FoundInExtensionsFolder && !entry.FoundInPreferences)
                    {
                        entry.IsResidualArtifact = true;
                    }

                    if (!entry.RuntimeArtifacts.Contains(dir))
                    {
                        entry.RuntimeArtifacts.Add(dir);

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
                string path =
                    dir.ToLowerInvariant();

                // =====================================================
                // CHROMIUM EXTENSION RUNTIME PATTERNS
                // =====================================================

                bool looksLikeExtension =
                    path.Contains(
                        "chrome-extension_")
                    ||
                    IsValidExtensionId(
                        Path.GetFileName(dir));

                if (!looksLikeExtension)
                    continue;

                results.Add(dir);
            }
            catch
            {
            }
        }

        return results;
    }

    private BrowserExtensionEntry GetOrCreate(
        Dictionary<string,
        BrowserExtensionEntry> extensions,
        string id)
    {
        if (!extensions.ContainsKey(id))
        {
            extensions[id] =
                new BrowserExtensionEntry
                {
                    Id = id
                };
        }

        return extensions[id];
    }

    private string? ExtractExtensionId(
    string path)
    {
        var parts =
            path.Split(
                Path.DirectorySeparatorChar,
                StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            // =====================================================
            // 1️⃣ DIREKTE EXTENSION ID
            // =====================================================

            if (IsValidExtensionId(part))
                return part;

            // =====================================================
            // 2️⃣ chrome-extension_<id>
            // =====================================================

            if (part.StartsWith(
                    "chrome-extension_"))
            {
                string id =
                    part.Replace(
                        "chrome-extension_",
                        "");

                // .indexeddb.leveldb entfernen
                int dot =
                    id.IndexOf('.');

                if (dot > 0)
                {
                    id =
                        id.Substring(
                            0,
                            dot);
                }

                if (IsValidExtensionId(id))
                    return id;
            }
        }

        return null;
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