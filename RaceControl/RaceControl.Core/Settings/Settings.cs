namespace RaceControl.Core.Settings;

public class Settings : BindableBase, ISettings
{
    private static readonly string Filename = FolderUtils.GetLocalApplicationDataFilename("RaceControl.settings.json");

    private readonly ILogger _logger;
    private readonly JsonSerializer _serializer;

    private string _subscriptionToken;
    private string _subscriptionStatus;
    private DateTime? _lastLogin;
    private string _defaultAudioLanguage;
    private VideoQuality _defaultVideoQuality;
    private string _recordingLocation = FolderUtils.GetSpecialFolderPath(Environment.SpecialFolder.Desktop);
    private bool _skipSaveDialog;
    private bool _disableThumbnailTooltips;
    private bool _disableLiveSessionNotification;
    private bool _disableMpvNoBorder;
    private bool _enableMpvAutoSync;
    private string _additionalMpvParameters;
    private string _customMpvPath;
    private string _latestRelease;
    private string _selectedNetworkInterface;
    private ObservableCollection<string> _selectedSeries;

    public Settings(ILogger logger, JsonSerializer serializer)
    {
        _logger = logger;
        _serializer = serializer;
    }

    public string SubscriptionToken
    {
        get => _subscriptionToken;
        set => SetProperty(ref _subscriptionToken, value);
    }

    public string SubscriptionStatus
    {
        get => _subscriptionStatus;
        set => SetProperty(ref _subscriptionStatus, value);
    }

    public DateTime? LastLogin
    {
        get => _lastLogin;
        set => SetProperty(ref _lastLogin, value);
    }

    public string DefaultAudioLanguage
    {
        get => _defaultAudioLanguage;
        set => SetProperty(ref _defaultAudioLanguage, value);
    }

    public VideoQuality DefaultVideoQuality
    {
        get => _defaultVideoQuality;
        set => SetProperty(ref _defaultVideoQuality, value);
    }

    public string RecordingLocation
    {
        get => _recordingLocation;
        set => SetProperty(ref _recordingLocation, value);
    }

    public bool SkipSaveDialog
    {
        get => _skipSaveDialog;
        set => SetProperty(ref _skipSaveDialog, value);
    }

    public bool DisableThumbnailTooltips
    {
        get => _disableThumbnailTooltips;
        set => SetProperty(ref _disableThumbnailTooltips, value);
    }

    public bool DisableLiveSessionNotification
    {
        get => _disableLiveSessionNotification;
        set => SetProperty(ref _disableLiveSessionNotification, value);
    }

    public bool DisableMpvNoBorder
    {
        get => _disableMpvNoBorder;
        set => SetProperty(ref _disableMpvNoBorder, value);
    }

    public bool EnableMpvAutoSync
    {
        get => _enableMpvAutoSync;
        set => SetProperty(ref _enableMpvAutoSync, value);
    }

    public string AdditionalMpvParameters
    {
        get => _additionalMpvParameters;
        set => SetProperty(ref _additionalMpvParameters, value);
    }

    public string CustomMpvPath
    {
        get => _customMpvPath;
        set => SetProperty(ref _customMpvPath, value);
    }

    public string LatestRelease
    {
        get => _latestRelease;
        set => SetProperty(ref _latestRelease, value);
    }

    public string SelectedNetworkInterface
    {
        get => _selectedNetworkInterface;
        set => SetProperty(ref _selectedNetworkInterface, value);
    }

    public ObservableCollection<string> SelectedSeries
    {
        get => _selectedSeries ??= new ObservableCollection<string>();
        set => SetProperty(ref _selectedSeries, value);
    }

    public void Load()
    {
        if (!File.Exists(Filename))
        {
            return;
        }

        try
        {
            using var file = File.OpenText(Filename);
            _serializer.Populate(file, this);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while loading settings.");
        }

        _logger.Info("Settings loaded.");
    }

    public void Save()
    {
        try
        {
            using var file = File.CreateText(Filename);
            _serializer.Serialize(file, this);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while saving settings.");
        }

        _logger.Info("Settings saved.");
    }

    public void ClearSubscriptionToken()
    {
        SubscriptionToken = null;
        SubscriptionStatus = null;
        LastLogin = null;
    }

    public void UpdateSubscriptionToken(string subscriptionToken, string subscriptionStatus)
    {
        SubscriptionToken = subscriptionToken;
        SubscriptionStatus = subscriptionStatus;
        LastLogin = DateTime.UtcNow;
    }

    public bool HasValidSubscriptionToken()
    {
        return !string.IsNullOrWhiteSpace(SubscriptionToken) && LastLogin.HasValue && LastLogin.Value >= DateTime.UtcNow.AddDays(-4);
    }
}