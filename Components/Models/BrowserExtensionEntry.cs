using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models
{
    public class BrowserExtensionEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new();
        public List<string> HostPermissions { get; set; } = new();

        public List<string> ContentScripts { get; set; } = new();

        public string? BackgroundScript { get; set; }

        public DateTime InstallTime { get; set; }

        public int Position { get; set; } // Timeline
    }
}
