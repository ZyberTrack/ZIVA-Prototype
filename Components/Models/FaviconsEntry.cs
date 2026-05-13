using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models
{
    public class FaviconEntry
    {
        public DateTime Time { get; set; }

        public string PageUrl { get; set; } = "";

        public string IconUrl { get; set; } = "";

        public int Position { get; set; }
    }
}
