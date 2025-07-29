using Microsoft.Data.Sqlite;
using System;

namespace ZIVA_Prototype.Components.Models
{
    public class BrowserHistoryEntry
    {
        public string Url { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string? Title { get; set; }
        public string? ReferrerUrl { get; set; }
        public string? ReferrerTitle { get; set; }
        public double Position { get; set; }
    }

    public class HistoryImportService
    {
        public List<BrowserHistoryEntry> LoadFromHistoryDatabase(string filePath)
        {
            var entries = new List<BrowserHistoryEntry>();

            using var connection = new SqliteConnection($"Data Source={filePath};");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT
                    v.id AS visit_id,
                    datetime(v.visit_time / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS datetime_local,
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
                ORDER BY v.visit_time DESC;
            ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var url = reader.GetString(reader.GetOrdinal("current_url"));
                var title = reader.IsDBNull(reader.GetOrdinal("current_title")) ? null : reader.GetString(reader.GetOrdinal("current_title"));
                var visitTimeRaw = reader.GetString(reader.GetOrdinal("datetime_local"));
                var referrerUrl = reader.IsDBNull(reader.GetOrdinal("referrer_url")) ? null : reader.GetString(reader.GetOrdinal("referrer_url"));
                var referrerTitle = reader.IsDBNull(reader.GetOrdinal("referrer_title")) ? null : reader.GetString(reader.GetOrdinal("referrer_title"));

                var visitTime = DateTime.Parse(visitTimeRaw);

                entries.Add(new BrowserHistoryEntry
                {
                    Url = url,
                    Title = title,
                    VisitTime = visitTime,
                    ReferrerUrl = referrerUrl,
                    ReferrerTitle = referrerTitle,
                    Position = 0 // Position can be set later if needed
                });
            }

            return entries;
        }
    }
}
