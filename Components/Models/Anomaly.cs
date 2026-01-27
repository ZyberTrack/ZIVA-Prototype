class AnomalyEntry
{
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }      // Optional, aber sehr sinnvoll

    public string Url { get; set; } = "";

    public double Position { get; set; }

    public DomainEntry LinkedDomain { get; set; } = null!;

    public AnomalyType Type { get; set; }

    public string Description { get; set; } = "";

    public int Severity { get; set; } = 1;      // 1–5

    public int Count { get; set; } = 1;         // neu

}


public enum AnomalyType
{
    BlacklistedDomain,
    SuspiciousRedirect,
    ExcessiveRequests,
    Unknown
}
