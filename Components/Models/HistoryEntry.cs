
using Microsoft.Data.Sqlite;



namespace ZIVA_Prototype.Components.Models
{
    public class BrowserHistoryEntry
    {
        public string Url { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string? Title { get; set; }
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
            SELECT url, title, last_visit_time
            FROM urls
            ORDER BY last_visit_time ASC
        ";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var url = reader.GetString(0);
                var title = reader.IsDBNull(1) ? null : reader.GetString(1);
                var lastVisitRaw = reader.GetInt64(2);

                // Chrome-Zeit (Microseconds seit 1601) zu DateTime
                var visitTime = DateTimeOffset.FromUnixTimeSeconds(0)
                    .AddYears(369) // Differenz zwischen 1601 und 1970
                    .AddMilliseconds(lastVisitRaw / 1000 / 10).DateTime;

                entries.Add(new BrowserHistoryEntry
                {
                    Url = url,
                    Title = title,
                    VisitTime = visitTime
                });
            }

            return entries;
        }
    }
}