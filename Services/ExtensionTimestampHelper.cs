using ZIVA_Prototype.Components.Models;

namespace ZIVA_Prototype.Services;

public static class ExtensionTimestampHelper
{
    public static void UpdateInstallTime(
        BrowserExtensionEntry entry,
        DateTime candidate)
    {
        // Ungültige Zeiten ignorieren

        if (candidate.Year < 2015 ||
            candidate > DateTime.UtcNow)
        {
            return;
        }

        // Noch kein gültiger Timestamp

        if (entry.InstallTime == default ||
            entry.InstallTime.Year < 2015)
        {
            entry.InstallTime = candidate;
            return;
        }

        // Frühesten Timestamp behalten

        if (candidate < entry.InstallTime)
        {
            entry.InstallTime = candidate;
        }
    }
}