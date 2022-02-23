namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class PlatformVariant
    {
        [JsonProperty("audioLanguages")]
        public List<AudioLanguage> AudioLanguages { get; set; }

        [JsonProperty("cpId")]
        public int CpId { get; set; }

        [JsonProperty("videoType")]
        public string VideoType { get; set; }

        [JsonProperty("pictureUrl")]
        public string PictureUrl { get; set; }

        [JsonProperty("technicalPackages")]
        public List<TechnicalPackage> TechnicalPackages { get; set; }

        [JsonProperty("trailerUrl")]
        public string TrailerUrl { get; set; }

        [JsonProperty("hasTrailer")]
        public bool HasTrailer { get; set; }
    }
}