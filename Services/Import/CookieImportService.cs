using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;

namespace ZIVA_Prototype.Services.Import
{
    public class CookieImportService
    {
        public async Task<List<BrowserCookieEntry>> LoadFromCookieDatabaseAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var cookies = new List<BrowserCookieEntry>();

                using var connection = new SqliteConnection($"Data Source={filePath};");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                SELECT
                    host_key,
                    name,
                    value,
                    encrypted_value,
                    path,
                    creation_utc,
                    expires_utc,
                    last_access_utc
                FROM cookies
                ORDER BY last_access_utc DESC;
            ";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var host = reader.GetString(reader.GetOrdinal("host_key"));
                    var name = reader.GetString(reader.GetOrdinal("name"));
                    var path = reader.GetString(reader.GetOrdinal("path"));

                    var value = reader.IsDBNull(reader.GetOrdinal("value"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("value"));

                    var encryptedValue = reader.IsDBNull(reader.GetOrdinal("encrypted_value"))
                        ? null
                        : (byte[])reader["encrypted_value"];

                    long creationRaw = reader.GetInt64(reader.GetOrdinal("creation_utc"));
                    long expiresRaw = reader.GetInt64(reader.GetOrdinal("expires_utc"));
                    long accessRaw = reader.GetInt64(reader.GetOrdinal("last_access_utc"));

                    DateTime creationTime = FromChromeUtc(creationRaw);
                    DateTime expiresTime = FromChromeUtc(expiresRaw);
                    DateTime accessTime = FromChromeUtc(accessRaw);

                    cookies.Add(new BrowserCookieEntry
                    {
                        Host = host,
                        Name = name,
                        Path = path,
                        Value = value,
                        EncryptedValue = encryptedValue,
                        Created = creationTime,
                        Expires = expiresTime,
                        LastAccessed = accessTime,
                        Position = 0
                    });
                }

                return cookies;
            });
        }

        private DateTime FromChromeUtc(long chromeTime)
        {
            if (chromeTime <= 0) return DateTime.MinValue;

            return DateTime.UnixEpoch
                .AddSeconds(-11644473600) // Unterschied 1601 → 1970
                .AddTicks(chromeTime * 10)
                .ToLocalTime();
        }
    }

}
