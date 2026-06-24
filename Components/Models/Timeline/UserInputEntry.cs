using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class UserInputEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Für stabile Identifikation, auch wenn sich der Inhalt ändert

        public DateTime Time { get; set; }
        public string Value { get; set; } = "";
        public UserInputType Type { get; set; }
        public int Position { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten

        // Für Badges oder schnelle Filter
        public int HighestAnomalySeverity { get; set; }

        public bool HasAnomaly => HighestAnomalySeverity > 0;
    }

    public enum UserInputType
    {
        SearchQuery,
        Autofill,
        Favicon,
    }
}
