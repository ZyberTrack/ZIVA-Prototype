public class BrowserCookieEntry
{
    public string Host { get; set; }
    public string Name { get; set; }
    public string Value { get; set; } // nur unverschlüsselte Cookies
    public byte[] EncryptedValue { get; set; } // Standardfall Chrome/Edge
    public string Path { get; set; }

    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastAccessed { get; set; }

    public int Position { get; set; } // für Timeline, falls genutzt
}
