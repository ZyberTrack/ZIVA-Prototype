using Microsoft.Data.Sqlite;
using System;
using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class BrowserHistoryEntry
    {

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string? Title { get; set; }
        public string? ReferrerUrl { get; set; }
        public string? ReferrerTitle { get; set; }
        public int Position { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten

        // Für Badges oder schnelle Filter
        public int HighestAnomalySeverity { get; set; }

        public bool HasAnomaly => HighestAnomalySeverity > 0;
    }
}
