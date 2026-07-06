using System;
using System.Collections.Generic;
using ZIVA_Prototype.Components.Models.Enums;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Timeline
{
    public class TimelineColorService
    {
        private readonly List<int> usedHues = new();

        private readonly Dictionary<string, string> domainColors = new();

        private const int HueStep = 40;
        private const int Saturation = 75;
        private const int Lightness = 45;

        public string GetColor(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "rgb(200,200,200)";

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return "rgb(200,200,200)";

            string domain = uri.Host.ToLower();

            if (domainColors.TryGetValue(domain, out var existing))
                return existing;

            int hue = (usedHues.Count * HueStep) % 360;

            usedHues.Add(hue);

            string color = HslToRgb(
                hue,
                Saturation,
                Lightness
            );

            domainColors[domain] = color;

            return color;
        }

        private string HslToRgb(int h, int s, int l)
        {
            double sat = s / 100.0;
            double light = l / 100.0;

            double c =
                (1 - Math.Abs(2 * light - 1)) * sat;

            double x =
                c * (1 - Math.Abs((h / 60.0) % 2 - 1));

            double m = light - c / 2;

            double r = 0;
            double g = 0;
            double b = 0;

            if (h < 60)
            {
                r = c;
                g = x;
            }
            else if (h < 120)
            {
                r = x;
                g = c;
            }
            else if (h < 180)
            {
                g = c;
                b = x;
            }
            else if (h < 240)
            {
                g = x;
                b = c;
            }
            else if (h < 300)
            {
                r = x;
                b = c;
            }
            else
            {
                r = c;
                b = x;
            }

            int R = (int)((r + m) * 255);
            int G = (int)((g + m) * 255);
            int B = (int)((b + m) * 255);

            return $"rgb({R},{G},{B})";
        }

        public string GetCookieColor(CookieCategory category)
        {
            return category switch
            {
                CookieCategory.BrowserBackground =>
                    "#4da3ff", // blau

                CookieCategory.Authentication =>
                    "#52d273", // grün

                CookieCategory.Tracking =>
                    "#ffb347", // orange

                CookieCategory.Analytics =>
                    "#d98cff", // violett

                CookieCategory.UserBrowsing =>
                    "#6846FC", // dein aktuelles lila

                _ =>
                    "#9e9e9e" // fallback grau
            };
        }

        public string GetAnalysisColor(AnalysisCategory category)
        {
            return category switch
            {
                AnalysisCategory.Information => "#1E88E5", // Blau
                AnalysisCategory.Warning => "#F9A825",     // Gelb
                AnalysisCategory.Anomaly => "#C62828",     // Rot

                _ => "#757575"
            };
        }

        public string GetAnalysisColor(AnalysisEntry analysis)
        {
            return GetAnalysisColor(analysis.Category);
        }
    }
}