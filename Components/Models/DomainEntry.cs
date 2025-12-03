using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models
{
    public class DomainEntry
    {
        public string Domain { get; set; }
        public string Url { get; set; }
        public DateTime VisitTime { get; set; }
        public bool IsDirect { get; set; } // true = direkter Aufruf
        public bool IsSearch { get; set; } // true = Suchmaschine
        public bool IsExpanded { get; set; } = false;
        public List<BrowserHistoryEntry> SubEntries { get; set; } = new();
        public int Position { get; set; } = 0; // für Timeline
    }

}
