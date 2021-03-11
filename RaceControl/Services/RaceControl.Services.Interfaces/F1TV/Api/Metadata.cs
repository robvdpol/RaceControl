using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceControl.Services.Interfaces.F1TV.Api
{
    public class Metadata
    {
        [JsonProperty("emfAttributes")]
        public EmfAttributes EmfAttributes { get; set; }

        [JsonProperty("longDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("directors")]
        public List<string> Directors { get; set; }

        [JsonProperty("isADVAllowed")]
        public bool IsADVAllowed { get; set; }

        [JsonProperty("contractStartDate")]
        public long ContractStartDate { get; set; }

        [JsonProperty("contractEndDate")]
        public long ContractEndDate { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("availableAlso")]
        public List<string> AvailableAlso { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titleBrief")]
        public string TitleBrief { get; set; }

        [JsonProperty("objectType")]
        public string ObjectType { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("contentSubtype")]
        public string ContentSubtype { get; set; }

        [JsonProperty("pcLevel")]
        public int PcLevel { get; set; }

        [JsonProperty("contentId")]
        public int ContentId { get; set; }

        [JsonProperty("starRating")]
        public int StarRating { get; set; }

        [JsonProperty("pictureUrl")]
        public string PictureUrl { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("season")]
        public int Season { get; set; }

        [JsonProperty("uiDuration")]
        public string UiDuration { get; set; }

        [JsonProperty("contentProvider")]
        public string ContentProvider { get; set; }

        [JsonProperty("isLatest")]
        public bool IsLatest { get; set; }

        [JsonProperty("isOnAir")]
        public bool IsOnAir { get; set; }

        [JsonProperty("isEncrypted")]
        public bool IsEncrypted { get; set; }

        [JsonProperty("objectSubtype")]
        public string ObjectSubtype { get; set; }

        [JsonProperty("metadataLanguage")]
        public string MetadataLanguage { get; set; }

        [JsonProperty("pcLevelVod")]
        public string PcLevelVod { get; set; }

        [JsonProperty("isParent")]
        public bool IsParent { get; set; }

        [JsonProperty("leavingSoon")]
        public bool LeavingSoon { get; set; }

        [JsonProperty("pcVodLabel")]
        public string PcVodLabel { get; set; }

        [JsonProperty("isGeoBlocked")]
        public bool IsGeoBlocked { get; set; }

        [JsonProperty("filter")]
        public string Filter { get; set; }

        [JsonProperty("actors")]
        public List<string> Actors { get; set; }

        [JsonProperty("comingSoon")]
        public bool ComingSoon { get; set; }

        [JsonProperty("isPopularEpisode")]
        public bool IsPopularEpisode { get; set; }

        [JsonProperty("primaryCategoryId")]
        public int PrimaryCategoryId { get; set; }

        [JsonProperty("meetingKey")]
        public string MeetingKey { get; set; }

        [JsonProperty("entitlement")]
        public string Entitlement { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        [JsonProperty("videoType")]
        public string VideoType { get; set; }

        [JsonProperty("parentalAdvisory")]
        public string ParentalAdvisory { get; set; }

        [JsonProperty("availableLanguages")]
        public List<AvailableLanguage> AvailableLanguages { get; set; }

        [JsonProperty("additionalStreams")]
        public List<AdditionalStream> AdditionalStreams { get; set; }
    }
}