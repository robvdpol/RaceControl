using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class ApiService : IApiService
    {
        private const string RaceSeason = "race-season";
        private const string EventOccurrence = "event-occurrence";
        private const string SessionOccurrence = "session-occurrence";

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
                .OrderBy(Season.YearField, LarkSortDirection.Descending)
                ;

            return (await _f1tvClient.GetCollectionAsync<Season>(request)).Objects;
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

        public async Task<string> GetTokenisedUrlForChannelAsync(string token, string channelUrl)
        {
            return (await _f1tvClient.GetTokenisedUrlForChannelAsync(token, channelUrl)).Url;
        }
    }
}