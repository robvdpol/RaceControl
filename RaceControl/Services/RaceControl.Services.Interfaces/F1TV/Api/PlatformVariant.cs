namespace RaceControl.Services.Interfaces.F1TV.Api;

public class PlatformVariant
{
    [JsonPropertyName("audioLanguages")]
    public List<AudioLanguage> AudioLanguages { get; set; }

    [JsonPropertyName("cpId")]
    public int CpId { get; set; }

    [JsonPropertyName("videoType")]
    public string VideoType { get; set; }

    [JsonPropertyName("pictureUrl")]
    public string PictureUrl { get; set; }

    [JsonPropertyName("technicalPackages")]
    public List<TechnicalPackage> TechnicalPackages { get; set; }

    [JsonPropertyName("trailerUrl")]
    public string TrailerUrl { get; set; }

    [JsonPropertyName("hasTrailer")]
    public bool HasTrailer { get; set; }
}