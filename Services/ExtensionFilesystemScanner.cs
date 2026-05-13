using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services;

public class ExtensionFilesystemScanner
{
    public void Scan(
        Dictionary<string,
        BrowserExtensionEntry> extensions)
    {
        string[] roots =
        {
            Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop),

            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments),

            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                "Downloads")
        };

        string[] ignoredFolders =
        {
            "node_modules",
            ".git",
            "bin",
            "obj",
            "Temp",
            "Cache"
        };

        foreach (var root in roots)
        {
            try
            {
                if (!Directory.Exists(root))
                    continue;

                foreach (var dir in
                         Directory.GetDirectories(
                             root,
                             "*",
                             SearchOption.AllDirectories))
                {
                    try
                    {
                        string folderName =
                            Path.GetFileName(dir);

                        if (ignoredFolders.Any(x =>
                            folderName.Equals(
                                x,
                                StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        bool hasManifest =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "manifest.json"));

                        bool hasScripts =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "background.js"))
                            ||
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "content.js"))
                            ||
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "service_worker.js"));

                        if (!hasManifest ||
                            !hasScripts)
                        {
                            continue;
                        }

                        string fakeId =
                            "FS-" +
                            Math.Abs(
                                dir.GetHashCode());

                        var entry =
                            GetOrCreate(
                                extensions,
                                fakeId);

                        entry.Name =
                            Path.GetFileName(dir);

                        entry.InstallLocation =
                            dir;

                        entry.IsUnpacked =
                            true;

                        entry.FoundInFilesystem =
                            true;

                        entry.SourceTypes
                            .Add("Filesystem");

                        // =====================================================
                        // FORENSIC TIMESTAMP
                        // =====================================================

                        DateTime fsTime =
                            GetBestFilesystemTimestamp(
                                dir);

                        ExtensionTimestampHelper
                            .UpdateInstallTime(
                                entry,
                                fsTime);

                        // =====================================================
                        // DETECTED FILES
                        // =====================================================

                        entry.DetectedFiles
                            .AddRange(
                                Directory.GetFiles(
                                    dir,
                                    "*.js",
                                    SearchOption.TopDirectoryOnly)
                                .Distinct());

                        // =====================================================
                        // FLAGS
                        // =====================================================

                        entry.HasManifest =
                            hasManifest;

                        entry.HasBackgroundScript =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "background.js"));

                        entry.HasContentScripts =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "content.js"));

                        entry.HasServiceWorker =
                            File.Exists(
                                Path.Combine(
                                    dir,
                                    "service_worker.js"));

                        // =====================================================
                        // CONFIDENCE
                        // =====================================================

                        entry.ConfidenceScore += 40;
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
    }

    private DateTime GetBestFilesystemTimestamp(
        string dir)
    {
        try
        {
            var files =
                Directory.GetFiles(
                    dir,
                    "*",
                    SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                return Directory
                    .GetCreationTimeUtc(dir);
            }

            DateTime earliest =
                files.Min(x =>
                    File.GetCreationTimeUtc(x));

            if (earliest.Year >= 2015 &&
                earliest <= DateTime.UtcNow)
            {
                return earliest;
            }

            return Directory
                .GetCreationTimeUtc(dir);
        }
        catch
        {
            return DateTime.UtcNow;
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