using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class FaviconEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Time { get; set; }

        public string PageUrl { get; set; } = "";

        public string IconUrl { get; set; } = "";

        public int Position { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten

        // Für Badges oder schnelle Filter
        public int HighestAnomalySeverity { get; set; }

        public bool HasAnomaly => HighestAnomalySeverity > 0;
    }
}
