using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Common.Enum;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Settings;
using RaceControl.Common.Utils;
using RaceControl.Comparers;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Github;
using RaceControl.Streamlink;
using RaceControl.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IExtendedDialogService _dialogService;
        private readonly IApiService _apiService;
        private readonly IGithubService _githubService;
        private readonly ICredentialService _credentialService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;

        private ICommand _loadedCommand;
        private ICommand _closingCommand;
        private ICommand _mouseMoveCommand;
        private ICommand _previewKeyDownCommand;
        private ICommand _keyDownCommand;
        private ICommand _seasonSelectionChangedCommand;
        private ICommand _eventSelectionChangedCommand;
        private ICommand _liveSessionSelectionChangedCommand;
        private ICommand _sessionSelectionChangedCommand;
        private ICommand _vodTypeSelectionChangedCommand;
        private ICommand _watchChannelCommand;
        private ICommand _watchEpisodeCommand;
        private ICommand _watchVlcChannelCommand;
        private ICommand _watchVlcEpisodeCommand;
        private ICommand _watchMpvChannelCommand;
        private ICommand _watchMpvEpisodeCommand;
        private ICommand _copyUrlChannelCommand;
        private ICommand _copyUrlEpisodeCommand;
        private ICommand _downloadChannelCommand;
        private ICommand _downloadEpisodeCommand;
        private ICommand _setRecordingLocationCommand;
        private ICommand _saveVideoDialogLayoutCommand;
        private ICommand _openVideoDialogLayoutCommand;
        private ICommand _deleteCredentialCommand;

        private string _token;
        private string _vlcExeLocation;
        private string _mpvExeLocation;
        private ObservableCollection<IVideoDialogViewModel> _videoDialogViewModels;
        private ObservableCollection<Season> _seasons;
        private ObservableCollection<Series> _series;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Session> _sessions;
        private ObservableCollection<Session> _liveSessions;
        private ObservableCollection<Channel> _channels;
        private ObservableCollection<VodType> _vodTypes;
        private ObservableCollection<Episode> _episodes;
        private Season _selectedSeason;
        private Event _selectedEvent;
        private Session _selectedLiveSession;
        private Session _selectedSession;
        private VodType _selectedVodType;
        private Timer _refreshLiveSessionsTimer;

        public MainWindowViewModel(
            ILogger logger,
            IExtendedDialogService dialogService,
            IApiService apiService,
            IGithubService githubService,
            ICredentialService credentialService,
            IStreamlinkLauncher streamlinkLauncher,
            ISettings settings,
            IVideoDialogLayout videoDialogLayout)
            : base(logger)
        {
            _dialogService = dialogService;
            _apiService = apiService;
            _githubService = githubService;
            _credentialService = credentialService;
            _streamlinkLauncher = streamlinkLauncher;
            Settings = settings;
            VideoDialogLayout = videoDialogLayout;
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
        public ICommand VodTypeSelectionChangedCommand => _vodTypeSelectionChangedCommand ??= new DelegateCommand(VodTypeSelectionChangedExecute);
        public ICommand WatchChannelCommand => _watchChannelCommand ??= new DelegateCommand<Channel>(WatchChannelExecute, CanWatchChannelExecute).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand WatchEpisodeCommand => _watchEpisodeCommand ??= new DelegateCommand<Episode>(WatchEpisodeExecute);
        public ICommand WatchVlcChannelCommand => _watchVlcChannelCommand ??= new DelegateCommand<Channel>(WatchVlcChannelExecute, CanWatchVlcChannelExecute).ObservesProperty(() => VlcExeLocation).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand WatchVlcEpisodeCommand => _watchVlcEpisodeCommand ??= new DelegateCommand<Episode>(WatchVlcEpisodeExecute, CanWatchVlcEpisodeExecute).ObservesProperty(() => VlcExeLocation);
        public ICommand WatchMpvChannelCommand => _watchMpvChannelCommand ??= new DelegateCommand<Channel>(WatchMpvChannelExecute, CanWatchMpvChannelExecute).ObservesProperty(() => MpvExeLocation).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand WatchMpvEpisodeCommand => _watchMpvEpisodeCommand ??= new DelegateCommand<Episode>(WatchMpvEpisodeExecute, CanWatchMpvEpisodeExecute).ObservesProperty(() => MpvExeLocation);
        public ICommand CopyUrlChannelCommand => _copyUrlChannelCommand ??= new DelegateCommand<Channel>(CopyUrlChannelExecute, CanCopyUrlChannelExecute).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand CopyUrlEpisodeCommand => _copyUrlEpisodeCommand ??= new DelegateCommand<Episode>(CopyUrlEpisodeExecute);
        public ICommand DownloadChannelCommand => _downloadChannelCommand ??= new DelegateCommand<Channel>(DownloadChannelExecute, CanDownloadChannelExecute).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand DownloadEpisodeCommand => _downloadEpisodeCommand ??= new DelegateCommand<Episode>(DownloadEpisodeExecute);
        public ICommand SetRecordingLocationCommand => _setRecordingLocationCommand ??= new DelegateCommand(SetRecordingLocationExecute);
        public ICommand SaveVideoDialogLayoutCommand => _saveVideoDialogLayoutCommand ??= new DelegateCommand(SaveVideoDialogLayoutExecute, CanSaveVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogViewModels.Count);
        public ICommand OpenVideoDialogLayoutCommand => _openVideoDialogLayoutCommand ??= new DelegateCommand(OpenVideoDialogLayoutExecute, CanOpenVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogLayout.Instances.Count).ObservesProperty(() => Channels.Count).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand DeleteCredentialCommand => _deleteCredentialCommand ??= new DelegateCommand(DeleteCredentialExecute);

        public ISettings Settings { get; }

        public IVideoDialogLayout VideoDialogLayout { get; }

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

        public ObservableCollection<IVideoDialogViewModel> VideoDialogViewModels
        {
            get => _videoDialogViewModels ??= new ObservableCollection<IVideoDialogViewModel>();
            set => SetProperty(ref _videoDialogViewModels, value);
        }

        public ObservableCollection<Season> Seasons
        {
            get => _seasons ??= new ObservableCollection<Season>();
            set => SetProperty(ref _seasons, value);
        }

        public ObservableCollection<Series> Series
        {
            get => _series ??= new ObservableCollection<Series>();
            set => SetProperty(ref _series, value);
        }

        public ObservableCollection<Event> Events
        {
            get => _events ??= new ObservableCollection<Event>();
            set => SetProperty(ref _events, value);
        }

        public ObservableCollection<Session> Sessions
        {
            get => _sessions ??= new ObservableCollection<Session>();
            set => SetProperty(ref _sessions, value);
        }

        public ObservableCollection<Session> LiveSessions
        {
            get => _liveSessions ??= new ObservableCollection<Session>();
            set => SetProperty(ref _liveSessions, value);
        }

        public ObservableCollection<Channel> Channels
        {
            get => _channels ??= new ObservableCollection<Channel>();
            set => SetProperty(ref _channels, value);
        }

        public ObservableCollection<VodType> VodTypes
        {
            get => _vodTypes ??= new ObservableCollection<VodType>();
            set => SetProperty(ref _vodTypes, value);
        }

        public ObservableCollection<Episode> Episodes
        {
            get => _episodes ??= new ObservableCollection<Episode>();
            set => SetProperty(ref _episodes, value);
        }

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

        public VodType SelectedVodType
        {
            get => _selectedVodType;
            set => SetProperty(ref _selectedVodType, value);
        }

        private async void LoadedExecute(RoutedEventArgs args)
        {
            Logger.Info("Initializing application...");
            IsBusy = true;
            Settings.Load();
            VideoDialogLayout.Load();
            SetVlcExeLocation();
            SetMpvExeLocation();

            await CheckForUpdatesAsync();

            if (Login())
            {
                await InitializeAsync();
            }

            IsBusy = false;
        }

        private void ClosingExecute()
        {
            if (_refreshLiveSessionsTimer != null)
            {
                _refreshLiveSessionsTimer.Stop();
                _refreshLiveSessionsTimer.Elapsed -= RefreshLiveSessionsTimer_Elapsed;
                _refreshLiveSessionsTimer.Dispose();
                _refreshLiveSessionsTimer = null;
            }

            Settings.Save();
            Logger.Info("Closing application...");
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

        private async void SeasonSelectionChangedExecute()
        {
            ClearEvents();

            if (SelectedSeason != null)
            {
                IsBusy = true;

                try
                {
                    var events = await _apiService.GetEventsForSeasonAsync(SelectedSeason.UID);
                    Events.AddRange(events);
                }
                catch (Exception ex)
                {
                    var message = $"An error occurred while trying to load events for race season '{SelectedSeason.Name}'.";
                    Logger.Error(ex, message);
                    MessageBoxHelper.ShowError(message);
                }

                IsBusy = false;
            }
        }

        private async void EventSelectionChangedExecute()
        {
            ClearSessions();

            if (SelectedEvent != null)
            {
                IsBusy = true;

                try
                {
                    var sessions = await _apiService.GetSessionsForEventAsync(SelectedEvent.UID);
                    Sessions.AddRange(sessions);
                }
                catch (Exception ex)
                {
                    var message = $"An error occurred while trying to load sessions for event '{SelectedEvent.Name}'.";
                    Logger.Error(ex, message);
                    MessageBoxHelper.ShowError(message);
                }

                IsBusy = false;
            }
        }

        private async void LiveSessionSelectionChangedExecute()
        {
            if (SelectedLiveSession != null)
            {
                IsBusy = true;
                SelectedSession = null;
                await SelectSessionAsync(SelectedLiveSession);
                IsBusy = false;
            }
        }

        private async void SessionSelectionChangedExecute()
        {
            if (SelectedSession != null)
            {
                IsBusy = true;
                SelectedLiveSession = null;
                await SelectSessionAsync(SelectedSession);
                IsBusy = false;
            }
        }

        private async void VodTypeSelectionChangedExecute()
        {
            if (SelectedVodType != null)
            {
                Episodes.Clear();
                Channels.Clear();
                SelectedLiveSession = null;
                SelectedSession = null;

                if (SelectedVodType.ContentUrls.Any())
                {
                    IsBusy = true;

                    try
                    {
                        var episodes = await DownloadHelper.BufferedDownload(_apiService.GetEpisodeAsync, SelectedVodType.ContentUrls.Select(c => c.GetUID()));
                        Episodes.AddRange(episodes.OrderBy(e => e.Title));
                    }
                    catch (Exception ex)
                    {
                        var message = $"An error occurred while trying to load episodes for VOD-type '{SelectedVodType.Name}'.";
                        Logger.Error(ex, message);
                        MessageBoxHelper.ShowError(message);
                    }

                    IsBusy = false;
                }
            }
        }

        private bool CanWatchChannelExecute(Channel channel)
        {
            return GetSelectedSession() != null;
        }

        private void WatchChannelExecute(Channel channel)
        {
            var session = GetSelectedSession();
            var playableContent = PlayableContent.Create(session, channel);
            WatchChannel(playableContent);
        }

        private void WatchEpisodeExecute(Episode episode)
        {
            var playableContent = PlayableContent.Create(episode);
            var parameters = new DialogParameters
            {
                { ParameterNames.TOKEN, _token },
                { ParameterNames.PLAYABLE_CONTENT, playableContent },
                { ParameterNames.INSTANCE, null }
            };

            OpenVideoDialog(parameters);
        }

        private bool CanWatchVlcChannelExecute(Channel channel)
        {
            return !string.IsNullOrWhiteSpace(VlcExeLocation) && File.Exists(VlcExeLocation) && GetSelectedSession() != null;
        }

        private async void WatchVlcChannelExecute(Channel channel)
        {
            IsBusy = true;
            var session = GetSelectedSession();
            var playableContent = PlayableContent.Create(session, channel);

            try
            {
                var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
                WatchStreamInVlc(playableContent, streamUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to watch channel '{playableContent.Title}' in VLC.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            IsBusy = false;
        }

        private bool CanWatchVlcEpisodeExecute(Episode episode)
        {
            return !string.IsNullOrWhiteSpace(VlcExeLocation) && File.Exists(VlcExeLocation);
        }

        private async void WatchVlcEpisodeExecute(Episode episode)
        {
            IsBusy = true;
            var playableContent = PlayableContent.Create(episode);

            try
            {
                var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
                WatchStreamInVlc(playableContent, streamUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to watch episode '{playableContent.Title}' in VLC.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            IsBusy = false;
        }

        private bool CanWatchMpvChannelExecute(Channel channel)
        {
            return !string.IsNullOrWhiteSpace(MpvExeLocation) && File.Exists(MpvExeLocation) && GetSelectedSession() != null;
        }

        private async void WatchMpvChannelExecute(Channel channel)
        {
            IsBusy = true;
            var session = GetSelectedSession();
            var playableContent = PlayableContent.Create(session, channel);

            try
            {
                var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
                WatchStreamInMpv(playableContent, streamUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to watch channel '{playableContent.Title}' in MPV.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            IsBusy = false;
        }

        private bool CanWatchMpvEpisodeExecute(Episode episode)
        {
            return !string.IsNullOrWhiteSpace(MpvExeLocation) && File.Exists(MpvExeLocation);
        }

        private async void WatchMpvEpisodeExecute(Episode episode)
        {
            IsBusy = true;
            var playableContent = PlayableContent.Create(episode);

            try
            {
                var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
                WatchStreamInMpv(playableContent, streamUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to watch episode '{playableContent.Title}' in MPV.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            IsBusy = false;
        }

        private bool CanCopyUrlChannelExecute(Channel channel)
        {
            return GetSelectedSession() != null;
        }

        private async void CopyUrlChannelExecute(Channel channel)
        {
            IsBusy = true;
            var session = GetSelectedSession();
            var playableContent = PlayableContent.Create(session, channel);

            try
            {
                var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
                Clipboard.SetText(streamUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to copy URL for channel '{channel.Name}' to clipboard.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            IsBusy = false;
        }

        private async void CopyUrlEpisodeExecute(Episode episode)
        {
            IsBusy = true;
            var playableContent = PlayableContent.Create(episode);

            try
            {
                var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
                Clipboard.SetText(streamUrl);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to copy URL for episode '{episode.Title}' to clipboard.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            IsBusy = false;
        }

        private bool CanDownloadChannelExecute(Channel channel)
        {
            return GetSelectedSession()?.IsReplay ?? false;
        }

        private void DownloadChannelExecute(Channel channel)
        {
            var session = GetSelectedSession();
            var playableContent = PlayableContent.Create(session, channel);
            StartDownload(playableContent);
        }

        private void DownloadEpisodeExecute(Episode episode)
        {
            var playableContent = PlayableContent.Create(episode);
            StartDownload(playableContent);
        }

        private void SetRecordingLocationExecute()
        {
            if (_dialogService.SelectFolder("Select a recording location", Settings.RecordingLocation, out var recordingLocation))
            {
                Settings.RecordingLocation = recordingLocation;
            }
        }

        private bool CanSaveVideoDialogLayoutExecute()
        {
            return VideoDialogViewModels.Any(vm => vm.PlayableContent.ContentType == ContentType.Channel);
        }

        private void SaveVideoDialogLayoutExecute()
        {
            VideoDialogLayout.Clear();
            VideoDialogLayout.Add(VideoDialogViewModels.Where(vm => vm.PlayableContent.ContentType == ContentType.Channel).Select(vm => vm.GetVideoDialogInstance()));

            if (VideoDialogLayout.Save())
            {
                MessageBoxHelper.ShowInfo("The current window layout has been successfully saved.", "Layout");
            }
        }

        private bool CanOpenVideoDialogLayoutExecute()
        {
            return VideoDialogLayout.Instances.Any() && Channels.Count > 1 && GetSelectedSession() != null;
        }

        private void OpenVideoDialogLayoutExecute()
        {
            var viewModelsToClose = VideoDialogViewModels.Where(vm => vm.PlayableContent.ContentType == ContentType.Channel).ToList();

            foreach (var viewModel in viewModelsToClose)
            {
                viewModel.CloseWindow();
            }

            var session = GetSelectedSession();
            var playableContents = Channels.Select(channel => PlayableContent.Create(session, channel)).ToList();

            foreach (var instance in VideoDialogLayout.Instances)
            {
                var playableContent = playableContents.FirstOrDefault(p => p.ContentType == ContentType.Channel && p.Name == instance.ChannelName);

                if (playableContent != null)
                {
                    WatchChannel(playableContent, instance);
                }
            }
        }

        private void DeleteCredentialExecute()
        {
            IsBusy = true;

            if (MessageBoxHelper.AskQuestion("Are you sure you want to delete your credentials from this system?", "Credentials"))
            {
                _credentialService.DeleteCredential();
                Login();
            }

            IsBusy = false;
        }

        private async void RefreshLiveSessionsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _refreshLiveSessionsTimer?.Stop();
            await RefreshLiveSessionsAsync();
            _refreshLiveSessionsTimer?.Start();
        }

        private void SetVlcExeLocation()
        {
            var registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC") ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\VideoLAN\VLC");

            if (registryKey != null && registryKey.GetValue(null) is string vlcExeLocation && File.Exists(vlcExeLocation))
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

            Release release;

            try
            {
                release = await _githubService.GetLatestRelease();
            }
            catch (Exception ex)
            {
                const string message = "An error occurred while checking for updates.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
                return;
            }

            if (release == null || release.PreRelease || release.Draft || release.TagName == Settings.LatestRelease)
            {
                Logger.Info("No new release found.");
            }
            else if (Version.TryParse(release.TagName, out var version) && version > AssemblyUtils.GetApplicationVersion())
            {
                Logger.Info($"Found new release '{release.Name}'.");

                var parameters = new DialogParameters
                {
                    { ParameterNames.RELEASE, release }
                };

                _dialogService.ShowDialog(nameof(UpgradeDialog), parameters, dialogResult =>
                {
                    if (dialogResult.Result == ButtonResult.OK)
                    {
                        ProcessUtils.BrowseToUrl(release.HtmlUrl);
                    }
                });

                Settings.LatestRelease = release.TagName;
            }

            Logger.Info("Done checking for updates.");
        }

        private bool Login()
        {
            var success = false;

            _dialogService.ShowDialog(nameof(LoginDialog), null, dialogResult =>
            {
                success = dialogResult.Result == ButtonResult.OK;

                if (success)
                {
                    _token = dialogResult.Parameters.GetValue<string>(ParameterNames.TOKEN);
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
            await Task.WhenAll(
                LoadSeasonsAsync(),
                LoadSeriesAsync(),
                LoadVodTypesAsync(),
                RefreshLiveSessionsAsync());

            SelectedSeason = Seasons.FirstOrDefault();

            _refreshLiveSessionsTimer = new Timer(60000) { AutoReset = false };
            _refreshLiveSessionsTimer.Elapsed += RefreshLiveSessionsTimer_Elapsed;
            _refreshLiveSessionsTimer.Start();
        }

        private async Task LoadSeasonsAsync()
        {
            try
            {
                var seasons = await _apiService.GetSeasonsAsync();
                Seasons.AddRange(seasons);
            }
            catch (Exception ex)
            {
                const string message = "An error occurred while trying to load race seasons.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }
        }

        private async Task LoadSeriesAsync()
        {
            try
            {
                var series = await _apiService.GetSeriesAsync();
                Series.AddRange(series);
            }
            catch (Exception ex)
            {
                const string message = "An error occurred while trying to load series.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }

            if (!Settings.SelectedSeries.Any())
            {
                var f1Series = Series.FirstOrDefault(s => s.Name == "Formula 1" && s.HasContent);

                if (f1Series != null)
                {
                    Settings.SelectedSeries.Add(f1Series.Self);
                }
            }
        }

        private async Task LoadVodTypesAsync()
        {
            try
            {
                var vodTypes = await _apiService.GetVodTypesAsync();
                VodTypes.AddRange(vodTypes.Where(vt => vt.ContentUrls.Any()));
            }
            catch (Exception ex)
            {
                const string message = "An error occurred while trying to load VOD-types.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }
        }

        private async Task RefreshLiveSessionsAsync()
        {
            Logger.Info("Refreshing live sessions...");

            IList<Session> liveSessions;

            try
            {
                liveSessions = (await _apiService.GetLiveSessionsAsync()).Where(session => session.IsLive).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while refreshing live sessions.");
                return;
            }

            var sessionsToRemove = LiveSessions.Where(existingLiveSession => liveSessions.All(liveSession => liveSession.UID != existingLiveSession.UID)).ToList();
            var sessionsToAdd = liveSessions.Where(newLiveSession => LiveSessions.All(liveSession => liveSession.UID != newLiveSession.UID)).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var sessionToRemove in sessionsToRemove)
                {
                    if (SelectedLiveSession != null && SelectedLiveSession.UID == sessionToRemove.UID)
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

            Logger.Info("Done refreshing live sessions.");
        }

        private async Task SelectSessionAsync(Session session)
        {
            Episodes.Clear();
            Channels.Clear();
            SelectedVodType = null;

            await Task.WhenAll(LoadChannelsForSessionAsync(session), LoadEpisodesForSessionAsync(session));
        }

        private async Task LoadChannelsForSessionAsync(Session session)
        {
            try
            {
                var channels = await _apiService.GetChannelsForSessionAsync(session.UID);

                if (session.IsLive && channels.Count > 1)
                {
                    channels.Add(new Channel
                    {
                        ChannelType = ChannelTypes.BACKUP,
                        Name = "Backup stream"
                    });
                }

                Channels.AddRange(channels.OrderBy(c => c.ChannelType, new ChannelTypeComparer()));
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to load channels for session '{session.SessionName}'.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }
        }

        private async Task LoadEpisodesForSessionAsync(Session session)
        {
            try
            {
                var episodes = await _apiService.GetEpisodesForSessionAsync(session.UID);
                Episodes.AddRange(episodes.OrderBy(e => e.Title));
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while trying to load episodes for session '{session.SessionName}'.";
                Logger.Error(ex, message);
                MessageBoxHelper.ShowError(message);
            }
        }

        private void WatchChannel(IPlayableContent playableContent, VideoDialogInstance instance = null)
        {
            var parameters = new DialogParameters
            {
                { ParameterNames.TOKEN, _token },
                { ParameterNames.PLAYABLE_CONTENT, playableContent },
                { ParameterNames.INSTANCE, instance }
            };

            OpenVideoDialog(parameters);
        }

        private void OpenVideoDialog(IDialogParameters parameters)
        {
            var viewModel = (IVideoDialogViewModel)_dialogService.Show(nameof(VideoDialog), parameters, OnVideoDialogClosed, false, nameof(VideoDialogWindow));
            VideoDialogViewModels.Add(viewModel);
        }

        private void OnVideoDialogClosed(IDialogResult result)
        {
            var uniqueIdentifier = result.Parameters.GetValue<Guid>(ParameterNames.UNIQUE_IDENTIFIER);
            var viewModel = VideoDialogViewModels.FirstOrDefault(vm => vm.UniqueIdentifier == uniqueIdentifier);

            if (viewModel != null)
            {
                VideoDialogViewModels.Remove(viewModel);
            }
        }

        private void WatchStreamInVlc(IPlayableContent playableContent, string streamUrl)
        {
            if (playableContent.IsLive && !Settings.DisableStreamlink)
            {
                _streamlinkLauncher.StartStreamlinkVlc(VlcExeLocation, streamUrl, playableContent.Title);
            }
            else
            {
                using var process = ProcessUtils.CreateProcess(VlcExeLocation, $"\"{streamUrl}\" --meta-title=\"{playableContent.Title}\"");
                process.Start();
            }
        }

        private void WatchStreamInMpv(IPlayableContent playableContent, string streamUrl)
        {
            if (playableContent.IsLive && !Settings.DisableStreamlink)
            {
                _streamlinkLauncher.StartStreamlinkMpv(MpvExeLocation, streamUrl, playableContent.Title);
            }
            else
            {
                using var process = ProcessUtils.CreateProcess(MpvExeLocation, $"\"{streamUrl}\" --title=\"{playableContent.Title}\"");
                process.Start();
            }
        }

        private void StartDownload(IPlayableContent playableContent)
        {
            var defaultFilename = $"{playableContent.Title}.ts".RemoveInvalidFileNameChars();

            if (_dialogService.SelectFile("Select a filename", Settings.RecordingLocation, defaultFilename, ".ts", out var filename))
            {
                var parameters = new DialogParameters
                {
                    { ParameterNames.TOKEN, _token },
                    { ParameterNames.PLAYABLE_CONTENT, playableContent},
                    { ParameterNames.FILENAME, filename }
                };

                _dialogService.Show(nameof(DownloadDialog), parameters, null);
            }
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
            SelectedVodType = null;
        }

        private Session GetSelectedSession() => SelectedLiveSession ?? SelectedSession;
    }
}