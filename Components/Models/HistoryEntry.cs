using Microsoft.Data.Sqlite;
using System;

namespace ZIVA_Prototype.Components.Models
{
    public class BrowserHistoryEntry
    {
        public string Url { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string? Title { get; set; }
        public string? ReferrerUrl { get; set; }
        public string? ReferrerTitle { get; set; }
        public double Position { get; set; }
    }
}
