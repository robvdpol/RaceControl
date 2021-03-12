using LazyCache;
using NLog;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Lark;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaceControl.Services.F1TV
{
    public class NewApiService : IApiService
    {
        private const int F1TVLaunchYear = 2018;

        private readonly ILogger _logger;
        private readonly IAppCache _cache;
        private readonly IF1TVClient _client;
        private readonly Func<IRestClient> _restClientFactory;

        public NewApiService(ILogger logger, IAppCache cache, IF1TVClient client, Func<IRestClient> restClientFactory)
        {
            _logger = logger;
            _cache = cache;
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
                    UID = "FORMULA 1",
                    Name = "Formula 1"
                },
                new()
                {
                    UID = "FORMULA 2",
                    Name = "Formula 2"
                },
                new()
                {
                    UID = "FORMULA 3",
                    Name = "Formula 3"
                },
                new()
                {
                    UID = "PORSCHE",
                    Name = "Porsche Supercup"
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

            var apiResponse = await QueryEventSessionsAsync(evt.UID);

            return apiResponse.ResultObj.Containers
                .Where(c => c.Metadata.ContentType == "VIDEO")
                .Where(c => c.Metadata.ContentSubtype == "REPLAY" || c.Metadata.ContentSubtype == "LIVE")
                .Select(CreateSession)
                .ToList();
        }

        public async Task<List<Channel>> GetChannelsForSessionAsync(Session session)
        {
            _logger.Info($"Querying channels for session with UID '{session.UID}'...");

            var apiResponse = await QuerySessionChannelsAsync(session.ContentID);
            var metadata = apiResponse.ResultObj.Containers.First().Metadata;

            var channels = new List<Channel>
            {
                // Add the world feed seperately
                new()
                {
                    Name = "WIF",
                    ChannelType = ChannelTypes.Wif,
                    PlaybackUrl = $"CONTENT/PLAY?contentId={metadata.ContentId}"
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

        public async Task<List<Episode>> GetEpisodesForSessionAsync(string sessionUID)
        {
            _logger.Info($"Querying episodes for session with UID '{sessionUID}'...");

            //var request = _client
            //    .NewRequest(SessionOccurrence, sessionUID)
            //    .WithField(Session.ContentUrlsField, true)
            //    .WithSubField(Session.ContentUrlsField, Episode.UIDField)
            //    .WithSubField(Session.ContentUrlsField, Episode.TitleField)
            //    .WithSubField(Session.ContentUrlsField, Episode.ItemsField)
            //    .WithSubField(Session.ContentUrlsField, Episode.ImageUrlsField, true)
            //    .WithSubSubField(Session.ContentUrlsField, Episode.ImageUrlsField, Image.UIDField)
            //    .WithSubSubField(Session.ContentUrlsField, Episode.ImageUrlsField, Image.TitleField)
            //    .WithSubSubField(Session.ContentUrlsField, Episode.ImageUrlsField, Image.ImageTypeField)
            //    .WithSubSubField(Session.ContentUrlsField, Episode.ImageUrlsField, Image.UrlField)
            //    ;

            //return (await _cache.GetOrAddAsync($"{nameof(NewApiService)}-{nameof(GetEpisodesForSessionAsync)}-{sessionUID}", () => _client.GetItemAsync<Session>(request), DateTimeOffset.Now.AddMinutes(1))).ContentUrls;

            return new List<Episode>();
        }

        public async Task<Episode> GetEpisodeAsync(string episodeUID)
        {
            _logger.Info($"Querying episode with UID '{episodeUID}'...");

            var request = _client
                .NewRequest("episodes", episodeUID)
                .WithField(Episode.UIDField)
                .WithField(Episode.TitleField)
                .WithField(Episode.ItemsField)
                .WithField(Episode.ImageUrlsField, true)
                .WithSubField(Episode.ImageUrlsField, Image.UIDField)
                .WithSubField(Episode.ImageUrlsField, Image.TitleField)
                .WithSubField(Episode.ImageUrlsField, Image.ImageTypeField)
                .WithSubField(Episode.ImageUrlsField, Image.UrlField)
                ;

            return await _cache.GetOrAddAsync($"{nameof(NewApiService)}-{nameof(GetEpisodeAsync)}-{episodeUID}", () => _client.GetItemAsync<Episode>(request));
        }

        public async Task<Driver> GetDriverAsync(string driverUID)
        {
            _logger.Info($"Querying driver with UID '{driverUID}'...");

            var request = _client
                .NewRequest("driver-occurrence", driverUID)
                .WithField(Driver.UIDField)
                .WithField(Driver.NameField)
                .WithField(Driver.ImageUrlsField, true)
                .WithSubField(Driver.ImageUrlsField, Image.UIDField)
                .WithSubField(Driver.ImageUrlsField, Image.TitleField)
                .WithSubField(Driver.ImageUrlsField, Image.ImageTypeField)
                .WithSubField(Driver.ImageUrlsField, Image.UrlField)
                ;

            return await _cache.GetOrAddAsync($"{nameof(NewApiService)}-{nameof(GetDriverAsync)}-{driverUID}", () => _client.GetItemAsync<Driver>(request));
        }

        public async Task<string> GetTokenisedUrlAsync(string token, IPlayableContent playableContent)
        {
            _logger.Info($"Getting tokenised URL for content-type '{playableContent.ContentType}' and content-URL '{playableContent.ContentUrl}' using token '{token}'...");

            return playableContent.ContentType switch
            {
                ContentType.Channel => (await QueryTokenisedUrlAsync(token, playableContent.ContentUrl)).ResultObj.Url,
                ContentType.Asset => (await _client.GetTokenisedUrlForAssetAsync(token, playableContent.ContentUrl)).Objects.First().TokenisedUrl.Url,
                ContentType.Backup => (await _client.GetBackupStream()).StreamManifest,
                _ => throw new ArgumentException($"Could not generate tokenised URL for unsupported content-type '{playableContent.ContentType}'.", nameof(playableContent))
            };
        }

        private async Task<ApiResponse> QueryLiveSessionsAsync()
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest("2.0/R/ENG/BIG_SCREEN_DASH/ALL/PAGE/395/F1_TV_Pro_Annual/2", DataFormat.Json);

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QuerySeasonEventsAsync(int year)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest("2.0/R/ENG/BIG_SCREEN_DASH/ALL/PAGE/SEARCH/VOD/F1_TV_Pro_Annual/2", DataFormat.Json);
            restRequest.AddQueryParameter("filter_objectSubtype", "Meeting");
            restRequest.AddQueryParameter("orderBy", "meeting_End_Date");
            restRequest.AddQueryParameter("sortOrder", "asc");
            restRequest.AddQueryParameter("filter_season", year.ToString());
            restRequest.AddQueryParameter("filter_orderByFom", "Y");

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QueryEventSessionsAsync(string meetingKey)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest("2.0/R/ENG/BIG_SCREEN_DASH/ALL/PAGE/SEARCH/VOD/F1_TV_Pro_Annual/2", DataFormat.Json);
            //restRequest.AddQueryParameter("filter_objectSubtype", "Replay");
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

            var restRequest = new RestRequest($"2.0/R/ENG/BIG_SCREEN_DASH/ALL/CONTENT/VIDEO/{contentID}/F1_TV_Pro_Annual/2", DataFormat.Json);

            return await restClient.GetAsync<ApiResponse>(restRequest);
        }

        private async Task<ApiResponse> QueryTokenisedUrlAsync(string token, string contentUrl)
        {
            var restClient = _restClientFactory();
            restClient.BaseUrl = new Uri(Constants.NewApiEndpointUrl);

            var restRequest = new RestRequest($"/1.0/R/ENG/BIG_SCREEN_DASH/ALL/{contentUrl}", DataFormat.Json);
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
                Name = container.Metadata.TitleBrief,
                SessionName = container.Metadata.Title,
                SeriesUID = container.Properties.First().Series
            };

            if (!string.IsNullOrWhiteSpace(container.Metadata.PictureUrl))
            {
                session.ThumbnailUrl = $"{Constants.ImageUrl}/{container.Metadata.PictureUrl}?w=354&h=199&q=HI&o=L";
            }

            return session;
        }
    }
}