namespace ZIVA_Prototype.Components.Models.Timeline
{
    public class AnomalyEntry
    {
        // ----------------------------------------------------
        // CORE
        // ----------------------------------------------------

        public Guid Id { get; set; } = Guid.NewGuid();

        public AnomalyType Type { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public int Severity { get; set; } = 1; // 1–5

        public int Confidence { get; set; } = 100; // 0–100 %

        public int Count { get; set; } = 1;

        public bool IsResolved { get; set; }

        public bool IsFalsePositive { get; set; }


        // ----------------------------------------------------
        // TIME
        // ----------------------------------------------------

        public DateTime FirstSeen { get; set; }

        public DateTime LastSeen { get; set; }

        public DateTime DetectedAt { get; set; } = DateTime.Now;


        // ----------------------------------------------------
        // POSITIONING / RENDERING
        // ----------------------------------------------------

        public int Position { get; set; }

        public double TargetPosition { get; set; }

        public double TargetYPercent { get; set; }

        public AnomalyTargetType TargetType { get; set; }


        // ----------------------------------------------------
        // RELATIONSHIPS
        // ----------------------------------------------------

        // Hauptdomain (optional)
        public DomainEntry? LinkedDomain { get; set; }

        // Präzise Artefakt-Referenzen
        public List<BrowserHistoryEntry> LinkedHistory { get; set; } = new();

        public List<BrowserCookieEntry> LinkedCookies { get; set; } = new();

        public List<UserInputEntry> LinkedInputs { get; set; } = new();

        public List<BrowserExtensionEntry> LinkedExtensions { get; set; } = new();

        public List<StorageEntry> LinkedStorage { get; set; } = new();

        // Andere zusammenhängende Anomalien
        public List<Guid> RelatedAnomalyIds { get; set; } = new();


        // ----------------------------------------------------
        // URL / NETWORK
        // ----------------------------------------------------

        public string Url { get; set; } = "";

        public string Domain { get; set; } = "";

        public string ReferrerUrl { get; set; } = "";

        public string Origin { get; set; } = "";

        public string IpAddress { get; set; } = "";

        public string Country { get; set; } = "";

        public bool IsEncrypted { get; set; }

        public bool IsIncognito { get; set; }


        // ----------------------------------------------------
        // ANALYSIS FLAGS
        // ----------------------------------------------------

        public bool IsBlacklistMatch { get; set; }

        public bool IsSuspiciousRedirect { get; set; }

        public bool IsMassRequestPattern { get; set; }

        public bool IsDeletedHistoryIndicator { get; set; }

        public bool IsPersistenceRelated { get; set; }

        public bool IsTrackingRelated { get; set; }

        public bool IsCredentialRelated { get; set; }


        // ----------------------------------------------------
        // VISUALIZATION
        // ----------------------------------------------------

        public string Color { get; set; } = "";

        public string Icon { get; set; } = "";

        public bool IsVisible { get; set; } = true;

        public bool IsFocused { get; set; }

        public bool IsExpanded { get; set; }


        // ----------------------------------------------------
        // TAGGING
        // ----------------------------------------------------

        public List<string> Tags { get; set; } = new();

        public List<string> MatchedRules { get; set; } = new();

        public List<string> Evidence { get; set; } = new();

        public List<string> Notes { get; set; } = new();

        // ----------------------------------------------------
        // SEVERITYBOOST
        // ----------------------------------------------------

        public int CorrelationBoost { get; set; }

        public int RelatedAnomalyCount { get; set; }

        public bool IsCorrelated { get; set; }
    }


    public enum AnomalyTargetType
    {
        Unknown,

        Domain,
        History,
        Cookie,
        Autofill,
        Extension,
        Storage,
        UserInput
    }


    public enum AnomalyType
    {
        Unknown,

        BlacklistedDomain,
        SuspiciousRedirect,
        ExcessiveRequests,
        DeletedHistoryIndicator,

        SuspiciousCookie,
        SuspiciousExtension,
        TrackingBehavior,
        CredentialExposure,
        PersistenceMechanism,

        SuspiciousSearch,
        SuspiciousStorage,

        TimeManipulation,
        BurstActivity,
        SessionHijackIndicator,

        CorrelatedThreat,
        ManualInvestigatorFlag
    }
}