namespace RaceControl.Services.Interfaces.F1TV.Api;

public class PlayToken
{
    public PlayToken(string domain, string path, string value)
    {
        Domain = domain;
        Path = path;
        Value = value;
    }

    public string Domain { get; }
    public string Path { get; }
    public string Value { get; }

    public string GetCookieString()
    {
        return $"Cookie: playToken={Value}; domain={Domain}; path={Path}";
    }
}