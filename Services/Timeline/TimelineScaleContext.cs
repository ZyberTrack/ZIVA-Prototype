using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineScaleContext
    {
        public DateTime BaseTime { get; set; }

        public double InitialOffsetPx { get; set; }

        public int Spacing { get; set; }

        public int CurrentZoomIndex { get; set; }

        public (int seconds, TimeUnit unit)[] ZoomLevels
        { get; set; } = default!;
    }
}