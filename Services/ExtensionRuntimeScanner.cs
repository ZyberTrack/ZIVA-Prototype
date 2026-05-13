using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services;

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

                foreach (var dir in
                         Directory.GetDirectories(
                             fullPath,
                             "*",
                             SearchOption.AllDirectories))
                {
                    string name =
                        Path.GetFileName(dir);

                    // Chromium Extension IDs
                    // sind normalerweise 32 Zeichen
                    if (name.Length < 20)
                        continue;

                    var entry =
                        GetOrCreate(
                            extensions,
                            name);

                    entry.FoundInRuntimeArtifacts =
                        true;

                    entry.RuntimeArtifacts
                        .Add(dir);

                    entry.SourceTypes
                        .Add(runtimeDir);

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
}