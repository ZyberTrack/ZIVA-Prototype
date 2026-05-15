using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class TimelineArtifact
    {
        public DateTime Time { get; set; }

        public object Data { get; set; } = default!;

        public ArtifactType Type { get; set; }
    }
}
