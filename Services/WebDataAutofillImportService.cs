using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services
{
    public class WebDataAutofillImportService
    {
        public async Task<List<WebDataAutofillEntry>> LoadAutofillAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var list = new List<WebDataAutofillEntry>();

                using var connection = new SqliteConnection($"Data Source={filePath};");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT
                    name,
                    value,
                    date_created,
                    date_last_used,
                    count
                FROM autofill
                ORDER BY date_last_used DESC;
            ";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string value = reader.IsDBNull(1) ? "" : reader.GetString(1);

                    long createdRaw = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                    long lastUsedRaw = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                    int count = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);

                    DateTime created = FromChromeUtcAutofill(createdRaw);
                    DateTime lastUsed = FromChromeUtcAutofill(lastUsedRaw);

                    list.Add(new WebDataAutofillEntry
                    {
                        Name = name,
                        Value = value,
                        DateCreated = created,
                        DateLastUsed = lastUsed,
                        Count = count,
                        Position = 0
                    });
                }

                return list;
            });
        }

        private DateTime FromChromeUtcAutofill(long unixSeconds)
        {
            if (unixSeconds <= 0) return DateTime.MinValue;
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
        }
    }

}
