using ZIVA_Prototype.Components.Models;

public class DomainEntry
{
    public string Domain { get; set; } = "";
    public string Url { get; set; } = "";
    public DateTime VisitTime { get; set; }
    public bool IsDirect { get; set; }
    public bool IsSearch { get; set; }
    public int Position { get; set; } = 0; // für Timeline

    public bool IsHeavyCluster { get; set; }
    public int HeavyClusterIndex { get; set; }

    // Automatischer Offset — keine globale Variable mehr nötig
    public int HeavyOffset => IsHeavyCluster ? HeavyClusterIndex * 20 : 0;

    public bool IsExpanded { get; set; }
    public List<BrowserHistoryEntry> SubEntries { get; set; } = new();
}
