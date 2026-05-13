using System.Globalization;
using System.Text.RegularExpressions;

namespace ZIVA_Prototype.Services
{
    public class TimestampExtractor
    {
        // =====================================================
        // REGEX
        // =====================================================

        private readonly Regex IsoRegex =
            new Regex(
                @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}",
                RegexOptions.Compiled);

        private readonly Regex LargeNumberRegex =
            new Regex(
                @"\b\d{10,20}\b",
                RegexOptions.Compiled);

        // =====================================================
        // MAIN
        // =====================================================

        public DateTime? Extract(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            // =========================================
            // ISO8601
            // =========================================

            var iso =
                TryExtractIso(text);

            if (iso != null)
                return iso;

            // =========================================
            // NUMERIC TIMESTAMPS
            // =========================================

            var numeric =
                TryExtractNumeric(text);

            if (numeric != null)
                return numeric;

            return null;
        }

        // =====================================================
        // ISO8601
        // =====================================================

        private DateTime? TryExtractIso(string text)
        {
            try
            {
                var match =
                    IsoRegex.Match(text);

                if (!match.Success)
                    return null;

                if (DateTime.TryParse(
                    match.Value,
                    null,
                    DateTimeStyles.AdjustToUniversal,
                    out var dt))
                {
                    return dt;
                }
            }
            catch
            {
            }

            return null;
        }

        // =====================================================
        // NUMERIC TIMESTAMPS
        // =====================================================

        private DateTime? TryExtractNumeric(string text)
        {
            try
            {
                var matches =
                    LargeNumberRegex.Matches(text);

                foreach (Match match in matches)
                {
                    if (!long.TryParse(
                        match.Value,
                        out long value))
                    {
                        continue;
                    }

                    // =================================
                    // UNIX SECONDS
                    // =================================

                    if (match.Value.Length == 10)
                    {
                        try
                        {
                            return DateTimeOffset
                                .FromUnixTimeSeconds(value)
                                .UtcDateTime;
                        }
                        catch
                        {
                        }
                    }

                    // =================================
                    // UNIX MILLISECONDS
                    // =================================

                    if (match.Value.Length == 13)
                    {
                        try
                        {
                            return DateTimeOffset
                                .FromUnixTimeMilliseconds(value)
                                .UtcDateTime;
                        }
                        catch
                        {
                        }
                    }

                    // =================================
                    // WEBKIT / CHROMIUM
                    // microseconds since 1601
                    // =================================

                    if (match.Value.Length >= 16)
                    {
                        try
                        {
                            DateTime epoch =
                                new DateTime(
                                    1601,
                                    1,
                                    1,
                                    0,
                                    0,
                                    0,
                                    DateTimeKind.Utc);

                            double milliseconds =
                                value / 1000.0;

                            var result =
                                epoch.AddMilliseconds(
                                    milliseconds);

                            // sanity check
                            if (result.Year >= 2000 &&
                                result.Year <= 2100)
                            {
                                return result;
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
