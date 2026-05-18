using System;
using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class StorageEntry
    {
        public DateTime Time { get; set; }

        public string Profile { get; set; }

        public string Type { get; set; }

        public string Origin { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string RawKeyHex { get; set; }

        public string RawValueHex { get; set; }

        public bool IsBinary { get; set; }

        public bool IsSensitive { get; set; }

        public DateTime? LastModified { get; set; }

        public int Position { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten

        // Für Badges oder schnelle Filter
        public int HighestAnomalySeverity { get; set; }

        public bool HasAnomaly => HighestAnomalySeverity > 0;
    }
}