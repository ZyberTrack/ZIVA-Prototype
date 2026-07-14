using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineFilterResult
    {
        public List<BrowserHistoryEntry> History { get; set; } = new();

        public List<BrowserCookieEntry> Cookies { get; set; } = new();

        public List<UserInputEntry> Inputs { get; set; } = new();

        public List<BrowserExtensionEntry> Extensions { get; set; } = new();

        public List<StorageEntry> Storage { get; set; } = new();

        public List<DomainEntry> Domains { get; set; } = new();

        public List<AnalysisEntry> Analysis { get; set; } = new();


        public TimelineFilterResult(TimelineFilterResult other)
        {
            History = other.History.ToList();
            Cookies = other.Cookies.ToList();
            Inputs = other.Inputs.ToList();
            Extensions = other.Extensions.ToList();
            Storage = other.Storage.ToList();
            Domains = other.Domains.ToList();
            Analysis = other.Analysis.ToList();
        }

        public TimelineFilterResult(
        IEnumerable<BrowserHistoryEntry> history,
        IEnumerable<BrowserCookieEntry> cookies,
        IEnumerable<UserInputEntry> inputs,
        IEnumerable<BrowserExtensionEntry> extensions,
        IEnumerable<StorageEntry> storage,
        IEnumerable<DomainEntry> domains,
        IEnumerable<AnalysisEntry> analysis)
        {
            History = history.ToList();
            Cookies = cookies.ToList();
            Inputs = inputs.ToList();
            Extensions = extensions.ToList();
            Storage = storage.ToList();
            Domains = domains.ToList();
            Analysis = analysis.ToList();
        }
    }
}