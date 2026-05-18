using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class FaviconEntry
    {
        public DateTime Time { get; set; }

        public string PageUrl { get; set; } = "";

        public string IconUrl { get; set; } = "";

        public int Position { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten
    }
}
