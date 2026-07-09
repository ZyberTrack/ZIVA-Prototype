using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class NavigationPathResult
    {
        public HashSet<DomainEntry> Domains { get; } = new();

        public HashSet<BrowserHistoryEntry> History { get; } = new();

        public HashSet<BrowserCookieEntry> Cookies { get; } = new();

        public HashSet<UserInputEntry> Inputs { get; } = new();

        public HashSet<BrowserExtensionEntry> Extensions { get; } = new();

        public HashSet<StorageEntry> Storage { get; } = new();

        public HashSet<AnalysisEntry> Analysis { get; } = new();
    }
}
