// Soll später für Benutzer Configurierbar sein bzw in einem Dev Mode.

using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Configuration
{
    public static class TimelineConstants
    {
        public static int HeavyClusterThreshold { get; set; } = 20;

        public static int DomainSummaryThreshold { get; set; } = 5;

        public static int MaxImportDays { get; set; } = 7;
    }
}
