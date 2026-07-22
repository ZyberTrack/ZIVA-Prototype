using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using ZIVA_Prototype.Components.Models.Timeline;
using ZIVA_Prototype.Components.Models.Enums;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.IO;

namespace ZIVA_Prototype.Services.Import
{
    public class CookieImportService
    {
        public async Task<List<BrowserCookieEntry>> LoadFromCookieDatabaseAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var cookies = new List<BrowserCookieEntry>();

                byte[]? masterKey = GetChromeMasterKey();

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

                    var encryptedValue = reader.IsDBNull(reader.GetOrdinal("encrypted_value"))
                        ? null
                        : (byte[])reader["encrypted_value"];

                    string? value = reader.IsDBNull(reader.GetOrdinal("value"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("value"));

                    /*if (string.IsNullOrEmpty(value) &&
                        encryptedValue != null &&
                        masterKey != null)
                    {
                        value = DecryptChromeCookie(encryptedValue, masterKey);
                    }*/

                    if (string.IsNullOrEmpty(value) && encryptedValue != null)
                    {
                        string prefix = encryptedValue.Length >= 3
                            ? Encoding.ASCII.GetString(encryptedValue, 0, 3)
                            : "UNKNOWN";

                        if (masterKey == null)
                        {
                            value = $"MASTERKEY_NULL ({prefix})";
                        }
                        else
                        {
                            value = DecryptChromeCookie(encryptedValue, masterKey);
                        }
                    }


                    long creationRaw = reader.GetInt64(reader.GetOrdinal("creation_utc"));
                    long expiresRaw = reader.GetInt64(reader.GetOrdinal("expires_utc"));
                    long accessRaw = reader.GetInt64(reader.GetOrdinal("last_access_utc"));

                    DateTime creationTime = FromChromeUtc(creationRaw);
                    DateTime expiresTime = FromChromeUtc(expiresRaw);
                    DateTime accessTime = FromChromeUtc(accessRaw);

                    var cookie = new BrowserCookieEntry
                    {
                        Host = host,
                        Name = name,
                        Path = path,
                        Value = value,
                        EncryptedValue = encryptedValue,
                        Created = creationTime,
                        Expires = expiresTime,
                        LastAccessed = accessTime,
                        Position = 0,
                        Category = DetectCategory(host),
                    };

                    if (cookie.EncryptedValue != null && cookie.EncryptedValue.Length > 0)
                    {
                        cookie.IsEncrypted = true;

                        if (cookie.Value != null &&
                            !cookie.Value.StartsWith("v20 -> ERROR") &&
                            !cookie.Value.StartsWith("v10 -> ERROR") &&
                            !cookie.Value.StartsWith("MASTERKEY_NULL"))
                        {
                            cookie.CouldDecrypt = true;
                        }
                    }

                    cookies.Add(cookie);
                }

                return cookies;
            });
        }

        private byte[]? GetChromeMasterKey()
        {
            try
            {
                string localStatePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Google\Chrome\User Data\Local State");

                if (!File.Exists(localStatePath))
                    return null;

                string json = File.ReadAllText(localStatePath);

                using JsonDocument doc = JsonDocument.Parse(json);

                string encryptedKey =
                    doc.RootElement
                       .GetProperty("os_crypt")
                       .GetProperty("encrypted_key")
                       .GetString()!;

                byte[] keyBytes = Convert.FromBase64String(encryptedKey);

                // "DPAPI" Prefix entfernen
                keyBytes = keyBytes.Skip(5).ToArray();

                return ProtectedData.Unprotect(
                    keyBytes,
                    null,
                    DataProtectionScope.CurrentUser);
            }
            catch
            {
                return null;
            }
        }

        private string DecryptChromeCookie(byte[] encryptedValue, byte[] masterKey)
        {
            try
            {
                if (encryptedValue == null || encryptedValue.Length == 0)
                    return string.Empty;

                // Alte Chrome-Versionen (DPAPI)
                if (!(encryptedValue[0] == (byte)'v' &&
                      encryptedValue[1] == (byte)'1'))
                {
                    byte[] decrypted = ProtectedData.Unprotect(
                        encryptedValue,
                        null,
                        DataProtectionScope.CurrentUser);

                    return Encoding.UTF8.GetString(decrypted);
                }

                // Chrome v10 / v11
                // Format:
                // [3 Byte Version][12 Byte Nonce][Ciphertext][16 Byte Tag]

                byte[] nonce = encryptedValue
                    .Skip(3)
                    .Take(12)
                    .ToArray();

                byte[] cipherText = encryptedValue
                    .Skip(15)
                    .Take(encryptedValue.Length - 15 - 16)
                    .ToArray();

                byte[] tag = encryptedValue
                    .Skip(encryptedValue.Length - 16)
                    .ToArray();

                byte[] plaintext = new byte[cipherText.Length];

                using var aes = new AesGcm(masterKey);

                aes.Decrypt(
                    nonce,
                    cipherText,
                    tag,
                    plaintext);

                return Encoding.UTF8.GetString(plaintext);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private CookieCategory DetectCategory(
    string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return CookieCategory.Unknown;
            }

            host =
                host.ToLowerInvariant();

            // =====================================================
            // BROWSER / INFRASTRUCTURE
            // =====================================================

            string[] browserInfra =
            {
        "ogs.google.com",
        "accounts.google.com",
        "clients.google.com",
        "clients2.google.com",
        "update.googleapis.com",
        "safebrowsing.googleapis.com",
        "optimizationguide-pa.googleapis.com",
        "gstatic.com",
        "googleapis.com",
        "edge.microsoft.com",
        "msedge.net",
        "firefox.com",
        "mozilla.net"
    };

            if (browserInfra.Any(x =>
                    host.Contains(x)))
            {
                return CookieCategory.BrowserBackground;
            }

            // =====================================================
            // TRACKING
            // =====================================================

            string[] tracking =
            {
        "doubleclick.net",
        "googlesyndication.com",
        "googleadservices.com",
        "facebook.com",
        "analytics",
        "tracker"
    };

            if (tracking.Any(x =>
                    host.Contains(x)))
            {
                return CookieCategory.Tracking;
            }

            // =====================================================
            // DEFAULT
            // =====================================================

            return CookieCategory.UserBrowsing;
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
