using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Common.Generators;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Utils;
using RaceControl.Comparers;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using RaceControl.Events;
using RaceControl.Extensions;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Entities;
using RaceControl.Services.Interfaces.Github;
using RaceControl.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable once UnusedType.Global
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IExtendedDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IApiService _apiService;
        private readonly IGithubService _githubService;
        private readonly ICredentialService _credentialService;
        private readonly INumberGenerator _numberGenerator;
        private readonly object _refreshTimerLock = new();

        private ICommand _loadedCommand;
        private ICommand _closingCommand;
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
        private ICommand _copyContentUrlCommand;
        private ICommand _downloadContentCommand;
        private ICommand _setDownloadLocationCommand;
        private ICommand _saveVideoDialogLayoutCommand;
        private ICommand _openVideoDialogLayoutCommand;
        private ICommand _deleteCredentialCommand;

        private string _token;
        private string _episodeFilterText;
        private string _vlcExeLocation;
        private string _mpvExeLocation;
        private ObservableCollection<Season> _seasons;
        private ObservableCollection<Series> _series;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Session> _sessions;
        private ObservableCollection<Session> _liveSessions;
        private ObservableCollection<string> _vodGenres;
        private ObservableCollection<IPlayableContent> _channels;
        private ObservableCollection<IPlayableContent> _episodes;
        private Season _selectedSeason;
        private Event _selectedEvent;
        private Session _selectedLiveSession;
        private Session _selectedSession;
        private string _selectedVodGenre;
        private Timer _refreshTimer;

        public MainWindowViewModel(
            ILogger logger,
            IExtendedDialogService dialogService,
            IEventAggregator eventAggregator,
            IApiService apiService,
            IGithubService githubService,
            ICredentialService credentialService,
            INumberGenerator numberGenerator,
            ISettings settings,
            IVideoDialogLayout videoDialogLayout)
            : base(logger)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            _apiService = apiService;
            _githubService = githubService;
            _credentialService = credentialService;
            _numberGenerator = numberGenerator;
            Settings = settings;
            VideoDialogLayout = videoDialogLayout;
            EpisodesView = CollectionViewSource.GetDefaultView(Episodes);
            EpisodesView.Filter = EpisodesViewFilter;
        }

        public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
        public ICommand ClosingCommand => _closingCommand ??= new DelegateCommand(ClosingExecute);
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
        public ICommand CopyContentUrlCommand => _copyContentUrlCommand ??= new DelegateCommand<IPlayableContent>(CopyContentUrlExecute);
        public ICommand DownloadContentCommand => _downloadContentCommand ??= new DelegateCommand<IPlayableContent>(DownloadContentExecute, CanDownloadContentExecute);
        public ICommand SetDownloadLocationCommand => _setDownloadLocationCommand ??= new DelegateCommand(SetDownloadLocationExecute);
        public ICommand SaveVideoDialogLayoutCommand => _saveVideoDialogLayoutCommand ??= new DelegateCommand(SaveVideoDialogLayoutExecute);
        public ICommand OpenVideoDialogLayoutCommand => _openVideoDialogLayoutCommand ??= new DelegateCommand<PlayerType?>(OpenVideoDialogLayoutExecute, CanOpenVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogLayout.Instances.Count).ObservesProperty(() => Channels.Count);
        public ICommand DeleteCredentialCommand => _deleteCredentialCommand ??= new DelegateCommand(DeleteCredentialExecute);

        public ISettings Settings { get; }

        public IVideoDialogLayout VideoDialogLayout { get; }

        public ICollectionView EpisodesView { get; }

        public IDictionary<string, string> StreamTypes { get; } = new Dictionary<string, string>
        {
            { StreamTypeKeys.BigScreenHls, "HLS (non-adaptive)" },
            { StreamTypeKeys.BigScreenDash, "DASH (adaptive)" }
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

        public ObservableCollection<Season> Seasons => _seasons ??= new ObservableCollection<Season>();

        public ObservableCollection<Series> Series => _series ??= new ObservableCollection<Series>();

        public ObservableCollection<Event> Events => _events ??= new ObservableCollection<Event>();

        public ObservableCollection<Session> Sessions => _sessions ??= new ObservableCollection<Session>();

        public ObservableCollection<Session> LiveSessions => _liveSessions ??= new ObservableCollection<Session>();

        public ObservableCollection<string> VodGenres => _vodGenres ??= new ObservableCollection<string>();

        public ObservableCollection<IPlayableContent> Channels => _channels ??= new ObservableCollection<IPlayableContent>();

        public ObservableCollection<IPlayableContent> Episodes => _episodes ??= new ObservableCollection<IPlayableContent>();

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

        private void LoadedExecute(RoutedEventArgs args)
        {
            IsBusy = true;
            Settings.Load();
            VideoDialogLayout.Load();
            SetVlcExeLocation();
            SetMpvExeLocation();

            if (Login())
            {
                InitializeAsync().Await(() =>
                {
                    SetNotBusy();
                    SelectedSeason = Seasons.FirstOrDefault();
                },
                HandleCriticalError);
                RefreshLiveSessionsAsync().Await(CreateRefreshTimer, HandleNonCriticalError);
                CheckForUpdatesAsync().Await(HandleNonCriticalError);
            }
        }

        private void ClosingExecute()
        {
            RemoveRefreshTimer();
            Settings.Save();
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
            ClearEvents();

            if (SelectedSeason != null)
            {
                IsBusy = true;
                SelectSeasonAsync(SelectedSeason).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void EventSelectionChangedExecute()
        {
            ClearSessions();

            if (SelectedEvent != null)
            {
                IsBusy = true;
                SelectEventAsync(SelectedEvent).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void LiveSessionSelectionChangedExecute()
        {
            if (SelectedLiveSession != null)
            {
                IsBusy = true;
                SelectedSession = null;
                SelectSessionAsync(SelectedLiveSession, true).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void SessionSelectionChangedExecute()
        {
            if (SelectedSession != null)
            {
                IsBusy = true;
                SelectedLiveSession = null;
                SelectSessionAsync(SelectedSession, false).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void VodGenreSelectionChangedExecute()
        {
            if (!string.IsNullOrWhiteSpace(SelectedVodGenre))
            {
                IsBusy = true;
                SelectGenreAsync(SelectedVodGenre).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void WatchContentExecute(IPlayableContent playableContent)
        {
            WatchContent(playableContent);
        }

        private bool CanWatchContentInVlcExecute(IPlayableContent playableContent)
        {
            return !string.IsNullOrWhiteSpace(VlcExeLocation) && File.Exists(VlcExeLocation);
        }

        private void WatchContentInVlcExecute(IPlayableContent playableContent)
        {
            IsBusy = true;
            WatchInVlcAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
        }

        private bool CanWatchContentInMpvExecute(IPlayableContent playableContent)
        {
            return !string.IsNullOrWhiteSpace(MpvExeLocation) && File.Exists(MpvExeLocation);
        }

        private void WatchContentInMpvExecute(IPlayableContent playableContent)
        {
            IsBusy = true;
            WatchInMpvAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
        }

        private void CopyContentUrlExecute(IPlayableContent playableContent)
        {
            IsBusy = true;
            CopyUrlAsync(playableContent).Await(SetNotBusy, HandleCriticalError);
        }

        private static bool CanDownloadContentExecute(IPlayableContent playableContent)
        {
            return !playableContent.IsLive;
        }

        private void DownloadContentExecute(IPlayableContent playableContent)
        {
            StartDownload(playableContent);
        }

        private void SetDownloadLocationExecute()
        {
            if (_dialogService.SelectFolder("Select a download location", Settings.DownloadLocation, out var downloadLocation))
            {
                Settings.DownloadLocation = downloadLocation;
            }
        }

        private void SaveVideoDialogLayoutExecute()
        {
            VideoDialogLayout.Instances.Clear();
            _eventAggregator.GetEvent<SaveLayoutEvent>().Publish(ContentType.Channel);

            if (VideoDialogLayout.Instances.Any())
            {
                if (VideoDialogLayout.Save())
                {
                    MessageBoxHelper.ShowInfo("The current window layout has been successfully saved.", "Video player layout");
                }
            }
            else
            {
                VideoDialogLayout.Load();
                MessageBoxHelper.ShowError("Could not find any internal player windows to save.", "Video player layout");
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

        private void DeleteCredentialExecute()
        {
            IsBusy = true;

            if (MessageBoxHelper.AskQuestion("Are you sure you want to delete your credentials from this system?", "Account"))
            {
                _credentialService.DeleteCredential();
                Login();
            }

            IsBusy = false;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_refreshTimerLock)
            {
                _refreshTimer?.Stop();
            }

            RefreshLiveSessionsAsync().Await(() =>
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

            if (registryKey?.GetValue(null) is string vlcExeLocation && File.Exists(vlcExeLocation))
            {
                VlcExeLocation = vlcExeLocation;
                Logger.Info($"Found VLC installation at '{vlcExeLocation}'.");
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

            _dialogService.ShowDialog(nameof(LoginDialog), null, dialogResult =>
            {
                success = dialogResult.Result == ButtonResult.OK;

                if (success)
                {
                    _token = dialogResult.Parameters.GetValue<string>(ParameterNames.Token);
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
                Settings.SelectedSeries.Add("FORMULA 1");
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

        private async Task RefreshLiveSessionsAsync()
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
                    }
                });
            }
        }

        private async Task SelectSeasonAsync(Season season)
        {
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
            await Task.WhenAll(LoadSessionsForEventAsync(evt), LoadEpisodesForEventAsync(evt));
        }

        private async Task SelectSessionAsync(Session session, bool clearEpisodes)
        {
            if (clearEpisodes)
            {
                Episodes.Clear();
            }

            Channels.Clear();
            SelectedVodGenre = null;

            await LoadChannelsForSessionAsync(session);
        }

        private async Task SelectGenreAsync(string genre)
        {
            Episodes.Clear();
            Channels.Clear();
            Sessions.Clear();
            SelectedLiveSession = null;
            SelectedSession = null;

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

            if (session.IsLive && channels.Count > 1)
            {
                channels.Add(new Channel
                {
                    ChannelType = ChannelTypes.Backup,
                    Name = "Backup stream"
                });
            }

            Channels.AddRange(channels.OrderBy(c => c.ChannelType, new ChannelTypeComparer()).Select(c => new PlayableChannel(session, c)));
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
                { ParameterNames.Token, _token },
                { ParameterNames.Identifier, identifier },
                { ParameterNames.Content, playableContent },
                { ParameterNames.Settings, settings }
            };

            _dialogService.Show(nameof(VideoDialog), parameters, _ => _numberGenerator.RemoveNumber(identifier), nameof(VideoDialogWindow));
        }

        private async Task WatchInVlcAsync(IPlayableContent playableContent)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, Settings.StreamType, playableContent);

            if (!ValidateStreamUrl(streamUrl))
            {
                return;
            }

            using var process = ProcessUtils.CreateProcess(VlcExeLocation, $"\"{streamUrl}\" --meta-title=\"{playableContent.Title}\"");
            process.Start();
        }

        private async Task WatchInMpvAsync(IPlayableContent playableContent, VideoDialogSettings settings = null)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, Settings.StreamType, playableContent);

            if (!ValidateStreamUrl(streamUrl))
            {
                return;
            }

            var arguments = new List<string>
            {
                $"\"{streamUrl}\"",
                $"--title=\"{playableContent.Title}\""
            };

            if (!Settings.DisableMpvNoBorder)
            {
                arguments.Add("--no-border");
            }

            if (!string.IsNullOrWhiteSpace(Settings.AdditionalMpvParameters))
            {
                arguments.Add(Settings.AdditionalMpvParameters.Trim());
            }

            if (settings != null)
            {
                var screenIndex = ScreenHelper.GetScreenIndex(settings);

                if (settings.WindowState == WindowState.Maximized)
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

                arguments.Add($"--volume={settings.Volume}");
                arguments.Add($"--mute={settings.IsMuted.GetYesNoString()}");
            }

            using var process = ProcessUtils.CreateProcess(MpvExeLocation, string.Join(" ", arguments));
            process.Start();
        }

        private async Task CopyUrlAsync(IPlayableContent playableContent)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, Settings.StreamType, playableContent);

            if (!ValidateStreamUrl(streamUrl))
            {
                return;
            }

            Clipboard.SetText(streamUrl);
        }

        private void StartDownload(IPlayableContent playableContent)
        {
            var defaultFilename = $"{playableContent.Title}.ts".RemoveInvalidFileNameChars();

            if (_dialogService.SelectFile("Select a filename", Settings.DownloadLocation, defaultFilename, ".ts", out var filename))
            {
                var parameters = new DialogParameters
                {
                    { ParameterNames.Token, _token },
                    { ParameterNames.Content, playableContent},
                    { ParameterNames.Filename, filename }
                };

                _dialogService.Show(nameof(DownloadDialog), parameters, null);
            }
        }

        private async Task OpenVideoDialogLayoutAsync(PlayerType? playerType)
        {
            var delaySeconds = playerType == PlayerType.Mpv ? 2 : 1;
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

        private static bool ValidateStreamUrl(string streamUrl)
        {
            if (string.IsNullOrWhiteSpace(streamUrl))
            {
                MessageBoxHelper.ShowError("An error occurred while retrieving the stream URL.");

                return false;
            }

            return true;
        }
    }
}