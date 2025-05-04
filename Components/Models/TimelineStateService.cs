using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ZIVA_Prototype.Components.Models
{
    public class TimelineStateService
    {
        public List<BrowserHistoryEntry> BrowserEntries { get; } = new();

        public void SetBrowserEntries(List<BrowserHistoryEntry> entries)
        {
            BrowserEntries.Clear();
            BrowserEntries.AddRange(entries.OrderBy(e => e.VisitTime));
        }
    }
}

