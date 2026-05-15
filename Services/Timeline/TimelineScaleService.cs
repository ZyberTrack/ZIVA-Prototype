using System;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Timeline
{
    public static class TimelineScaleService
    {
        public static double GetPixelPerUnit(
            int spacing,
            (int seconds, TimeUnit unit)[] zoomLevels,
            int currentZoomIndex)
        {
            // spacing bleibt fix
            double fixedSpacing = spacing;

            // aktuelle Zoomstufe
            double valueInSeconds =
                zoomLevels[currentZoomIndex].seconds;

            TimeUnit unit =
                zoomLevels[currentZoomIndex].unit;

            // Dynamisch die Einheit anpassen
            while (true)
            {
                // Berechne value in der aktuellen Einheit
                double valueInCurrentUnit = valueInSeconds;

                switch (unit)
                {
                    case TimeUnit.Seconds:
                        break;

                    case TimeUnit.Minutes:
                        valueInCurrentUnit /= 60.0;
                        break;

                    case TimeUnit.Hours:
                        valueInCurrentUnit /= 3600.0;
                        break;

                    case TimeUnit.Days:
                        valueInCurrentUnit /= 86400.0;
                        break;

                    case TimeUnit.Months:
                        valueInCurrentUnit /= (30 * 86400.0);
                        break;

                    case TimeUnit.Years:
                        valueInCurrentUnit /= (365 * 86400.0);
                        break;
                }

                // Prüfen: passt valueInCurrentUnit zu spacing?
                if (valueInCurrentUnit <= fixedSpacing ||
                    unit == TimeUnit.Years)
                    break;

                // sonst nächstgrößere Einheit wählen
                unit = unit switch
                {
                    TimeUnit.Seconds => TimeUnit.Minutes,
                    TimeUnit.Minutes => TimeUnit.Hours,
                    TimeUnit.Hours => TimeUnit.Days,
                    TimeUnit.Days => TimeUnit.Months,
                    TimeUnit.Months => TimeUnit.Years,
                    TimeUnit.Years => TimeUnit.Years
                };
            }

            // Pixel pro Einheit = spacing / valueInCurrentUnit
            double pixelPerUnit =
                fixedSpacing / valueInSeconds;

            return pixelPerUnit;
        }

        public static TimeSpan GetTimeSpanFromZoomLevel(
            (int seconds, TimeUnit unit) zoomLevel)
        {
            return zoomLevel.unit switch
            {
                TimeUnit.Seconds =>
                    TimeSpan.FromSeconds(zoomLevel.seconds),

                TimeUnit.Minutes =>
                    TimeSpan.FromMinutes(
                        zoomLevel.seconds / 60.0),

                TimeUnit.Hours =>
                    TimeSpan.FromHours(
                        zoomLevel.seconds / 3600.0),

                TimeUnit.Days =>
                    TimeSpan.FromDays(
                        zoomLevel.seconds / 86400.0),

                TimeUnit.Months =>
                    TimeSpan.FromDays(
                        zoomLevel.seconds / 86400.0),

                TimeUnit.Years =>
                    TimeSpan.FromDays(
                        zoomLevel.seconds / 86400.0),

                _ =>
                    TimeSpan.FromSeconds(zoomLevel.seconds)
            };
        }

        public static double TimeToPixel(
            DateTime t,
            TimelineScaleContext context)
        {
            double secondsFromBase =
                (t - context.BaseTime).TotalSeconds;

            double pixelsPerSecond =
                GetPixelPerUnit(
                    context.Spacing,
                    context.ZoomLevels,
                    context.CurrentZoomIndex);

            return context.InitialOffsetPx +
                   secondsFromBase * pixelsPerSecond;
        }

        public static DateTime PixelToTime(
            double px,
            TimelineScaleContext context)
        {
            double pixelsFromStart =
                px - context.InitialOffsetPx;

            double secondsPerPixel =
                (double)context.ZoomLevels[context.CurrentZoomIndex].seconds
                / context.Spacing;

            double secondsFromBase =
                pixelsFromStart * secondsPerPixel;

            return context.BaseTime.AddSeconds(secondsFromBase);
        }

        public static double GetUnitDistance(
            DateTime from,
            DateTime to,
            TimeUnit currentTimeUnit)
        {
            return currentTimeUnit switch
            {
                TimeUnit.Seconds =>
                    (to - from).TotalSeconds,

                TimeUnit.Minutes =>
                    (to - from).TotalMinutes,

                TimeUnit.Hours =>
                    (to - from).TotalHours,

                TimeUnit.Days =>
                    (to - from).TotalDays,

                TimeUnit.Months =>
                    ((to.Year - from.Year) * 12
                    + to.Month - from.Month)
                    + (to.Day - from.Day) / 30.0,

                TimeUnit.Years =>
                    (to.Year - from.Year)
                    + (to.DayOfYear - from.DayOfYear)
                    / 365.0,

                _ => 0
            };
        }
    }
}