using NLog;
using RaceControl.Common.Utils;
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
        private const string Episodes = "episodes";
        private const string Sets = "sets";
        private const string Slug = "slug";
        private const string GrandPrixWeekendLiveSlug = "grand-prix-weekend-live";

        private readonly ILogger _logger;
        private readonly IF1TVClient _f1tvClient;

        public ApiService(ILogger logger, IF1TVClient f1tvClient)
        {
            _logger = logger;
            _f1tvClient = f1tvClient;
        }

        public async Task<List<Season>> GetRaceSeasonsAsync()
        {
            _logger.Info("Querying race seasons...");

            var request = _f1tvClient
                .NewRequest(RaceSeason)
                .WithField(Season.UIDField)
                .WithField(Season.NameField)
                .WithField(Season.HasContentField)
                .WithField(Season.YearField)
                .WithField(Season.EventOccurrenceUrlsField)
                .WithFilter(Season.YearField, LarkFilterType.GreaterThan, "2017")
                .OrderBy(Season.YearField, LarkSortDirection.Ascending)
                ;

            var seasons = (await _f1tvClient.GetCollectionAsync<Season>(request)).Objects;
            _logger.Info($"Found {seasons.Count} race seasons.");

            return seasons;
        }

        public async Task<List<Event>> GetLiveEventsAsync()
        {
            _logger.Info("Querying live events...");

            var request = _f1tvClient
                .NewRequest(Sets)
                .WithField(Collection.ItemsField)
                .WithFilter(Slug, LarkFilterType.Equals, GrandPrixWeekendLiveSlug)
                ;

            var collectionList = await _f1tvClient.GetCollectionAsync<Collection>(request);
            var events = new List<Event>();

            foreach (var collection in collectionList.Objects)
            {
                foreach (var collectionItem in collection.Items)
                {
                    events.Add(await GetEventAsync(collectionItem.ContentURL.GetUID()));
                }
            }

            _logger.Info($"Found {events.Count} live events.");

            return events;
        }

        public async Task<Event> GetEventAsync(string eventUID)
        {
            _logger.Info($"Querying event with UID '{eventUID}'...");

            var request = _f1tvClient
                .NewRequest(EventOccurrence, eventUID)
                .WithField(Event.UIDField)
                .WithField(Event.NameField)
                .WithField(Event.OfficialNameField)
                .WithField(Event.SessionOccurrenceUrlsField)
                .WithField(Event.StartDateField)
                .WithField(Event.EndDateField)
                ;

            var evt = await _f1tvClient.GetItemAsync<Event>(request);

            if (evt != null)
            {
                _logger.Info($"Found event '{evt}'.");
            }
            else
            {
                _logger.Info("Event not found.");
            }

            return evt;
        }

        public async Task<Session> GetSessionAsync(string sessionUID)
        {
            _logger.Info($"Querying session with UID '{sessionUID}'...");

            var request = _f1tvClient
                .NewRequest(SessionOccurrence, sessionUID)
                .WithField(Session.UIDField)
                .WithField(Session.NameField)
                .WithField(Session.SessionNameField)
                .WithField(Session.StatusField)
                .WithField(Session.ContentUrlsField)
                .WithField(Session.StartTimeField)
                .WithField(Session.EndTimeField)
                ;

            var session = await _f1tvClient.GetItemAsync<Session>(request);

            if (session != null)
            {
                _logger.Info($"Found session '{session}'.");
            }
            else
            {
                _logger.Info("Session not found.");
            }

            return session;
        }

        public async Task<List<Channel>> GetChannelsAsync(string sessionUID)
        {
            _logger.Info($"Querying channels for session with UID '{sessionUID}'...");

            var request = _f1tvClient
                .NewRequest(SessionOccurrence, sessionUID)
                .WithField(Session.ChannelUrlsField, true)
                .WithSubField(Session.ChannelUrlsField, Channel.UIDField)
                .WithSubField(Session.ChannelUrlsField, Channel.SelfField)
                .WithSubField(Session.ChannelUrlsField, Channel.NameField)
                ;

            var channels = (await _f1tvClient.GetItemAsync<Session>(request)).ChannelUrls;
            _logger.Info($"Found {channels.Count} channels.");

            return channels;
        }

        public async Task<List<VodType>> GetVodTypesAsync()
        {
            _logger.Info("Querying VOD types...");

            var request = _f1tvClient
                .NewRequest(VodTypeTag)
                .WithField(VodType.UIDField)
                .WithField(VodType.NameField)
                .WithField(VodType.ContentUrlsField)
                ;

            var vodTypes = (await _f1tvClient.GetCollectionAsync<VodType>(request)).Objects;
            _logger.Info($"Found {vodTypes.Count} VOD types.");

            return vodTypes;
        }

        public async Task<Episode> GetEpisodeAsync(string episodeUID)
        {
            _logger.Info($"Querying episode with UID '{episodeUID}'...");

            var request = _f1tvClient
                .NewRequest(Episodes, episodeUID)
                .WithField(Episode.UIDField)
                .WithField(Episode.TitleField)
                .WithField(Episode.SubtitleField)
                .WithField(Episode.DataSourceIDField)
                .WithField(Episode.ItemsField)
                ;

            var episode = await _f1tvClient.GetItemAsync<Episode>(request);

            if (episode != null)
            {
                _logger.Info($"Found episode '{episode}'.");
            }
            else
            {
                _logger.Info("Episode not found.");
            }

            return episode;
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