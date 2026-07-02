using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{

    public class DomainEntry
    {
        public string Domain { get; set; } = "";
        public string Url { get; set; } = "";
        public DateTime VisitTime { get; set; }
        public bool IsDirect { get; set; }
        public bool IsSearch { get; set; }
        public int Position { get; set; } = 0; // für Timeline

        public string RenderColor { get; set; } = ""; // for domain Coloring

        public bool IsHeavyCluster { get; set; }
        public int HeavyClusterIndex { get; set; }

        // Automatischer Offset — keine globale Variable mehr nötig
        public int HeavyOffset => IsHeavyCluster ? HeavyClusterIndex * 20 : 0;

        public bool IsExpanded { get; set; }
        public List<BrowserHistoryEntry> SubEntries { get; set; } = new();

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten

        // Für Badges oder schnelle Filter
        public int HighestAnomalySeverity { get; set; }

        public bool HasAnomaly => HighestAnomalySeverity > 0;

        public double RenderYPercent { get; set; } = 33; // Für Domäne Summeries

        public int ClusterStartPosition { get; set; }

        public int ClusterEndPosition { get; set; }
    }

}
