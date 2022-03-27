using System.Net;

namespace RaceControl.Services.F1TV;

public class ApiService : IApiService
{
    private const int F1TVArchiveStartYear = 1981;
    private const string DefaultStreamType = StreamTypeKeys.BigScreenHls;
    private const string SearchVodUrl = "2.0/R/ENG/" + DefaultStreamType + "/ALL/PAGE/SEARCH/VOD/F1_TV_Pro_Annual/2";

    private static readonly Uri ApiEndpointUri = new(Constants.ApiEndpointUrl);

    private readonly ILogger _logger;
    private readonly RestClient _restClient;

    public ApiService(ILogger logger, RestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
    }

    public List<Series> GetSeries()
    {
        return new()
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
            }
        };
    }

    public List<Season> GetSeasons()
    {
        _logger.Info("Querying seasons...");

        var seasons = new List<Season>();

        for (var year = DateTime.Now.Year; year >= F1TVArchiveStartYear; year--)
        {
            seasons.Add(new Season
            {
                Year = year,
                Name = $"{year} season"
            });
        }

        return seasons;
    }

    public async Task<List<Session>> GetLiveSessionsAsync()
    {
        _logger.Info("Querying live sessions...");

        var apiResponse = await QueryLiveSessionsAsync().ConfigureAwait(false);

        return apiResponse.ResultObj.Containers
            .SelectMany(c1 => c1.RetrieveItems.ResultObj.Containers
                .Where(c2 => c2.Metadata?.ContentType == "VIDEO" && c2.Metadata?.ContentSubtype == "LIVE")
                .Select(CreateSession))
            .DistinctBy(s => s.ContentID)
            .OrderBy(s => s.SeriesUID)
            .ThenByDescending(s => s.SessionIndex)
            .ToList();
    }

    public async Task<List<Event>> GetEventsForSeasonAsync(Season season)
    {
        _logger.Info($"Querying events for season '{season.Name}'...");

        var apiResponse = await QuerySeasonEventsAsync(season.Year).ConfigureAwait(false);
        var events = apiResponse != null ? apiResponse.ResultObj.Containers.Select(CreateEvent).ToList() : new List<Event>();

        // Show events for current season in reverse
        if (season.Year == DateTime.UtcNow.Year)
        {
            events.Reverse();
        }

        return events;
    }

    public async Task<List<Episode>> GetEpisodesForSeasonAsync(Season season)
    {
        _logger.Info($"Querying episodes for season '{season.Name}'...");

        var apiResponse = await QuerySeasonEpisodesAsync(season.Year).ConfigureAwait(false);

        return apiResponse.ResultObj.Containers
            .Select(CreateEpisode)
            .OrderByDescending(e => e.SessionIndex)
            .ToList();
    }

    public async Task<List<Session>> GetSessionsForEventAsync(Event evt)
    {
        _logger.Info($"Querying sessions for event with UID '{evt.UID}'...");

        var apiResponse = await QueryEventVideosAsync(evt.UID).ConfigureAwait(false);

        return apiResponse.ResultObj.Containers
            .Where(c => c.Metadata.ContentType == "VIDEO")
            .Where(c => c.Metadata.ContentSubtype == "REPLAY" || c.Metadata.ContentSubtype == "LIVE")
            .Select(CreateSession)
            .OrderBy(s => s.SeriesUID)
            .ThenByDescending(s => s.SessionIndex)
            .ToList();
    }

    public async Task<List<Episode>> GetEpisodesForEventAsync(Event evt)
    {
        _logger.Info($"Querying episodes for event with UID '{evt.UID}'...");

        var apiResponse = await QueryEventVideosAsync(evt.UID).ConfigureAwait(false);

        return apiResponse.ResultObj.Containers
            .Where(c => c.Metadata.ContentType == "VIDEO")
            .Where(c => c.Metadata.ContentSubtype != "REPLAY" && c.Metadata.ContentSubtype != "LIVE")
            .Select(CreateEpisode)
            .OrderByDescending(e => e.SessionIndex)
            .ToList();
    }

    public async Task<List<Channel>> GetChannelsForSessionAsync(Session session)
    {
        _logger.Info($"Querying channels for session with UID '{session.UID}'...");

        var apiResponse = await QuerySessionChannelsAsync(session.ContentID).ConfigureAwait(false);
        var metadata = apiResponse.ResultObj.Containers.First().Metadata;

        if (metadata.AdditionalStreams != null && metadata.AdditionalStreams.Any())
        {
            return metadata.AdditionalStreams
                .Select(s => new Channel
                {
                    Name = s.Type == ChannelTypes.Onboard ? $"{s.DriverFirstName} {s.DriverLastName}" : s.Title,
                    ChannelType = s.Type,
                    PlaybackUrl = s.PlaybackUrl
                })
                .ToList();
        }

        return new List<Channel>
        {
            new()
            {
                Name = ChannelNames.Wif,
                ChannelType = ChannelTypes.Wif,
                PlaybackUrl = GetPlaybackUrl(metadata.ContentId)
            }
        };
    }

    public async Task<List<string>> GetVodGenresAsync()
    {
        _logger.Info("Querying vod genres...");

        var showsResponse = await QueryPageAsync(410).ConfigureAwait(false);
        var documentariesResponse = await QueryPageAsync(413).ConfigureAwait(false);
        var archiveResponse = await QueryPageAsync(493).ConfigureAwait(false);

        return showsResponse.ResultObj.Containers
            .Union(documentariesResponse.ResultObj.Containers)
            .Union(archiveResponse.ResultObj.Containers)
            .Where(c => c.RetrieveItems.ResultObj.Containers != null)
            .SelectMany(c1 => c1.RetrieveItems.ResultObj.Containers
                .SelectMany(c2 => c2.Metadata.Genres))
            .Distinct()
            .OrderBy(g => g)
            .ToList();
    }

    public async Task<List<Episode>> GetEpisodesForGenreAsync(string genre)
    {
        _logger.Info($"Querying episodes for vod genre '{genre}'...");

        var apiResponse = await QueryGenreVideosAsync(genre).ConfigureAwait(false);

        return apiResponse.ResultObj.Containers
            .Where(c => c.Metadata.ContentType == "VIDEO")
            .Where(c => c.Metadata.ContentSubtype != "LIVE")
            .Select(CreateEpisode)
            .OrderByDescending(e => e.ContractStartDate)
            .ToList();
    }

    public async Task<string> GetTokenisedUrlAsync(string subscriptionToken, IPlayableContent playableContent)
    {
        return await GetTokenisedUrlAsync(subscriptionToken, playableContent, DefaultStreamType).ConfigureAwait(false);
    }

    public async Task<string> GetTokenisedUrlAsync(string subscriptionToken, IPlayableContent playableContent, string streamType)
    {
        _logger.Info($"Getting tokenised URL for content-type '{playableContent.ContentType}' and content-URL '{playableContent.ContentUrl}'...");

        return (await QueryTokenisedUrlAsync(subscriptionToken, streamType, playableContent.ContentUrl).ConfigureAwait(false)).ResultObj.Url;
    }

    public async Task<PlayToken> GetPlayTokenAsync(string streamUrl)
    {
        var restRequest = new RestRequest(streamUrl, Method.Head);
        var restResponse = await _restClient.HeadAsync(restRequest).ConfigureAwait(false);
        var playToken = restResponse.Cookies?.FirstOrDefault(cookie => string.Equals(cookie.Name, "playToken", StringComparison.OrdinalIgnoreCase));

        return playToken != null ? new PlayToken(playToken.Domain, playToken.Path, playToken.Value) : null;
    }

    private async Task<ApiResponse> QueryLiveSessionsAsync()
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, $"2.0/R/ENG/{DefaultStreamType}/ALL/PAGE/395/F1_TV_Pro_Annual/2"));

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
    }

    private async Task<ApiResponse> QuerySeasonEventsAsync(int year)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, SearchVodUrl));
        restRequest.AddQueryParameter("orderBy", "meeting_End_Date");
        restRequest.AddQueryParameter("sortOrder", "asc");
        restRequest.AddQueryParameter("filter_objectSubtype", "Meeting");
        restRequest.AddQueryParameter("filter_season", year.ToString());
        restRequest.AddQueryParameter("filter_orderByFom", "Y");
        restRequest.AddQueryParameter("maxResults", "100");

        try
        {
            return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            throw;
        }
    }

    private async Task<ApiResponse> QuerySeasonEpisodesAsync(int year)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, SearchVodUrl));
        restRequest.AddQueryParameter("orderBy", "meeting_Number");
        restRequest.AddQueryParameter("sortOrder", "asc");
        restRequest.AddQueryParameter("filter_year", year.ToString());
        restRequest.AddQueryParameter("filter_orderByFom", "Y");
        restRequest.AddQueryParameter("maxResults", "100");

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
    }

    private async Task<ApiResponse> QueryEventVideosAsync(string meetingKey)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, SearchVodUrl));
        restRequest.AddQueryParameter("orderBy", "meeting_End_Date");
        restRequest.AddQueryParameter("sortOrder", "asc");
        restRequest.AddQueryParameter("filter_MeetingKey", meetingKey);
        restRequest.AddQueryParameter("filter_orderByFom", "Y");
        restRequest.AddQueryParameter("maxResults", "100");

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
    }

    private async Task<ApiResponse> QuerySessionChannelsAsync(long contentID)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, $"3.0/R/ENG/{DefaultStreamType}/ALL/CONTENT/VIDEO/{contentID}/F1_TV_Pro_Annual/2"));

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
    }

    private async Task<ApiResponse> QueryPageAsync(int page)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, $"2.0/R/ENG/{DefaultStreamType}/ALL/PAGE/{page}/F1_TV_Pro_Annual/2"));

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
    }

    private async Task<ApiResponse> QueryGenreVideosAsync(string genre)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, SearchVodUrl));
        restRequest.AddQueryParameter("orderBy", "meeting_Number");
        restRequest.AddQueryParameter("sortOrder", "asc");
        restRequest.AddQueryParameter("filter_genres", genre);
        restRequest.AddQueryParameter("filter_orderByFom", "Y");
        restRequest.AddQueryParameter("maxResults", "100");

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
    }

    private async Task<ApiResponse> QueryTokenisedUrlAsync(string subscriptionToken, string streamType, string contentUrl)
    {
        var restRequest = new RestRequest(new Uri(ApiEndpointUri, $"/2.0/R/ENG/{streamType}/ALL/{contentUrl}"));
        restRequest.AddHeader("ascendontoken", subscriptionToken);

        return await _restClient.GetAsync<ApiResponse>(restRequest).ConfigureAwait(false);
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
        var seriesUID = container.Properties?.FirstOrDefault()?.Series;

        return new()
        {
            UID = container.Id.ToString(),
            ContentID = container.Metadata.ContentId,
            ContentType = container.Metadata.ContentType,
            ContentSubtype = container.Metadata.ContentSubtype,
            ShortName = GetSessionShortName(container.Metadata.TitleBrief, seriesUID),
            LongName = container.Metadata.Title,
            SeriesUID = seriesUID,
            ThumbnailUrl = GetThumbnailUrl(container.Metadata.PictureUrl),
            StartDate = container.Metadata.EmfAttributes.SessionStartDate.GetDateTimeFromEpoch(),
            EndDate = container.Metadata.EmfAttributes.SessionEndDate.GetDateTimeFromEpoch(),
            SessionIndex = container.Metadata.EmfAttributes.SessionIndex
        };
    }

    private static Episode CreateEpisode(Container container)
    {
        var seriesUID = container.Properties?.FirstOrDefault()?.Series;

        return new()
        {
            UID = container.Id.ToString(),
            ContentID = container.Metadata.ContentId,
            ContentType = container.Metadata.ContentType,
            ContentSubtype = container.Metadata.ContentSubtype,
            ShortName = container.Metadata.TitleBrief,
            LongName = container.Metadata.Title,
            SeriesUID = seriesUID,
            PlaybackUrl = GetPlaybackUrl(container.Metadata.ContentId),
            ThumbnailUrl = GetThumbnailUrl(container.Metadata.PictureUrl),
            StartDate = container.Metadata.EmfAttributes.SessionStartDate.GetDateTimeFromEpoch(),
            EndDate = container.Metadata.EmfAttributes.SessionEndDate.GetDateTimeFromEpoch(),
            ContractStartDate = container.Metadata.ContractStartDate.GetDateTimeFromEpoch(),
            ContractEndDate = container.Metadata.ContractEndDate.GetDateTimeFromEpoch(),
            SessionIndex = container.Metadata.EmfAttributes.SessionIndex
        };
    }

    private static string GetSessionShortName(string titleBrief, string seriesUID)
    {
        if (string.IsNullOrWhiteSpace(seriesUID) || seriesUID == SeriesIds.Formula1 || !SeriesNames.ShortNames.TryGetValue(seriesUID, out var shortNames) || shortNames.Any(shortName => titleBrief.Contains(shortName, StringComparison.OrdinalIgnoreCase)))
        {
            return titleBrief;
        }

        return $"{shortNames.First()} {titleBrief}";
    }

    private static string GetPlaybackUrl(long contentId)
    {
        return $"CONTENT/PLAY?contentId={contentId}";
    }

    private static string GetThumbnailUrl(string pictureUrl)
    {
        return !string.IsNullOrWhiteSpace(pictureUrl) ? $"{Constants.ImageUrl}/{pictureUrl}?w=354&h=199&q=HI&o=L" : null;
    }
}