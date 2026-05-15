using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Import
{
    public class WebDataAutofillImportService
    {
        public async Task<List<WebDataAutofillEntry>> LoadAutofillAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var list = new List<WebDataAutofillEntry>();

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("WebData DB nicht gefunden.");
                    return list;
                }

                try
                {
                    using var connection = new SqliteConnection($"Data Source={filePath};");
                    connection.Open();

                    // 🔍 Alle Tabellen holen
                    var tables = GetTables(connection);

                    // 🔥 1. Klassisches Autofill
                    if (tables.Contains("autofill"))
                    {
                        Console.WriteLine("Lese Tabelle: autofill");

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
                            list.Add(new WebDataAutofillEntry
                            {
                                Name = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                Value = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                DateCreated = FromChromeTime(reader.IsDBNull(2) ? 0 : reader.GetInt64(2)),
                                DateLastUsed = FromChromeTime(reader.IsDBNull(3) ? 0 : reader.GetInt64(3)),
                                Count = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                Position = 0
                            });
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tabelle 'autofill' existiert nicht.");
                    }

                    // 🔥 2. OPTIONAL: Profile (wenn du willst später nutzen)
                    if (tables.Contains("autofill_profiles"))
                    {
                        Console.WriteLine("Tabelle 'autofill_profiles' gefunden (optional nutzbar)");
                    }

                    Console.WriteLine($"Autofill geladen: {list.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Autofill Import Fehler: {ex.Message}");
                }

                return list;
            });
        }

        // 🔍 Holt alle Tabellen der DB
        private HashSet<string> GetTables(SqliteConnection connection)
        {
            var tables = new HashSet<string>();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        // 🕒 Chrome Zeit → DateTime
        private DateTime FromChromeTime(long microseconds)
        {
            if (microseconds <= 0) return DateTime.MinValue;

            var epoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(microseconds / 1000.0).ToLocalTime();
        }
    }
}