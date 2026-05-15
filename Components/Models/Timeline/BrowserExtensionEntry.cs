using System;
using System.Collections.Generic;
using System.Linq;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class BrowserExtensionEntry
    {
        // 🔑 Identität
        public string Id { get; set; } = string.Empty;
        public string ArtifactId => $"EXT-{Id}";

        // 📦 Basisinfos
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // 🔐 Permissions
        public List<string> Permissions { get; set; } = new();
        public List<string> HostPermissions { get; set; } = new();
        public List<string> OptionalPermissions { get; set; } = new();
        public List<string> OptionalHostPermissions { get; set; } = new();

        // 🔗 Erweiterte Zugriffsmöglichkeiten
        public List<string> WebAccessibleResources { get; set; } = new();
        public List<string> ExternallyConnectableMatches { get; set; } = new();
        public List<string> Commands { get; set; } = new();

        // 📜 Verhalten
        public List<string> ContentScripts { get; set; } = new();
        public string? BackgroundScript { get; set; }

        // 🖥️ Browser State (Preferences)

        public bool IsEnabled { get; set; }

        public bool IsFromWebStore { get; set; }

        public bool IsUnpacked { get; set; }

        public string InstallLocation { get; set; } = string.Empty;

        public string UpdateUrl { get; set; } = string.Empty;

        // 🧠 Detection
        public int RiskScore { get; set; }
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
        public List<Finding> Findings { get; set; } = new();

        // ⏱️ Zeit
        public DateTime InstallTime { get; set; }

        // ⚠️ Optional: nur wenn du es wirklich brauchst
        // public int Position { get; set; }

        // 🔄 Helper: alle Permissions kombiniert (wichtig für Analyse!)
        public List<string> AllPermissions =>
            Permissions
            .Concat(HostPermissions)
            .Concat(OptionalPermissions)
            .Concat(OptionalHostPermissions)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        // =====================================================
        // FORENSIC SOURCES
        // =====================================================

        public List<string> SourceTypes { get; set; } = new();

        public List<string> RuntimeArtifacts { get; set; } = new();

        public List<string> ResidualArtifacts { get; set; } = new();

        public List<string> HistoryIndicators { get; set; } = new();

        public List<string> DetectedFiles { get; set; } = new();

        public bool FoundInPreferences { get; set; }

        public bool FoundInSecurePreferences { get; set; }

        public bool FoundInExtensionsFolder { get; set; }

        public bool FoundInRuntimeArtifacts { get; set; }

        public bool FoundInHistory { get; set; }

        public bool FoundInFilesystem { get; set; }

        public bool IsResidualArtifact { get; set; }

        public bool ManifestMissing { get; set; }

        public bool HasBackgroundScript { get; set; }

        public bool HasContentScripts { get; set; }

        public bool HasServiceWorker { get; set; }

        public int ConfidenceScore { get; set; }

        // =====================================================
        // FORENSIC HELPERS
        // =====================================================

        public bool HasTimestamp =>
            InstallTime != default;

        // =====================================================
        // MANIFEST / FILESYSTEM
        // =====================================================

        public bool HasManifest { get; set; }

        // =====================================================
        // OPTIONAL: BROWSER CONTEXT
        // =====================================================

        public string Browser { get; set; } = string.Empty;

        public string ProfileName { get; set; } = string.Empty;


        public int Position { get; set; } // Timeline
    }

    // 🔥 Sauberer RiskLevel
    public enum RiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    // 🔥 Strukturierte Findings (viel stärker als string!)
    public class Finding
    {
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public int Weight { get; set; } // für Scoring
    }
}