using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models.WebData
{
    public class StorageArtifact
    {
        public DateTime Time { get; set; }

        public string Profile { get; set; }

        public string FilePath { get; set; }

        public string ArtifactType { get; set; }

        public string Value { get; set; }

        public long Offset { get; set; }

        public bool IsSensitive { get; set; }
    }
}
