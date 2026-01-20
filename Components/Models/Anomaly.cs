class AnomalyEntry
{
    public DateTime Time { get; set; }

    public string Url { get; set; } = "";

    public double Position { get; set; }

    public DomainEntry LinkedDomain { get; set; } = null!;

    public AnomalyType Type { get; set; }

    public string Description { get; set; } = "";

    // Optional – extrem sinnvoll später
    public int Severity { get; set; } = 1; // 1–5
}

public enum AnomalyType
{
    BlacklistedDomain,
    SuspiciousRedirect,
    ExcessiveRequests,
    Unknown
}
