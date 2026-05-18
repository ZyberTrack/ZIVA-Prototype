using System.Text.Json.Serialization;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class BrowserCookieEntry
    {
        public string Host { get; set; } = "";
        public string Name { get; set; } = "";
        public string Value { get; set; } = ""; // nur unverschlüsselte Cookies
        public byte[] EncryptedValue { get; set; } = Array.Empty<byte>();
        public string Path { get; set; } = "";

        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastAccessed { get; set; }

        public int Position { get; set; } // für Timeline, falls genutzt

        public CookieCategory Category { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten

        // Für Badges oder schnelle Filter
        public int HighestAnomalySeverity { get; set; }

        public bool HasAnomaly => HighestAnomalySeverity > 0;
    }
}