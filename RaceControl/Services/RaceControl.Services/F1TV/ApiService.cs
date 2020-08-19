using NLog;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class ApiService : IApiService
    {
        private const string RaceSeason = "race-season";
        private const string EventOccurrence = "event-occurrence";
        private const string SessionOccurrence = "session-occurrence";
        private const string VodTypeTag = "vod-type-tag";

        private readonly ILogger _logger;
        private readonly IF1TVClient _f1tvClient;

        public ApiService(ILogger logger, IF1TVClient f1tvClient)
        {
            _logger = logger;
            _f1tvClient = f1tvClient;
        }

        public async Task<List<Session>> GetLiveSessionsAsync()
        {
            _logger.Info("Querying live sessions...");

            var utcNow = DateTime.UtcNow;

            var request = _f1tvClient
                .NewRequest(SessionOccurrence)
                .WithField(Session.UIDField)
                .WithField(Session.NameField)
                .WithField(Session.SessionNameField)
                .WithField(Session.StatusField)
                .WithField(Session.StartTimeField)
                .WithField(Session.EndTimeField)
                .WithField(Session.EventOccurrenceUrlField)
                .WithFilter(Session.StartTimeField, LarkFilterType.LessThan, utcNow.AddDays(2).ToString("yyyy-MM-ddTHH:mm:ss"))
                .WithFilter(Session.EndTimeField, LarkFilterType.GreaterThan, utcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm:ss"))
                .OrderBy(Session.StartTimeField, LarkSortDirection.Descending)
                ;

            var sessions = (await _f1tvClient.GetCollectionAsync<Session>(request)).Objects;
            _logger.Info($"Found {sessions.Count} live sessions.");

            return sessions;
        }

        public async Task<List<Season>> GetSeasonsAsync()
        {
            _logger.Info("Querying seasons...");

            var request = _f1tvClient
                .NewRequest(RaceSeason)
                .WithField(Season.UIDField)
                .WithField(Season.NameField)
                .WithField(Season.HasContentField)
                .WithField(Season.YearField)
                .WithFilter(Season.YearField, LarkFilterType.GreaterThan, 2017.ToString())
                .WithFilter(Season.HasContentField, LarkFilterType.Equals, true.ToString().ToLower())
                .OrderBy(Season.YearField, LarkSortDirection.Descending)
                ;

            var seasons = (await _f1tvClient.GetCollectionAsync<Season>(request)).Objects;
            _logger.Info($"Found {seasons.Count} seasons.");

            return seasons;
        }

        public async Task<List<VodType>> GetVodTypesAsync()
        {
            _logger.Info("Querying VOD types...");

            var request = _f1tvClient
                .NewRequest(VodTypeTag)
                .WithField(VodType.UIDField)
                .WithField(VodType.NameField)
                ;

            var vodTypes = (await _f1tvClient.GetCollectionAsync<VodType>(request)).Objects;
            _logger.Info($"Found {vodTypes.Count} VOD types.");

            return vodTypes;
        }

        public async Task<List<Event>> GetEventsForSeasonAsync(string seasonUID)
        {
            _logger.Info($"Querying events for season with UID '{seasonUID}'...");

            var request = _f1tvClient
                .NewRequest(EventOccurrence)
                .WithField(Event.UIDField)
                .WithField(Event.NameField)
                .WithField(Event.OfficialNameField)
                .WithField(Event.StartDateField)
                .WithField(Event.EndDateField)
                .WithFilter(Event.RaceSeasonUrlField, LarkFilterType.Equals, seasonUID)
                .OrderBy(Event.StartDateField, LarkSortDirection.Ascending)
                ;

            var events = (await _f1tvClient.GetCollectionAsync<Event>(request)).Objects;
            _logger.Info($"Found {events.Count} events.");

            return events;
        }

        public async Task<List<Session>> GetSessionsForEventAsync(string eventUID)
        {
            _logger.Info($"Querying sessions for event with UID '{eventUID}'...");

            var request = _f1tvClient
                .NewRequest(SessionOccurrence)
                .WithField(Session.UIDField)
                .WithField(Session.NameField)
                .WithField(Session.SessionNameField)
                .WithField(Session.StatusField)
                .WithField(Session.StartTimeField)
                .WithField(Session.EndTimeField)
                .WithField(Session.EventOccurrenceUrlField)
                .WithFilter(Session.EventOccurrenceUrlField, LarkFilterType.Equals, eventUID)
                .OrderBy(Session.StartTimeField, LarkSortDirection.Ascending)
                ;

            var sessions = (await _f1tvClient.GetCollectionAsync<Session>(request)).Objects;
            _logger.Info($"Found {sessions.Count} sessions.");

            return sessions;
        }

        public async Task<List<Channel>> GetChannelsForSessionAsync(string sessionUID)
        {
            _logger.Info($"Querying channels for session with UID '{sessionUID}'...");

            var request = _f1tvClient
                .NewRequest(SessionOccurrence, sessionUID)
                .WithField(Session.ChannelUrlsField, true)
                .WithSubField(Session.ChannelUrlsField, Channel.UIDField)
                .WithSubField(Session.ChannelUrlsField, Channel.SelfField)
                .WithSubField(Session.ChannelUrlsField, Channel.NameField)
                .WithSubField(Session.ChannelUrlsField, Channel.ChannelTypeField)
                ;

            var channels = (await _f1tvClient.GetItemAsync<Session>(request)).ChannelUrls;
            _logger.Info($"Found {channels.Count} channels.");

            return channels;
        }

        public async Task<List<Episode>> GetEpisodesForSessionAsync(string sessionUID)
        {
            _logger.Info($"Querying episodes for session with UID '{sessionUID}'...");

            var request = _f1tvClient
                .NewRequest(SessionOccurrence, sessionUID)
                .WithField(Session.ContentUrlsField, true)
                .WithSubField(Session.ContentUrlsField, Episode.UIDField)
                .WithSubField(Session.ContentUrlsField, Episode.TitleField)
                .WithSubField(Session.ContentUrlsField, Episode.ItemsField)
                ;

            var episodes = (await _f1tvClient.GetItemAsync<Session>(request)).ContentUrls;
            _logger.Info($"Found {episodes.Count} episodes.");

            return episodes;
        }

        public async Task<List<Episode>> GetEpisodesForVodTypeAsync(string vodTypeUID)
        {
            _logger.Info($"Querying episodes for VOD type with UID '{vodTypeUID}'...");

            var request = _f1tvClient
                .NewRequest(VodTypeTag, vodTypeUID)
                .WithField(VodType.ContentUrlsField, true)
                .WithSubField(VodType.ContentUrlsField, Episode.UIDField)
                .WithSubField(VodType.ContentUrlsField, Episode.TitleField)
                .WithSubField(VodType.ContentUrlsField, Episode.ItemsField)
                ;

            var episodes = (await _f1tvClient.GetItemAsync<VodType>(request)).ContentUrls;
            _logger.Info($"Found {episodes.Count} episodes.");

            return episodes;
        }

        public async Task<string> GetTokenisedUrlForChannelAsync(string token, string channelUrl)
        {
            _logger.Info($"Getting tokenised URL for channel with URL '{channelUrl}' using token '{token}'...");
            var url = (await _f1tvClient.GetTokenisedUrlForChannelAsync(token, channelUrl)).Url;
            _logger.Info($"Got tokenised URL '{url}'.");

            return url;
        }

        public async Task<string> GetTokenisedUrlForAssetAsync(string token, string assetUrl)
        {
            _logger.Info($"Getting tokenised URL for asset with URL '{assetUrl}' using token '{token}'...");
            var url = (await _f1tvClient.GetTokenisedUrlForAssetAsync(token, assetUrl)).Objects.First().TokenisedUrl.Url;
            _logger.Info($"Got tokenised URL '{url}'.");

            return url;
        }

        public async Task<string> GetTokenisedUrlAsync(string token, ContentType contentType, string contentUrl)
        {
            switch (contentType)
            {
                case ContentType.Channel:
                    return await GetTokenisedUrlForChannelAsync(token, contentUrl);

                case ContentType.Asset:
                    return await GetTokenisedUrlForAssetAsync(token, contentUrl);
            }

            throw new ArgumentException($"Could not generate tokenised URL for content type '{contentType}'", nameof(contentType));
        }
    }
}