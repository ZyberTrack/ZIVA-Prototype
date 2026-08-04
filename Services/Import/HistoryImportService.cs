using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using ZIVA_Prototype.Components.Models.Timeline;

namespace ZIVA_Prototype.Services.Import
{
    public class HistoryImportService
    {
        public async Task<List<BrowserHistoryEntry>>LoadFromHistoryDatabaseAsync(string filePath, ImportRange? range = null)
        {
            return await Task.Run(() =>
            {
                var entries = new List<BrowserHistoryEntry>();

                using var connection = new SqliteConnection($"Data Source={filePath};");
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
                SELECT
                    v.id AS visit_id,
                    v.visit_time,
                    u.url AS current_url,
                    u.title AS current_title,
                    v.transition,
                    v.from_visit,
                    f.url AS referrer_url,
                    f.title AS referrer_title
                FROM visits v
                LEFT JOIN urls u ON v.url = u.id
                LEFT JOIN visits vf ON v.from_visit = vf.id
                LEFT JOIN urls f ON vf.url = f.id
                ";

                if (range != null)
                {
                    command.CommandText += @"
                    WHERE v.visit_time >= @from
                    ";

                    command.Parameters.AddWithValue("@from", ToChromeUtc(range.From));
                    command.Parameters.AddWithValue("@to", ToChromeUtc(range.To));
                }

                command.CommandText += @"
                ORDER BY v.visit_time DESC;
                ";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var url = reader.GetString(reader.GetOrdinal("current_url"));
                    var title = reader.IsDBNull(reader.GetOrdinal("current_title")) ? null : reader.GetString(reader.GetOrdinal("current_title"));
                    //var visitTimeRaw = reader.GetString(reader.GetOrdinal("datetime_local"));
                    var referrerUrl = reader.IsDBNull(reader.GetOrdinal("referrer_url")) ? null : reader.GetString(reader.GetOrdinal("referrer_url"));
                    var referrerTitle = reader.IsDBNull(reader.GetOrdinal("referrer_title")) ? null : reader.GetString(reader.GetOrdinal("referrer_title"));

                    long visitTimeRaw = reader.GetInt64(reader.GetOrdinal("visit_time"));

                    var visitTime = FromChromeUtc(visitTimeRaw);

                    string host = string.Empty;
                    if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    {
                        host = uri.Host;
                    }

                    entries.Add(new BrowserHistoryEntry
                    {
                        Url = url,
                        Host = host,
                        Title = title,
                        VisitTime = visitTime,
                        ReferrerUrl = referrerUrl,
                        ReferrerTitle = referrerTitle,
                        Position = 0 // Position can be set later if needed
                    });
                }

                return entries;
            });
        }

        public async Task<HistoryRange> GetHistoryRangeAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using var connection =
                    new SqliteConnection($"Data Source={filePath};");

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
            SELECT
                MIN(visit_time) AS min_time,
                MAX(visit_time) AS max_time
            FROM visits;
        ";

                using var reader = command.ExecuteReader();

                if (!reader.Read() ||
                    reader.IsDBNull(reader.GetOrdinal("min_time")) ||
                    reader.IsDBNull(reader.GetOrdinal("max_time")))
                {
                    throw new Exception("No browser history found.");
                }

                long minRaw = reader.GetInt64(reader.GetOrdinal("min_time"));
                long maxRaw = reader.GetInt64(reader.GetOrdinal("max_time"));

                return new HistoryRange
                {
                    Min = FromChromeUtc(minRaw),
                    Max = FromChromeUtc(maxRaw)
                };
            });
        }

        private static DateTime FromChromeUtc(long chromeTime)
        {
            if (chromeTime <= 0)
                return DateTime.MinValue;

            return DateTime.UnixEpoch
                .AddSeconds(-11644473600)
                .AddTicks(chromeTime * 10)
                .ToLocalTime();
        }

        private static long ToChromeUtc(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime() - DateTime.UnixEpoch)
                .Ticks / 10 + 11644473600000000;
        }
    }

}
