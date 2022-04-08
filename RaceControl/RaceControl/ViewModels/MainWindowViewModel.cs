using GoogleCast;
using GoogleCast.Channels;
using GoogleCast.Models.Media;
using Microsoft.Win32;
using Action = System.Action;
using Timer = System.Timers.Timer;

namespace RaceControl.ViewModels;

// ReSharper disable UnusedMember.Global
// ReSharper disable once UnusedType.Global
public class MainWindowViewModel : ViewModelBase, ICloseWindow
{
    private const int PlayPauseHotKeyID = 9000;

    private readonly IExtendedDialogService _dialogService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IApiService _apiService;
    private readonly IGithubService _githubService;
    private readonly INumberGenerator _numberGenerator;
    private readonly IDeviceLocator _deviceLocator;
    private readonly ISender _sender;
    private readonly SoundPlayer _liveSessionPlayer;
    private readonly object _refreshTimerLock = new();

    private ICommand _loadedCommand;
    private ICommand _closingCommand;
    private ICommand _closeCommand;
    private ICommand _openLogFileCommand;
    private ICommand _mouseMoveCommand;
    private ICommand _previewKeyDownCommand;
    private ICommand _keyDownCommand;
    private ICommand _seasonSelectionChangedCommand;
    private ICommand _eventSelectionChangedCommand;
    private ICommand _liveSessionSelectionChangedCommand;
    private ICommand _sessionSelectionChangedCommand;
    private ICommand _vodGenreSelectionChangedCommand;
    private ICommand _watchContentCommand;
    private ICommand _watchContentInVlcCommand;
    private ICommand _watchContentInMpvCommand;
    private ICommand _watchContentInMpcCommand;
    private ICommand _castContentCommand;
    private ICommand _copyContentUrlCommand;
    private ICommand _downloadContentCommand;
    private ICommand _saveVideoDialogLayoutCommand;
    private ICommand _importVideoDialogLayoutCommand;
    private ICommand _openVideoDialogLayoutCommand;
    private ICommand _scanReceiversCommand;
    private ICommand _receiverSelectionChangedCommand;
    private ICommand _audioTrackSelectionChangedCommand;
    private ICommand _logOutCommand;
    private ICommand _requestNavigateCommand;

    private HwndSource _hwndSource;
    private string _episodeFilterText;
    private string _vlcExeLocation;
    private string _mpvExeLocation;
    private string _mpcExeLocation;
    private ObservableCollection<Season> _seasons;
    private ObservableCollection<Series> _series;
    private ObservableCollection<Event> _events;
    private ObservableCollection<Session> _sessions;
    private ObservableCollection<Session> _liveSessions;
    private ObservableCollection<string> _vodGenres;
    private ObservableCollection<IPlayableContent> _channels;
    private ObservableCollection<IPlayableContent> _episodes;
    private ObservableCollection<NetworkInterface> _networkInterfaces;
    private ObservableCollection<IReceiver> _receivers;
    private ObservableCollection<Track> _audioTracks;
    private Season _selectedSeason;
    private Event _selectedEvent;
    private Session _selectedLiveSession;
    private Session _selectedSession;
    private string _selectedVodGenre;
    private NetworkInterface _selectedNetworkInterface;
    private IReceiver _selectedReceiver;
    private Track _selectedAudioTrack;
    private Timer _refreshTimer;

    public MainWindowViewModel(
        ILogger logger,
        IExtendedDialogService dialogService,
        IEventAggregator eventAggregator,
        IApiService apiService,
        IGithubService githubService,
        INumberGenerator numberGenerator,
        IDeviceLocator deviceLocator,
        ISender sender,
        ISettings settings,
        IVideoDialogLayout videoDialogLayout)
        : base(logger)
    {
        _dialogService = dialogService;
        _eventAggregator = eventAggregator;
        _apiService = apiService;
        _githubService = githubService;
        _numberGenerator = numberGenerator;
        _deviceLocator = deviceLocator;
        _sender = sender;
        Settings = settings;
        _liveSessionPlayer = new SoundPlayer("livesession.wav");
        VideoDialogLayout = videoDialogLayout;
        EpisodesView = CollectionViewSource.GetDefaultView(Episodes);
        EpisodesView.Filter = EpisodesViewFilter;
    }

    public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
    public ICommand ClosingCommand => _closingCommand ??= new DelegateCommand(ClosingExecute);
    public ICommand CloseCommand => _closeCommand ??= new DelegateCommand(CloseExecute);
    public ICommand OpenLogFileCommand => _openLogFileCommand ??= new DelegateCommand(OpenLogFileExecute);
    public ICommand MouseMoveCommand => _mouseMoveCommand ??= new DelegateCommand(MouseMoveExecute);
    public ICommand PreviewKeyDownCommand => _previewKeyDownCommand ??= new DelegateCommand<KeyEventArgs>(PreviewKeyDownExecute);
    public ICommand KeyDownCommand => _keyDownCommand ??= new DelegateCommand<KeyEventArgs>(KeyDownExecute);
    public ICommand SeasonSelectionChangedCommand => _seasonSelectionChangedCommand ??= new DelegateCommand(SeasonSelectionChangedExecute);
    public ICommand EventSelectionChangedCommand => _eventSelectionChangedCommand ??= new DelegateCommand(EventSelectionChangedExecute);
    public ICommand LiveSessionSelectionChangedCommand => _liveSessionSelectionChangedCommand ??= new DelegateCommand(LiveSessionSelectionChangedExecute);
    public ICommand SessionSelectionChangedCommand => _sessionSelectionChangedCommand ??= new DelegateCommand(SessionSelectionChangedExecute);
    public ICommand VodGenreSelectionChangedCommand => _vodGenreSelectionChangedCommand ??= new DelegateCommand(VodGenreSelectionChangedExecute);
    public ICommand WatchContentCommand => _watchContentCommand ??= new DelegateCommand<IPlayableContent>(WatchContentExecute);
    public ICommand WatchContentInVlcCommand => _watchContentInVlcCommand ??= new DelegateCommand<IPlayableContent>(WatchContentInVlcExecute, CanWatchContentInVlcExecute).ObservesProperty(() => VlcExeLocation);
    public ICommand WatchContentInMpvCommand => _watchContentInMpvCommand ??= new DelegateCommand<IPlayableContent>(WatchContentInMpvExecute, CanWatchContentInMpvExecute).ObservesProperty(() => MpvExeLocation);
    public ICommand WatchContentInMpcCommand => _watchContentInMpcCommand ??= new DelegateCommand<IPlayableContent>(WatchContentInMpcExecute, CanWatchContentInMpcExecute).ObservesProperty(() => MpcExeLocation);
    public ICommand CastContentCommand => _castContentCommand ??= new DelegateCommand<IPlayableContent>(CastContentExecute, CanCastContentExecute).ObservesProperty(() => SelectedReceiver);
    public ICommand CopyContentUrlCommand => _copyContentUrlCommand ??= new DelegateCommand<IPlayableContent>(CopyContentUrlExecute);
    public ICommand DownloadContentCommand => _downloadContentCommand ??= new DelegateCommand<IPlayableContent>(DownloadContentExecute);
    public ICommand SaveVideoDialogLayoutCommand => _saveVideoDialogLayoutCommand ??= new DelegateCommand(SaveVideoDialogLayoutExecute);
    public ICommand ImportVideoDialogLayoutCommand => _importVideoDialogLayoutCommand ??= new DelegateCommand(ImportVideoDialogLayoutExecute);
    public ICommand OpenVideoDialogLayoutCommand => _openVideoDialogLayoutCommand ??= new DelegateCommand<PlayerType?>(OpenVideoDialogLayoutExecute, CanOpenVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogLayout.Instances.Count).ObservesProperty(() => Channels.Count);
    public ICommand ScanReceiversCommand => _scanReceiversCommand ??= new DelegateCommand(ScanReceiversExecute);
    public ICommand ReceiverSelectionChangedCommand => _receiverSelectionChangedCommand ??= new DelegateCommand(ReceiverSelectionChangedExecute);
    public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand(AudioTrackSelectionChangedExecute);
    public ICommand LogOutCommand => _logOutCommand ??= new DelegateCommand(LogOutExecute);
    public ICommand RequestNavigateCommand => _requestNavigateCommand ??= new DelegateCommand<RequestNavigateEventArgs>(RequestNavigateExecute);

    public Action Close { get; set; }

    public ISettings Settings { get; }

    public IVideoDialogLayout VideoDialogLayout { get; }

    public ICollectionView EpisodesView { get; }

    public IDictionary<string, string> AudioLanguages { get; } = new Dictionary<string, string>
        {
            { LanguageCodes.English, "English" },
            { LanguageCodes.German, "German" },
            { LanguageCodes.French, "French" },
            { LanguageCodes.Spanish, "Spanish" },
            { LanguageCodes.Dutch, "Dutch" },
            { LanguageCodes.Portuguese, "Portuguese" }
        };

    public IDictionary<VideoQuality, string> VideoQualities { get; } = new Dictionary<VideoQuality, string>
        {
            { VideoQuality.High, "High" },
            { VideoQuality.Medium, "Medium" },
            { VideoQuality.Low, "Low" },
            { VideoQuality.Lowest, "Potato" }
        };

    public string EpisodeFilterText
    {
        get => _episodeFilterText;
        set
        {
            if (SetProperty(ref _episodeFilterText, value))
            {
                EpisodesView.Refresh();
            }
        }
    }

    public string VlcExeLocation
    {
        get => _vlcExeLocation;
        set => SetProperty(ref _vlcExeLocation, value);
    }

    public string MpvExeLocation
    {
        get => _mpvExeLocation;
        set => SetProperty(ref _mpvExeLocation, value);
    }

    public string MpcExeLocation
    {
        get => _mpcExeLocation;
        set => SetProperty(ref _mpcExeLocation, value);
    }

    public ObservableCollection<Season> Seasons => _seasons ??= new ObservableCollection<Season>();

    public ObservableCollection<Series> Series => _series ??= new ObservableCollection<Series>();

    public ObservableCollection<Event> Events => _events ??= new ObservableCollection<Event>();

    public ObservableCollection<Session> Sessions => _sessions ??= new ObservableCollection<Session>();

    public ObservableCollection<Session> LiveSessions => _liveSessions ??= new ObservableCollection<Session>();

    public ObservableCollection<string> VodGenres => _vodGenres ??= new ObservableCollection<string>();

    public ObservableCollection<IPlayableContent> Channels => _channels ??= new ObservableCollection<IPlayableContent>();

    public ObservableCollection<IPlayableContent> Episodes => _episodes ??= new ObservableCollection<IPlayableContent>();

    public ObservableCollection<NetworkInterface> NetworkInterfaces => _networkInterfaces ??= new ObservableCollection<NetworkInterface>(NetworkInterface.GetAllNetworkInterfaces());

    public ObservableCollection<IReceiver> Receivers => _receivers ??= new ObservableCollection<IReceiver>();

    public ObservableCollection<Track> AudioTracks => _audioTracks ??= new ObservableCollection<Track>();

    public Season SelectedSeason
    {
        get => _selectedSeason;
        set => SetProperty(ref _selectedSeason, value);
    }

    public Event SelectedEvent
    {
        get => _selectedEvent;
        set => SetProperty(ref _selectedEvent, value);
    }

    public Session SelectedLiveSession
    {
        get => _selectedLiveSession;
        set => SetProperty(ref _selectedLiveSession, value);
    }

    public Session SelectedSession
    {
        get => _selectedSession;
        set => SetProperty(ref _selectedSession, value);
    }

    public string SelectedVodGenre
    {
        get => _selectedVodGenre;
        set => SetProperty(ref _selectedVodGenre, value);
    }

    public NetworkInterface SelectedNetworkInterface
    {
        get => _selectedNetworkInterface;
        set => SetProperty(ref _selectedNetworkInterface, value);
    }

    public IReceiver SelectedReceiver
    {
        get => _selectedReceiver;
        set => SetProperty(ref _selectedReceiver, value);
    }

    public Track SelectedAudioTrack
    {
        get => _selectedAudioTrack;
        set => SetProperty(ref _selectedAudioTrack, value);
    }

    private void LoadedExecute(RoutedEventArgs args)
    {
        var helper = new WindowInteropHelper((Window)args.Source);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);

        IsBusy = true;
        Settings.Load();
        VideoDialogLayout.Load();
        SetVlcExeLocation();
        SetMpvExeLocation();
        SetMpcExeLocation();
        SetNetworkInterface();
        SetHotKeys();

        if (Settings.HasValidSubscriptionToken() || Login())
        {
            InitializeAsync().Await(() =>
            {
                SetNotBusy();
                SelectedSeason = Seasons.FirstOrDefault();
            },
            HandleCriticalError);
            RefreshLiveSessionsAsync(true).Await(CreateRefreshTimer, HandleNonCriticalError);
            CheckForUpdatesAsync().Await(HandleNonCriticalError);
        }
    }

    private void ClosingExecute()
    {
        RemoveRefreshTimer();
        RemoveHotKeys();
        Settings.SelectedNetworkInterface = SelectedNetworkInterface?.Id;
        Settings.Save();
    }

    private void CloseExecute()
    {
        Close?.Invoke();
    }

    private static void OpenLogFileExecute()
    {
        using var process = ProcessUtils.CreateProcess(FolderUtils.GetApplicationLogFilePath(), string.Empty, true);
        process.Start();
    }

    private static void MouseMoveExecute()
    {
        if (Mouse.OverrideCursor != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
        }
    }

    private void PreviewKeyDownExecute(KeyEventArgs args)
    {
        if (IsBusy)
        {
            args.Handled = true;
        }
    }

    private void KeyDownExecute(KeyEventArgs args)
    {
        if (IsBusy)
        {
            args.Handled = true;
        }
    }

    private void SeasonSelectionChangedExecute()
    {
        if (SelectedSeason != null)
        {
            ClearEvents();
            SelectSeasonAsync(SelectedSeason).Await(() =>
            {
                SetNotBusy();

                if (SelectedSeason.Year == DateTime.UtcNow.Year)
                {
                    // Select the most recent event
                    SelectedEvent = Events.OrderByDescending(e => e.StartDate).FirstOrDefault(e => DateTime.UtcNow.Date >= e.StartDate.GetValueOrDefault().Date);
                }
            },
            HandleCriticalError);
        }
    }

    private void EventSelectionChangedExecute()
    {
        if (SelectedEvent != null)
        {
            ClearSessions();
            SelectEventAsync(SelectedEvent).Await(SetNotBusy, HandleCriticalError);
        }
    }

    private void LiveSessionSelectionChangedExecute()
    {
        if (SelectedLiveSession != null)
        {
            SelectedSession = null;
            SelectSessionAsync(SelectedLiveSession).Await(SetNotBusy, HandleCriticalError);
        }
    }

    private void SessionSelectionChangedExecute()
    {
        if (SelectedSession != null)
        {
            SelectedLiveSession = null;
            SelectSessionAsync(SelectedSession).Await(SetNotBusy, HandleCriticalError);
        }
    }

    private void VodGenreSelectionChangedExecute()
    {
        if (!string.IsNullOrWhiteSpace(SelectedVodGenre))
        {
            SelectGenreAsync(SelectedVodGenre).Await(SetNotBusy, HandleCriticalError);
        }
    }

    private void WatchContentExecute(IPlayableContent playableContent)
    {
        WatchContent(playableContent);
    }

    private bool CanWatchContentInVlcExecute(IPlayableContent playableContent)
    {
        return !string.IsNullOrWhiteSpace(VlcExeLocation);
    }

    private void WatchContentInVlcExecute(IPlayableContent playableContent)
    {
        IsBusy = true;
        WatchInVlcAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
    }

    private bool CanWatchContentInMpvExecute(IPlayableContent playableContent)
    {
        return !string.IsNullOrWhiteSpace(MpvExeLocation);
    }

    private void WatchContentInMpvExecute(IPlayableContent playableContent)
    {
        IsBusy = true;
        WatchInMpvAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
    }

    private bool CanWatchContentInMpcExecute(IPlayableContent playableContent)
    {
        return !string.IsNullOrWhiteSpace(MpcExeLocation);
    }

    private void WatchContentInMpcExecute(IPlayableContent playableContent)
    {
        IsBusy = true;
        WatchInMpcAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
    }

    private bool CanCastContentExecute(IPlayableContent playableContent)
    {
        return SelectedReceiver != null;
    }

    private void CastContentExecute(IPlayableContent playableContent)
    {
        IsBusy = true;
        CastContentAsync(SelectedReceiver, playableContent).Await(() =>
        {
            SetNotBusy();

            var languageCodes = playableContent.GetAudioLanguages(Settings.DefaultAudioLanguage);

            foreach (var languageCode in languageCodes)
            {
                var audioTrack = AudioTracks.FirstOrDefault(track => track.Language == languageCode);

                if (audioTrack != null)
                {
                    SelectedAudioTrack = audioTrack;
                    break;
                }
            }
        }, HandleCriticalError);
    }

    private void CopyContentUrlExecute(IPlayableContent playableContent)
    {
        IsBusy = true;
        CopyUrlAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
    }

    private void DownloadContentExecute(IPlayableContent playableContent)
    {
        StartDownload(playableContent);
    }

    private void SaveVideoDialogLayoutExecute()
    {
        const string caption = "Save layout";

        VideoDialogLayout.Instances.Clear();
        _eventAggregator.GetEvent<SaveLayoutEvent>().Publish(ContentType.Channel);

        if (VideoDialogLayout.Instances.Any())
        {
            if (VideoDialogLayout.Save())
            {
                MessageBoxHelper.ShowInfo("The current window layout has been successfully saved.", caption);
            }
        }
        else
        {
            VideoDialogLayout.Load();
            MessageBoxHelper.ShowError("Could not find any internal player windows to save.", caption);
        }
    }

    private void ImportVideoDialogLayoutExecute()
    {
        const string caption = "Import layout";

        var initialDirectory = FolderUtils.GetLocalApplicationDataFolder();

        if (_dialogService.OpenFile("Select layout file", initialDirectory, ".json", out var filename))
        {
            if (VideoDialogLayout.Import(filename))
            {
                MessageBoxHelper.ShowInfo("Layout file has been successfully imported.", caption);
            }
            else
            {
                MessageBoxHelper.ShowError("Layout file could not be imported.", caption);
            }
        }
    }

    private bool CanOpenVideoDialogLayoutExecute(PlayerType? playerType)
    {
        return VideoDialogLayout.Instances.Any() && Channels.Count > 1 && (playerType == PlayerType.Internal || playerType == PlayerType.Mpv);
    }

    private void OpenVideoDialogLayoutExecute(PlayerType? playerType)
    {
        _eventAggregator.GetEvent<CloseAllEvent>().Publish(ContentType.Channel);
        OpenVideoDialogLayoutAsync(playerType).Await(HandleCriticalError);
    }

    private void ScanReceiversExecute()
    {
        IsBusy = true;
        FindReceiversAsync().Await(() =>
        {
            SetNotBusy();

            if (Receivers.Any())
            {
                SelectedReceiver = Receivers.First();
            }
        }, HandleCriticalError);
    }

    private void ReceiverSelectionChangedExecute()
    {
        if (SelectedReceiver == null)
        {
            return;
        }

        IsBusy = true;
        FindAudioTracksAsync(SelectedReceiver).Await(SetNotBusy, HandleCriticalError);
    }

    private void AudioTrackSelectionChangedExecute()
    {
        if (SelectedAudioTrack == null)
        {
            return;
        }

        IsBusy = true;
        ChangeAudioTrackAsync(SelectedReceiver, SelectedAudioTrack).Await(SetNotBusy, HandleCriticalError);
    }

    private void LogOutExecute()
    {
        IsBusy = true;

        if (MessageBoxHelper.AskQuestion("Are you sure you want to log out?", "Account"))
        {
            Login();
        }

        IsBusy = false;
    }

    private static void RequestNavigateExecute(RequestNavigateEventArgs e)
    {
        ProcessUtils.BrowseToUrl(e.Uri.AbsoluteUri);
    }

    private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        lock (_refreshTimerLock)
        {
            _refreshTimer?.Stop();
        }

        RefreshLiveSessionsAsync(false).Await(() =>
        {
            lock (_refreshTimerLock)
            {
                _refreshTimer?.Start();
            }
        }, HandleNonCriticalError);
    }

    private void SetVlcExeLocation()
    {
        var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC") ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\VideoLAN\VLC");

        if (registryKey?.GetValue("InstallDir") is string vlcInstallDir && !string.IsNullOrWhiteSpace(vlcInstallDir))
        {
            var vlcExeLocation = Path.Combine(vlcInstallDir, "vlc.exe");

            if (File.Exists(vlcExeLocation))
            {
                VlcExeLocation = vlcExeLocation;
                Logger.Info($"Found VLC installation at '{vlcExeLocation}'.");
            }
            else
            {
                Logger.Warn("Could not find VLC installation.");
            }
        }
        else
        {
            Logger.Warn("Could not find VLC installation.");
        }
    }

    private void SetMpvExeLocation()
    {
        var mpvExeLocation = Path.Combine(Environment.CurrentDirectory, @"mpv\mpv.exe");

        if (File.Exists(mpvExeLocation))
        {
            MpvExeLocation = mpvExeLocation;
            Logger.Info($"Found MPV installation at '{mpvExeLocation}'.");
        }
        else
        {
            Logger.Warn("Could not find MPV installation.");
        }
    }

    private void SetMpcExeLocation()
    {
        var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MPC-HC\MPC-HC") ?? Registry.CurrentUser.OpenSubKey(@"SOFTWARE\WOW6432Node\MPC-HC\MPC-HC");

        if (registryKey?.GetValue("ExePath") is string mpcExeLocation && File.Exists(mpcExeLocation))
        {
            MpcExeLocation = mpcExeLocation;
            Logger.Info($"Found MPC-HC installation at '{mpcExeLocation}'.");
        }
        else
        {
            Logger.Warn("Could not find MPC-HC installation.");
        }
    }

    private void SetNetworkInterface()
    {
        SelectedNetworkInterface = !string.IsNullOrWhiteSpace(Settings.SelectedNetworkInterface) ?
            NetworkInterfaces.FirstOrDefault(n => n.Id == Settings.SelectedNetworkInterface) :
            NetworkInterfaces.FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);
    }

    private async Task CheckForUpdatesAsync()
    {
        Logger.Info("Checking for updates...");

        var release = await _githubService.GetLatestRelease();

        if (release == null || release.PreRelease || release.Draft || release.TagName == Settings.LatestRelease)
        {
            Logger.Info("No new release found.");
        }
        else if (Version.TryParse(release.TagName, out var version) && version > AssemblyUtils.GetApplicationVersion())
        {
            Logger.Info($"Found new release '{release.Name}'.");

            var parameters = new DialogParameters
                {
                    { ParameterNames.Release, release }
                };

            _dialogService.ShowDialog(nameof(UpgradeDialog), parameters, dialogResult =>
            {
                if (dialogResult.Result == ButtonResult.OK)
                {
                    ProcessUtils.BrowseToUrl(release.HtmlUrl);
                }
            });
        }

        if (release != null)
        {
            Settings.LatestRelease = release.TagName;
        }
    }

    private bool Login()
    {
        var success = false;
        Settings.ClearSubscriptionToken();

        _dialogService.ShowDialog(nameof(LoginDialog), null, dialogResult =>
        {
            success = dialogResult.Result == ButtonResult.OK;

            if (success)
            {
                var subscriptionToken = dialogResult.Parameters.GetValue<string>(ParameterNames.SubscriptionToken);
                var subscriptionStatus = dialogResult.Parameters.GetValue<string>(ParameterNames.SubscriptionStatus);
                Settings.UpdateSubscriptionToken(subscriptionToken, subscriptionStatus);
            }
            else
            {
                Logger.Info("Login cancelled by user, shutting down...");
                Application.Current.Shutdown();
            }
        });

        return success;
    }

    private async Task InitializeAsync()
    {
        LoadSeasons();
        LoadSeries();
        await LoadVodGenresAsync();
    }

    private void LoadSeasons()
    {
        var seasons = _apiService.GetSeasons();
        Seasons.AddRange(seasons);
    }

    private void LoadSeries()
    {
        var series = _apiService.GetSeries();
        Series.AddRange(series);

        if (!Settings.SelectedSeries.Any())
        {
            Settings.SelectedSeries.Add(SeriesIds.Formula1);
        }
    }

    private async Task LoadVodGenresAsync()
    {
        VodGenres.AddRange(await _apiService.GetVodGenresAsync());
    }

    private void CreateRefreshTimer()
    {
        RemoveRefreshTimer();

        lock (_refreshTimerLock)
        {
            _refreshTimer = new Timer(60000) { AutoReset = false };
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.Start();
        }
    }

    private void RemoveRefreshTimer()
    {
        lock (_refreshTimerLock)
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
        }
    }

    private async Task RefreshLiveSessionsAsync(bool isFirstRefresh)
    {
        Logger.Info("Refreshing live sessions...");

        var liveSessions = (await _apiService.GetLiveSessionsAsync()).ToList();
        var sessionsToRemove = LiveSessions.Where(existingLiveSession => liveSessions.All(liveSession => liveSession.UID != existingLiveSession.UID)).ToList();
        var sessionsToAdd = liveSessions.Where(newLiveSession => LiveSessions.All(liveSession => liveSession.UID != newLiveSession.UID)).ToList();

        if (sessionsToRemove.Any() || sessionsToAdd.Any())
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var sessionToRemove in sessionsToRemove)
                {
                    if (SelectedLiveSession?.UID == sessionToRemove.UID)
                    {
                        Episodes.Clear();
                        Channels.Clear();
                    }

                    LiveSessions.Remove(sessionToRemove);
                }

                if (sessionsToAdd.Any())
                {
                    LiveSessions.AddRange(sessionsToAdd);

                    if (!isFirstRefresh && LiveSessions.Count == 1 && !Settings.DisableLiveSessionNotification)
                    {
                        _liveSessionPlayer.Play();
                    }
                }
            });
        }
    }

    private async Task SelectSeasonAsync(Season season)
    {
        IsBusy = true;

        if (season.Year >= 2018)
        {
            await LoadEventsForSeasonAsync(season);
        }
        else
        {
            await LoadEpisodesForSeasonAsync(season);
        }
    }

    private async Task SelectEventAsync(Event evt)
    {
        IsBusy = true;

        await Task.WhenAll(LoadSessionsForEventAsync(evt), LoadEpisodesForEventAsync(evt));
    }

    private async Task SelectSessionAsync(Session session)
    {
        IsBusy = true;
        Channels.Clear();
        SelectedVodGenre = null;

        await LoadChannelsForSessionAsync(session);
    }

    private async Task SelectGenreAsync(string genre)
    {
        IsBusy = true;
        Episodes.Clear();
        Channels.Clear();
        Sessions.Clear();
        SelectedLiveSession = null;
        SelectedSession = null;
        SelectedEvent = null;

        await LoadEpisodesForGenreAsync(genre);
    }

    private async Task LoadEventsForSeasonAsync(Season season)
    {
        var events = await _apiService.GetEventsForSeasonAsync(season);
        Events.AddRange(events);
    }

    private async Task LoadEpisodesForSeasonAsync(Season season)
    {
        var episodes = await _apiService.GetEpisodesForSeasonAsync(season);
        Episodes.AddRange(episodes.Select(e => new PlayableEpisode(e)));
    }

    private async Task LoadSessionsForEventAsync(Event evt)
    {
        var sessions = await _apiService.GetSessionsForEventAsync(evt);
        Sessions.AddRange(sessions);
    }

    private async Task LoadEpisodesForEventAsync(Event evt)
    {
        var episodes = await _apiService.GetEpisodesForEventAsync(evt);
        Episodes.AddRange(episodes.Select(e => new PlayableEpisode(e)));
    }

    private async Task LoadChannelsForSessionAsync(Session session)
    {
        var channels = await _apiService.GetChannelsForSessionAsync(session);

        Channels.AddRange(channels.Select(c => new PlayableChannel(session, c)).OrderBy(c => c.Name, new ChannelNameComparer()));
    }

    private async Task LoadEpisodesForGenreAsync(string genre)
    {
        var episodes = await _apiService.GetEpisodesForGenreAsync(genre);
        Episodes.AddRange(episodes.Select(e => new PlayableEpisode(e)));
    }

    private void WatchContent(IPlayableContent playableContent, VideoDialogSettings settings = null)
    {
        var identifier = _numberGenerator.GetNextNumber();
        var parameters = new DialogParameters
            {
                { ParameterNames.Identifier, identifier },
                { ParameterNames.Content, playableContent },
                { ParameterNames.Settings, settings }
            };

        _dialogService.Show(nameof(VideoDialog), parameters, _ => _numberGenerator.RemoveNumber(identifier), nameof(VideoDialogWindow));
    }

    private async Task WatchInVlcAsync(IPlayableContent playableContent)
    {
        var streamUrl = await _apiService.GetTokenisedUrlAsync(Settings.SubscriptionToken, playableContent);
        ValidateStreamUrl(streamUrl);
        using var process = ProcessUtils.CreateProcess(VlcExeLocation, $"\"{streamUrl}\" --meta-title=\"{playableContent.Title}\"");
        process.Start();
    }

    private async Task WatchInMpvAsync(IPlayableContent playableContent, VideoDialogSettings settings = null)
    {
        // Use different stream type because this one doesn't require a playToken cookie (not supported by MPV)
        var streamUrl = await _apiService.GetTokenisedUrlAsync(Settings.SubscriptionToken, playableContent, StreamTypeKeys.BigScreenHls);
        ValidateStreamUrl(streamUrl);

        var arguments = new List<string>
            {
                $"\"{streamUrl}\"",
                $"--title=\"{playableContent.Title}\""
            };

        if (!Settings.DisableMpvNoBorder)
        {
            arguments.Add("--no-border");
        }

        if (Settings.EnableMpvAutoSync && playableContent.ContentType == ContentType.Channel && playableContent.IsLive)
        {
            arguments.Add("--script=autosync.lua --script-opts=autosync-lag=20.0");
        }

        if (!string.IsNullOrWhiteSpace(Settings.AdditionalMpvParameters))
        {
            arguments.Add(Settings.AdditionalMpvParameters.Trim());
        }

        if (settings == null)
        {
            AddVideoQualityArgument(Settings.DefaultVideoQuality);

            if (!arguments.Any(argument => argument.Contains("--alang", StringComparison.OrdinalIgnoreCase)))
            {
                var languageCodes = playableContent.GetAudioLanguages(Settings.DefaultAudioLanguage);
                arguments.Add($"--alang={string.Join(',', languageCodes)}");
            }
        }
        else
        {
            var screenIndex = ScreenHelper.GetScreenIndex(settings);

            if (settings.FullScreen)
            {
                arguments.Add("--fs");
                arguments.Add($"--screen={screenIndex}");
                arguments.Add($"--fs-screen={screenIndex}");
            }
            else
            {
                var screenScale = ScreenHelper.GetScreenScale();
                var scaledWidth = settings.Width * screenScale;
                var scaledHeight = settings.Height * screenScale;
                ScreenHelper.GetRelativeCoordinates(screenIndex, settings.Left, settings.Top, out var relativeLeft, out var relativeTop);
                arguments.Add($"--geometry={scaledWidth:0}x{scaledHeight:0}+{relativeLeft:+0;-#}+{relativeTop:+0;-#}");
                arguments.Add($"--screen={screenIndex}");
            }

            if (settings.Topmost)
            {
                arguments.Add("--ontop");
            }

            AddVideoQualityArgument(settings.VideoQuality);
            arguments.Add($"--alang={settings.AudioTrack}");
            arguments.Add($"--volume={settings.Volume}");
            arguments.Add($"--mute={settings.IsMuted.GetYesNoString()}");
        }

        var mpvExeLocation = MpvExeLocation;
        var customMpvPath = Settings.CustomMpvPath?.Trim();

        if (!string.IsNullOrWhiteSpace(customMpvPath))
        {
            if (File.Exists(customMpvPath))
            {
                mpvExeLocation = customMpvPath;
            }
            else
            {
                Logger.Warn($"Could not find MPV executable at '{customMpvPath}', falling back to included MPV executable.");
            }
        }

        void AddVideoQualityArgument(VideoQuality videoQuality)
        {
            switch (videoQuality)
            {
                case VideoQuality.Lowest:
                    arguments.Add("--hls-bitrate=2500000");
                    break;

                case VideoQuality.Low:
                    arguments.Add("--hls-bitrate=4000000");
                    break;

                case VideoQuality.Medium:
                    arguments.Add("--hls-bitrate=7000000");
                    break;
            }
        }

        using var process = ProcessUtils.CreateProcess(mpvExeLocation, string.Join(" ", arguments));
        process.Start();
    }

    private async Task WatchInMpcAsync(IPlayableContent playableContent)
    {
        var streamUrl = await _apiService.GetTokenisedUrlAsync(Settings.SubscriptionToken, playableContent);
        ValidateStreamUrl(streamUrl);
        using var process = ProcessUtils.CreateProcess(MpcExeLocation, $"\"{streamUrl}\"");
        process.Start();
    }

    private async Task CastContentAsync(IReceiver receiver, IPlayableContent playableContent)
    {
        var streamUrl = await _apiService.GetTokenisedUrlAsync(Settings.SubscriptionToken, playableContent);
        ValidateStreamUrl(streamUrl);
        AudioTracks.Clear();

        try
        {
            await _sender.ConnectAsync(receiver);
            var mediaChannel = _sender.GetChannel<IMediaChannel>();

            if (mediaChannel != null)
            {
                // hacking mediachannel to update app id
                mediaChannel.SetApplicationId("B3E81094");
                await _sender.LaunchAsync(mediaChannel);
                var status = await mediaChannel.LoadAsync(new MediaInformation
                {
                    ContentId = streamUrl,
                    ContentType = "application/x-mpegURL",
                    StreamType = playableContent.IsLive ? StreamType.Live : StreamType.Buffered,
                    CustomData = GetCustomData()
                });

                if (status?.Media?.Tracks != null)
                {
                    var audioTracks = status.Media.Tracks.Where(t => t.Type == TrackType.Audio);
                    AudioTracks.AddRange(audioTracks);
                }
            }
        }
        finally
        {
            _sender.Disconnect();
        }

        IDictionary<string, string> GetCustomData()
        {
            var metadata = new
            {
                ascendontoken = Settings.SubscriptionToken,
                requestChannel = StreamTypeKeys.WebHls
            };

            var options = new
            {
                manifestWithCredentials = true,
                withCredentials = true
            };

            return new Dictionary<string, string>
                {
                    { nameof(metadata), JsonConvert.SerializeObject(metadata) },
                    { nameof(options), JsonConvert.SerializeObject(options) }
                };
        }
    }

    private async Task CopyUrlAsync(IPlayableContent playableContent)
    {
        var streamUrl = await _apiService.GetTokenisedUrlAsync(Settings.SubscriptionToken, playableContent);
        ValidateStreamUrl(streamUrl);
        Clipboard.SetText(streamUrl);
    }

    private void StartDownload(IPlayableContent playableContent)
    {
        var defaultFilename = $"{playableContent.Title}.mp4".RemoveInvalidFileNameChars();
        var initialDirectory = !string.IsNullOrWhiteSpace(Settings.RecordingLocation) ? Settings.RecordingLocation : FolderUtils.GetSpecialFolderPath(Environment.SpecialFolder.Desktop);
        var filename = Path.Join(initialDirectory, defaultFilename);

        if (Settings.SkipSaveDialog || _dialogService.SaveFile("Select a filename", initialDirectory, defaultFilename, ".mp4", out filename))
        {
            var parameters = new DialogParameters
                {
                    { ParameterNames.Content, playableContent},
                    { ParameterNames.Filename, filename }
                };

            _dialogService.Show(nameof(DownloadDialog), parameters, null);
        }
    }

    private async Task OpenVideoDialogLayoutAsync(PlayerType? playerType)
    {
        const int delaySeconds = 2;

        var delayTimeSpan = TimeSpan.FromSeconds(delaySeconds);

        foreach (var settings in VideoDialogLayout.Instances)
        {
            var playableContent = Channels.FirstOrDefault(c => c.ContentType == ContentType.Channel && c.Name == settings.ChannelName);

            if (playableContent != null)
            {
                switch (playerType)
                {
                    case PlayerType.Internal:
                        WatchContent(playableContent, settings);
                        break;

                    case PlayerType.Mpv:
                        await WatchInMpvAsync(playableContent, settings);
                        break;
                }
            }

            await Task.Delay(delayTimeSpan);
        }
    }

    private async Task FindReceiversAsync()
    {
        Receivers.Clear();
        AudioTracks.Clear();
        var receivers = await _deviceLocator.FindReceiversAsync(SelectedNetworkInterface);
        Receivers.AddRange(receivers);
    }

    private async Task FindAudioTracksAsync(IReceiver receiver)
    {
        AudioTracks.Clear();

        try
        {
            await _sender.ConnectAsync(receiver);
            var mediaChannel = _sender.GetChannel<IMediaChannel>();

            if (mediaChannel != null)
            {
                var status = await mediaChannel.GetStatusAsync();

                if (status?.Media?.Tracks != null)
                {
                    var audioTracks = status.Media.Tracks.Where(t => t.Type == TrackType.Audio);
                    AudioTracks.AddRange(audioTracks);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            HandleNonCriticalError(ex);
        }
        catch (ArgumentNullException ex)
        {
            HandleNonCriticalError(ex);
        }
        finally
        {
            _sender.Disconnect();
        }
    }

    private async Task ChangeAudioTrackAsync(IReceiver receiver, Track audioTrack)
    {
        try
        {
            await _sender.ConnectAsync(receiver);
            var mediaChannel = _sender.GetChannel<IMediaChannel>();
            await mediaChannel.GetStatusAsync();
            await mediaChannel.EditTracksInfoAsync(activeTrackIds: new[] { audioTrack.TrackId });
        }
        finally
        {
            _sender.Disconnect();
        }
    }

    private bool EpisodesViewFilter(object episode)
    {
        if (!string.IsNullOrEmpty(EpisodeFilterText) && episode is IPlayableContent playableContent)
        {
            return (playableContent.ToString() ?? string.Empty).Contains(EpisodeFilterText, StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private void ClearEvents()
    {
        ClearSessions();
        Events.Clear();
    }

    private void ClearSessions()
    {
        Episodes.Clear();
        Channels.Clear();
        Sessions.Clear();
        SelectedLiveSession = null;
        SelectedVodGenre = null;
    }

    private static void ValidateStreamUrl(string streamUrl)
    {
        if (string.IsNullOrWhiteSpace(streamUrl))
        {
            throw new Exception("An error occurred while retrieving the stream URL.");
        }
    }

    private void SetHotKeys()
    {
        if (_hwndSource != null)
        {
            _hwndSource.AddHook(HwndHook);
            RegisterHotKey(_hwndSource.Handle, PlayPauseHotKeyID, 0, (uint)KeyInterop.VirtualKeyFromKey(Key.MediaPlayPause));
        }
    }

    private void RemoveHotKeys()
    {
        if (_hwndSource != null)
        {
            UnregisterHotKey(_hwndSource.Handle, PlayPauseHotKeyID);
            _hwndSource.RemoveHook(HwndHook);
        }
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmHotkey = 0x0312;

        if (msg == wmHotkey && wParam.ToInt32() == PlayPauseHotKeyID)
        {
            OnPlayPausePressed();
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void OnPlayPausePressed()
    {
        _eventAggregator.GetEvent<PauseAllEvent>().Publish();
    }

    [DllImport("User32.dll")]
    private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

    [DllImport("User32.dll")]
    private static extern bool UnregisterHotKey([In] IntPtr hWnd, [In] int id);
}