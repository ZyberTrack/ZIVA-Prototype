class AnomalyEntry
{
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }      // Optional, aber sehr sinnvoll

    public string Url { get; set; } = "";

    public double Position { get; set; }

    public DomainEntry LinkedDomain { get; set; } = null!;

    // 🔥 NEU
    public AnomalyTargetType TargetType { get; set; }

    // Ziel-Position (X)
    public double TargetPosition { get; set; }

    // Ziel-Y in Prozent
    public double TargetYPercent { get; set; }

    public AnomalyType Type { get; set; }

    public string Description { get; set; } = "";

    public int Severity { get; set; } = 1;      // 1–5

    public int Count { get; set; } = 1;         // neu

}
public enum AnomalyTargetType
{
    Domain,
    Cookie,
    Autofill
}

public enum AnomalyType
{
    BlacklistedDomain,
    SuspiciousRedirect,
    ExcessiveRequests,
    DeletedHistoryIndicator, // 🔥 NEU
    Unknown
}

