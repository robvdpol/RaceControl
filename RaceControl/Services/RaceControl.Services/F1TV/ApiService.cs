using NLog;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.F1TV.Constants;
using RaceControl.Services.Interfaces.Lark;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class ApiService : IApiService
    {
        private const int F1TVLaunchYear = 2018;
        private const string StreamType = "BIG_SCREEN_HLS";

        private readonly ILogger _logger;
        private readonly IF1TVClient _client;
        private readonly Func<IRestClient> _restClientFactory;

        public ApiService(ILogger logger, IF1TVClient client, Func<IRestClient> restClientFactory)
        {
            _logger = logger;
            _client = client;
            _restClientFactory = restClientFactory;
        }

        public async Task<List<Session>> GetLiveSessionsAsync()
        {
            _logger.Info("Querying live sessions...");

            var apiResponse = await QueryLiveSessionsAsync();

            return apiResponse.ResultObj.Containers
                .SelectMany(c1 => c1.RetrieveItems.ResultObj.Containers
                    .Where(c2 => c2.Metadata.ContentType == "VIDEO" && c2.Metadata.ContentSubtype == "LIVE")
                    .Select(CreateSession))
                .ToList();
        }

        public List<Season> GetSeasons()
        {
            _logger.Info("Querying seasons...");

            var seasons = new List<Season>();

            for (var year = DateTime.Now.Year; year >= F1TVLaunchYear; year--)
            {
                seasons.Add(new Season
                {
                    Year = year,
                    Name = $"{year} season"
                });
            }

            return seasons;
        }

        public List<Series> GetSeries()
        {
            _logger.Info("Querying series...");

            return new List<Series>
            {
                new()
                {
                    UID = SeriesIds.Formula1,
                    Name = SeriesNames.Formula1
                },
                new()
                {
                    UID = SeriesIds.Formula2,
                    Name = SeriesNames.Formula2
                },
                new()
                {
                    UID = SeriesIds.Formula3,
                    Name = SeriesNames.Formula3
                },
                new()
                {
                    UID = SeriesIds.PorscheSupercup,
                    Name = SeriesNames.PorscheSupercup
                },
            };
        }

        public async Task<List<VodType>> GetVodTypesAsync()
        {
            _logger.Info("Querying VOD types...");

            var request = _client
                .NewRequest("vod-type-tag")
                .WithField(VodType.UIDField)
                .WithField(VodType.NameField)
                .WithField(VodType.ContentUrlsField)
            ;

            return (await _client.GetCollectionAsync<VodType>(request)).Objects;
        }

        public async Task<List<Event>> GetEventsForSeasonAsync(Season season)
        {
            _logger.Info($"Querying events for season '{season.Name}'...");

            var apiResponse = await QuerySeasonEventsAsync(season.Year.GetValueOrDefault());

            return apiResponse.ResultObj.Containers
                .Select(CreateEvent)
                .ToList();
        }

        public async Task<List<Session>> GetSessionsForEventAsync(Event evt)
        {
            _logger.Info($"Querying sessions for event with UID '{evt.UID}'...");

            var apiResponse = await QueryEventVideosAsync(evt.UID);

            return apiResponse.ResultObj.Containers
                .Where(c => c.Metadata.ContentType == "VIDEO")
                .Where(c => c.Metadata.ContentSubtype == "REPLAY" || c.Metadata.ContentSubtype == "LIVE")
                .Select(CreateSession)
                .ToList();
        }

        public async Task<List<Episode>> GetEpisodesForEventAsync(Event evt)
        {
            _logger.Info($"Querying sessions for event with UID '{evt.UID}'...");

            var apiResponse = await QueryEventVideosAsync(evt.UID);

            return apiResponse.ResultObj.Containers
                .Where(c => c.Metadata.ContentType == "VIDEO")
                .Where(c => c.Metadata.ContentSubtype != "REPLAY" && c.Metadata.ContentSubtype != "LIVE")
                .Select(CreateEpisode)
                .ToList();
        }

        public async Task<List<Channel>> GetChannelsForSessionAsync(Session session)
        {
            _logger.Info($"Querying channels for session with UID '{session.UID}'...");

            var apiResponse = await QuerySessionChannelsAsync(session.ContentID);
            var metadata = apiResponse.ResultObj.Containers.First().Metadata;

            var channels = new List<Channel>
            {
                new()
                {
                    // Add the world feed seperately
                    Name = ChannelNames.Wif,
                    ChannelType = ChannelTypes.Wif,
                    PlaybackUrl = GetPlaybackUrl(metadata.ContentId)
                }
            };

            if (metadata.AdditionalStreams != null && metadata.AdditionalStreams.Any())
            {
                channels.AddRange(metadata.AdditionalStreams
                      .Select(s => new Channel
                      {
                          Name = s.Type == "obc" ? $"{s.DriverFirstName} {s.DriverLastName}" : s.Title,
                          ChannelType = s.Type,
                          PlaybackUrl = s.PlaybackUrl
                      })
                      .ToList());
            }

            return channels;
        }

        public async Task<string> GetTokenisedUrlAsync(string token, IPlayableContent playableContent)
        {
            _logger.Info($"Getting tokenised URL for content-type '{playableContent.ContentType}' and content-URL '{playableContent.ContentUrl}' using token '{token}'...");

            return playableContent.ContentType switch
            {
                ContentType.Channel => (await QueryTokenisedUrlAsync(token, playableContent.ContentUrl)).ResultObj.Url,
                ContentType.Asset => (await QueryTokenisedUrlAsync(token, playableContent.ContentUrl)).ResultObj.Url,
                ContentType.Backup => (await _client.GetBackupStream()).StreamManifest,
                _ => throw new ArgumentException($"Could not generate tokenised URL for unsupported content-type '{playableContent.ContentType}'.", nameof(playableContent))
            };
        }

        private async Task<ApiResponse> QueryLiveSessionsAsync()
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest($"2.0/R/ENG/{StreamType}/ALL/PAGE/395/F1_TV_Pro_Annual/2", DataFormat.Json);

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QuerySeasonEventsAsync(int year)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest($"2.0/R/ENG/{StreamType}/ALL/PAGE/SEARCH/VOD/F1_TV_Pro_Annual/2", DataFormat.Json);
            restRequest.AddQueryParameter("filter_objectSubtype", "Meeting");
            restRequest.AddQueryParameter("orderBy", "meeting_End_Date");
            restRequest.AddQueryParameter("sortOrder", "asc");
            restRequest.AddQueryParameter("filter_season", year.ToString());
            restRequest.AddQueryParameter("filter_orderByFom", "Y");

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QueryEventVideosAsync(string meetingKey)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest($"2.0/R/ENG/{StreamType}/ALL/PAGE/SEARCH/VOD/F1_TV_Pro_Annual/2", DataFormat.Json);
            restRequest.AddQueryParameter("orderBy", "session_index");
            restRequest.AddQueryParameter("sortOrder", "asc");
            restRequest.AddQueryParameter("filter_MeetingKey", meetingKey);
            restRequest.AddQueryParameter("filter_orderByFom", "Y");

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QuerySessionChannelsAsync(int contentID)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest($"2.0/R/ENG/{StreamType}/ALL/CONTENT/VIDEO/{contentID}/F1_TV_Pro_Annual/2", DataFormat.Json);

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QueryTokenisedUrlAsync(string token, string contentUrl)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest($"/1.0/R/ENG/{StreamType}/ALL/{contentUrl}", DataFormat.Json);
            restRequest.AddHeader("ascendontoken", token);

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private static Event CreateEvent(Container container)
        {
            return new()
            {
                UID = container.Metadata.EmfAttributes.MeetingKey,
                Name = container.Metadata.EmfAttributes.MeetingName,
                StartDate = container.Metadata.EmfAttributes.MeetingStartDate,
                EndDate = container.Metadata.EmfAttributes.MeetingEndDate
            };
        }

        private static Session CreateSession(Container container)
        {
            var session = new Session
            {
                UID = container.Id,
                ContentID = container.Metadata.ContentId,
                ContentType = container.Metadata.ContentType,
                ContentSubtype = container.Metadata.ContentSubtype,
                ShortName = container.Metadata.TitleBrief,
                LongName = container.Metadata.Title,
                SeriesUID = container.Properties.First().Series,
                ThumbnailUrl = GetThumbnailUrl(container.Metadata.PictureUrl)
            };

            return session;
        }

        private static Episode CreateEpisode(Container container)
        {
            var episode = new Episode
            {
                UID = container.Id,
                ContentID = container.Metadata.ContentId,
                ContentType = container.Metadata.ContentType,
                ContentSubtype = container.Metadata.ContentSubtype,
                ShortName = container.Metadata.TitleBrief,
                LongName = container.Metadata.Title,
                SeriesUID = container.Properties.First().Series,
                PlaybackUrl = GetPlaybackUrl(container.Metadata.ContentId),
                ThumbnailUrl = GetThumbnailUrl(container.Metadata.PictureUrl)
            };

            return episode;
        }

        private static string GetPlaybackUrl(int contentId)
        {
            return $"CONTENT/PLAY?contentId={contentId}";
        }

        private static string GetThumbnailUrl(string pictureUrl)
        {
            return !string.IsNullOrWhiteSpace(pictureUrl) ? $"{Constants.ImageUrl}/{pictureUrl}?w=354&h=199&q=HI&o=L" : null;
        }
    }
}