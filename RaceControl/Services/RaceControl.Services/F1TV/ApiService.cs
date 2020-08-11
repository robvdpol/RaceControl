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

        private readonly IF1TVClient _f1tvClient;

        public ApiService(IF1TVClient f1tvClient)
        {
            _f1tvClient = f1tvClient;
        }

        public async Task<List<Season>> GetRaceSeasonsAsync()
        {
            var request = _f1tvClient
                .NewRequest(RaceSeason)
                .WithField(Season.UIDField)
                .WithField(Season.NameField)
                .WithField(Season.HasContentField)
                .WithField(Season.YearField)
                .WithField(Season.EventOccurrenceUrlsField)
                .WithFilter(Season.YearField, LarkFilterType.GreaterThan, "2017")
                .WithFilter(Season.YearField, LarkFilterType.LessThan, DateTime.Now.AddYears(1).Year.ToString())
                .OrderBy(Season.YearField, LarkSortDirection.Ascending)
                ;

            return (await _f1tvClient.GetCollectionAsync<Season>(request)).Objects;
        }

        public async Task<List<Event>> GetLiveEventsAsync()
        {
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

            return events;
        }

        public async Task<Event> GetEventAsync(string eventUID)
        {
            var request = _f1tvClient
                .NewRequest(EventOccurrence, eventUID)
                .WithField(Event.UIDField)
                .WithField(Event.NameField)
                .WithField(Event.OfficialNameField)
                .WithField(Event.SessionOccurrenceUrlsField)
                .WithField(Event.StartDateField)
                .WithField(Event.EndDateField)
                ;

            return await _f1tvClient.GetItemAsync<Event>(request);
        }

        public async Task<Session> GetSessionAsync(string sessionUID)
        {
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

            return await _f1tvClient.GetItemAsync<Session>(request);
        }

        public async Task<List<Channel>> GetChannelsAsync(string sessionUID)
        {
            var request = _f1tvClient
                .NewRequest(SessionOccurrence, sessionUID)
                .WithField(Session.ChannelUrlsField, true)
                .WithSubField(Session.ChannelUrlsField, Channel.UIDField)
                .WithSubField(Session.ChannelUrlsField, Channel.SelfField)
                .WithSubField(Session.ChannelUrlsField, Channel.NameField)
                ;

            return (await _f1tvClient.GetItemAsync<Session>(request)).ChannelUrls;
        }

        public async Task<List<VodType>> GetVodTypesAsync()
        {
            var request = _f1tvClient
                .NewRequest(VodTypeTag)
                .WithField(VodType.UIDField)
                .WithField(VodType.NameField)
                .WithField(VodType.ContentUrlsField)
                ;

            return (await _f1tvClient.GetCollectionAsync<VodType>(request)).Objects;
        }

        public async Task<Episode> GetEpisodeAsync(string episodeUID)
        {
            var request = _f1tvClient
                .NewRequest(Episodes, episodeUID)
                .WithField(Episode.UIDField)
                .WithField(Episode.TitleField)
                .WithField(Episode.SubtitleField)
                .WithField(Episode.DataSourceIDField)
                .WithField(Episode.ItemsField)
                ;

            return await _f1tvClient.GetItemAsync<Episode>(request);
        }

        public async Task<string> GetTokenisedUrlForChannelAsync(string token, string channelUrl)
        {
            return (await _f1tvClient.GetTokenisedUrlForChannelAsync(token, channelUrl)).Url;
        }

        public async Task<string> GetTokenisedUrlForAssetAsync(string token, string assetUrl)
        {
            return (await _f1tvClient.GetTokenisedUrlForAssetAsync(token, assetUrl)).Objects.First().TokenisedUrl.Url;
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