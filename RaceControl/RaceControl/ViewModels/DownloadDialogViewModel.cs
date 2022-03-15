namespace RaceControl.ViewModels;

public class DownloadDialogViewModel : DialogViewModelBase
{
    private readonly ISettings _settings;
    private readonly IApiService _apiService;

    private IPlayableContent _playableContent;
    private string _filename;

    public DownloadDialogViewModel(
        ILogger logger,
        ISettings settings,
        IApiService apiService,
        IMediaDownloader mediaDownloader)
        : base(logger)
    {
        _settings = settings;
        _apiService = apiService;
        MediaDownloader = mediaDownloader;
    }

    public override string Title => "Download";

    public IMediaDownloader MediaDownloader { get; }

    public IPlayableContent PlayableContent
    {
        get => _playableContent;
        set => SetProperty(ref _playableContent, value);
    }

    public string Filename
    {
        get => _filename;
        set => SetProperty(ref _filename, value);
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        PlayableContent = parameters.GetValue<IPlayableContent>(ParameterNames.Content);
        Filename = parameters.GetValue<string>(ParameterNames.Filename);
        StartDownloadAsync().Await(DownloadStarted, DownloadFailed);
    }

    public override void OnDialogClosed()
    {
        base.OnDialogClosed();
        MediaDownloader.Dispose();
    }

    private async Task StartDownloadAsync()
    {
        var streamUrl = await _apiService.GetTokenisedUrlAsync(_settings.SubscriptionToken, PlayableContent);
        var playToken = await _apiService.GetPlayTokenAsync(streamUrl);
        await MediaDownloader.StartDownloadAsync(streamUrl, PlayableContent.IsLive, playToken, Filename);
    }

    private void DownloadStarted()
    {
        base.OnDialogOpened(null);
        MediaDownloader.SetDownloadStatus(DownloadStatus.Downloading);
    }

    private void DownloadFailed(Exception ex)
    {
        base.OnDialogOpened(null);
        MediaDownloader.SetDownloadStatus(DownloadStatus.Failed);
        HandleCriticalError(ex);
    }
}