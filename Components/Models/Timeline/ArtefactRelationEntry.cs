using System;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class ArtifactRelationEntry
    {
        // =====================================================
        // CORE
        // =====================================================

        public Guid Id { get; set; }
            = Guid.NewGuid();

        public ArtifactRelationType Type { get; set; }

        public int Confidence { get; set; } = 100;

        public string Reason { get; set; } = "";

        public DateTime Time { get; set; }


        // =====================================================
        // REFERENCES
        // =====================================================

        public BrowserCookieEntry? Cookie { get; set; }

        public DomainEntry? Domain { get; set; }

        public BrowserHistoryEntry? History { get; set; }

        public BrowserExtensionEntry? Extension { get; set; }

        public StorageEntry? Storage { get; set; }

        public UserInputEntry? UserInput { get; set; }

        public WebDataAutofillEntry? Autofill { get; set; }

        public FaviconEntry? Favicon { get; set; }


        // =====================================================
        // VISUALIZATION
        // =====================================================

        public bool IsVisible { get; set; } = true;

        public string Color { get; set; } = "";

        public int Severity { get; set; } = 1;
    }
}