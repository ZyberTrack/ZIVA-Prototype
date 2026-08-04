using System;
using System.Collections.Generic;
using System.Text;

namespace ZIVA_Prototype.Components.Models.Timeline;

public class HistoryRange
{
    public DateTime Min { get; set; }

    public DateTime Max { get; set; }

    public double TotalDays =>
        (Max - Min).TotalDays;
}
