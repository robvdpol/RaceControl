namespace RaceControl.Services.Interfaces.F1TV.Api;

public class ApiResponse
{
    [JsonPropertyName("resultCode")]
    public string ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("errorDescription")]
    public string ErrorDescription { get; set; }

    [JsonPropertyName("resultObj")]
    public ResultObj ResultObj { get; set; }

    [JsonPropertyName("systemTime")]
    public long SystemTime { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }
}