using Microsoft.Data.Sqlite;
using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services;

public class FaviconsImportService
{
    public async Task<List<FaviconEntry>>
        LoadFromFaviconsAsync(
            string faviconsDbPath)
    {
        return await Task.Run(() =>
        {
            var list =
                new List<FaviconEntry>();

            if (!File.Exists(faviconsDbPath))
            {
                Console.WriteLine(
                    $"❌ Favicons DB nicht gefunden: {faviconsDbPath}");

                return list;
            }

            try
            {
                using var connection =
                    new SqliteConnection(
                        $"Data Source={faviconsDbPath}");

                connection.Open();

                Console.WriteLine(
                    $"✅ Favicons DB geöffnet: {faviconsDbPath}");

                // =====================================================
                // DEBUG: Tabellen anzeigen
                // =====================================================

                using (var debugCmd = connection.CreateCommand())
                {
                    debugCmd.CommandText =
                    @"
                    SELECT name
                    FROM sqlite_master
                    WHERE type='table'
                    ";

                    using var debugReader =
                        debugCmd.ExecuteReader();

                    Console.WriteLine("📋 Tabellen in Favicons DB:");

                    while (debugReader.Read())
                    {
                        Console.WriteLine(
                            $"   -> {debugReader.GetString(0)}");
                    }
                }

                // =====================================================
                // FAVICONS LADEN
                // =====================================================

                var command =
                    connection.CreateCommand();

                command.CommandText =
                @"
                SELECT
                    icon_mapping.page_url,
                    favicons.url,
                    favicon_bitmaps.last_updated
                FROM icon_mapping

                LEFT JOIN favicons
                    ON icon_mapping.icon_id = favicons.id

                LEFT JOIN favicon_bitmaps
                    ON favicons.id = favicon_bitmaps.icon_id
                ";

                using var reader =
                    command.ExecuteReader();

                int rowCount = 0;

                while (reader.Read())
                {
                    try
                    {
                        rowCount++;

                        string pageUrl =
                            reader.IsDBNull(0)
                            ? ""
                            : reader.GetString(0);

                        string iconUrl =
                            reader.IsDBNull(1)
                            ? ""
                            : reader.GetString(1);

                        long chromeTime =
                            reader.IsDBNull(2)
                            ? 0
                            : reader.GetInt64(2);

                        DateTime time =
                            ChromeTimeToDateTime(
                                chromeTime);

                        if (string.IsNullOrWhiteSpace(pageUrl)
                            && string.IsNullOrWhiteSpace(iconUrl))
                        {
                            continue;
                        }

                        list.Add(
                            new FaviconEntry
                            {
                                Time = time,
                                PageUrl = pageUrl,
                                IconUrl = iconUrl
                            });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"❌ Fehler beim Lesen eines Favicons:");

                        Console.WriteLine(ex);
                    }
                }

                Console.WriteLine(
                    $"✅ Favicons Rows gelesen: {rowCount}");

                Console.WriteLine(
                    $"✅ Favicons importiert: {list.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "❌ Fehler beim Öffnen/Laden der Favicons DB:");

                Console.WriteLine(ex);
            }

            return list;
        });
    }

    private DateTime ChromeTimeToDateTime(
        long chromeTime)
    {
        try
        {
            if (chromeTime <= 0)
                return DateTime.UtcNow;

            return new DateTime(
                    1601,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc)
                .AddTicks(chromeTime * 10)
                .ToLocalTime();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ ChromeTime Fehler: {ex}");

            return DateTime.UtcNow;
        }
    }
}