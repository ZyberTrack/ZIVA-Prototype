using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineRenderCacheService
    {
        public List<BrowserCookieEntry> VisibleCookies { get; private set; } = new();

        public List<UserInputEntry> VisibleInputs { get; private set; } = new();

        public List<BrowserExtensionEntry> VisibleExtensions { get; private set; } = new();

        public List<StorageEntry> VisibleStorage { get; private set; } = new();

        public List<DomainEntry> VisibleDomains { get; private set; } = new();

        public List<AnomalyEntry> VisibleAnomalies { get; private set; } = new();


        public void BuildVisibleCaches(
            List<BrowserCookieEntry> cookies,
            List<UserInputEntry> inputs,
            List<BrowserExtensionEntry> extensions,
            List<StorageEntry> storage,
            List<DomainEntry> domains,
            List<AnomalyEntry> anomalies,
            int viewportWidth)
        {
            VisibleCookies = cookies
                .Where(c => IsVisible(c.Position, viewportWidth))
                .ToList();

            VisibleInputs = inputs
                .Where(i => IsVisible(i.Position, viewportWidth))
                .ToList();

            VisibleExtensions = extensions
                .Where(e => IsVisible(e.Position, viewportWidth))
                .ToList();

            VisibleStorage = storage
                .Where(s => IsVisible(s.Position, viewportWidth))
                .ToList();

            VisibleDomains = domains
                .Where(d => IsVisible(d.Position, viewportWidth))
                .ToList();

            VisibleAnomalies = anomalies
                .Where(a => IsVisible(a.Position, viewportWidth))
                .ToList();
        }

        private bool IsVisible(int position, int viewportWidth)
        {
            return position > -50 &&
                   position < viewportWidth - 300;
        }
    }
}
