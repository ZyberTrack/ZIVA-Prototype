using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineArtifactBuilderService
    {
        public List<TimelineArtifact> Build(
            IEnumerable<BrowserHistoryEntry> history,
            IEnumerable<BrowserCookieEntry> cookies,
            IEnumerable<UserInputEntry> inputs,
            IEnumerable<BrowserExtensionEntry> extensions,
            IEnumerable<StorageEntry> storage,
            IEnumerable<AnalysisEntry> analysis)
        {
            var artifacts = new List<TimelineArtifact>();

            artifacts.AddRange(
                history.Select(h => new TimelineArtifact
                {
                    Time = h.VisitTime,
                    Data = h,
                    Type = ArtifactType.History
                }));

            artifacts.AddRange(
                cookies.Select(c => new TimelineArtifact
                {
                    Time = c.Created,
                    Data = c,
                    Type = ArtifactType.Cookie
                }));

            artifacts.AddRange(
                inputs.Select(i => new TimelineArtifact
                {
                    Time = i.Time,
                    Data = i,
                    Type = ArtifactType.UserInput
                }));

            artifacts.AddRange(
                extensions.Select(e => new TimelineArtifact
                {
                    Time = e.InstallTime,
                    Data = e,
                    Type = ArtifactType.Extension
                }));

            artifacts.AddRange(
                storage.Select(s => new TimelineArtifact
                {
                    Time = s.Time,
                    Data = s,
                    Type = ArtifactType.Storage
                }));

            artifacts.AddRange(
                analysis.Select(a => new TimelineArtifact
                {
                    Time = a.FirstSeen,
                    Data = a,
                    Type = ArtifactType.Anomaly
                }));

            return artifacts
                .OrderBy(a => a.Time)
                .ToList();
        }
    }
}