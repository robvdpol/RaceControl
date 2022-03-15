namespace RaceControl.Services.Interfaces.F1TV.Api;

public class Metadata
{
    [JsonPropertyName("emfAttributes")]
    public EmfAttributes EmfAttributes { get; set; }

    [JsonPropertyName("longDescription")]
    public string LongDescription { get; set; }

    [JsonPropertyName("year")]
    public string Year { get; set; }

    [JsonPropertyName("directors")]
    public List<string> Directors { get; set; }

    [JsonPropertyName("isADVAllowed")]
    public bool IsADVAllowed { get; set; }

    [JsonPropertyName("contractStartDate")]
    public long ContractStartDate { get; set; }

    [JsonPropertyName("contractEndDate")]
    public long ContractEndDate { get; set; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; }

    [JsonPropertyName("availableAlso")]
    public List<string> AvailableAlso { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("titleBrief")]
    public string TitleBrief { get; set; }

    [JsonPropertyName("objectType")]
    public string ObjectType { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; }

    [JsonPropertyName("contentSubtype")]
    public string ContentSubtype { get; set; }

    [JsonPropertyName("pcLevel")]
    public int PcLevel { get; set; }

    [JsonPropertyName("contentId")]
    public long ContentId { get; set; }

    [JsonPropertyName("starRating")]
    public int StarRating { get; set; }

    [JsonPropertyName("pictureUrl")]
    public string PictureUrl { get; set; }

    [JsonPropertyName("contentType")]
    public string ContentType { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("season")]
    public int Season { get; set; }

    [JsonPropertyName("uiDuration")]
    public string UiDuration { get; set; }

    [JsonPropertyName("contentProvider")]
    public string ContentProvider { get; set; }

    [JsonPropertyName("isLatest")]
    public bool IsLatest { get; set; }

    [JsonPropertyName("isOnAir")]
    public bool IsOnAir { get; set; }

    [JsonPropertyName("isEncrypted")]
    public bool IsEncrypted { get; set; }

    [JsonPropertyName("objectSubtype")]
    public string ObjectSubtype { get; set; }

    [JsonPropertyName("metadataLanguage")]
    public string MetadataLanguage { get; set; }

    [JsonPropertyName("pcLevelVod")]
    public string PcLevelVod { get; set; }

    [JsonPropertyName("isParent")]
    public bool IsParent { get; set; }

    [JsonPropertyName("leavingSoon")]
    public bool LeavingSoon { get; set; }

    [JsonPropertyName("pcVodLabel")]
    public string PcVodLabel { get; set; }

    [JsonPropertyName("isGeoBlocked")]
    public bool IsGeoBlocked { get; set; }

    [JsonPropertyName("filter")]
    public string Filter { get; set; }

    [JsonPropertyName("actors")]
    public List<string> Actors { get; set; }

    [JsonPropertyName("comingSoon")]
    public bool ComingSoon { get; set; }

    [JsonPropertyName("isPopularEpisode")]
    public bool IsPopularEpisode { get; set; }

    [JsonPropertyName("primaryCategoryId")]
    public int PrimaryCategoryId { get; set; }

    [JsonPropertyName("meetingKey")]
    public string MeetingKey { get; set; }

    [JsonPropertyName("entitlement")]
    public string Entitlement { get; set; }

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("videoType")]
    public string VideoType { get; set; }

    [JsonPropertyName("parentalAdvisory")]
    public string ParentalAdvisory { get; set; }

    [JsonPropertyName("availableLanguages")]
    public List<AvailableLanguage> AvailableLanguages { get; set; }

    [JsonPropertyName("additionalStreams")]
    public List<AdditionalStream> AdditionalStreams { get; set; }
}