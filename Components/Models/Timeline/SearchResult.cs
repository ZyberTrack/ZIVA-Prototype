using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class SearchResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TimelineArtifact Artifact { get; set; }

        public SearchResultType Type { get; set; }

        public DateTime Time { get; set; }

        public string DisplayText { get; set; } = "";
    }
}
