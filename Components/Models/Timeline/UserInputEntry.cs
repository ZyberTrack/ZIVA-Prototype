using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class UserInputEntry
    {
        public DateTime Time { get; set; }
        public string Value { get; set; } = "";
        public UserInputType Type { get; set; }
        public int Position { get; set; }

        [JsonIgnore]
        public List<ArtifactRelationEntry> Relations { get; set; } = new(); // Für Verknüpfungen zu anderen Artefakten
    }

    public enum UserInputType
    {
        SearchQuery,
        Autofill,
        Favicon,
    }
}
