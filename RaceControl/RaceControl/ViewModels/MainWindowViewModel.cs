using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
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
using System.Threading.Tasks.Dataflow;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string NotApplicableSeriesUID = @"seri_77d8d2aa25fb4da08bada2d93ec0dd1f";
        private const string Formula1SeriesUID = @"seri_436bb431c3a24d7d8e200a74e1d11de4";

        private readonly ILogger _logger;
        private readonly IExtendedDialogService _dialogService;
        private readonly IApiService _apiService;
        private readonly IGithubService _githubService;
        private readonly ICredentialService _credentialService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;

        private ICommand _loadedCommand;
        private ICommand _closingCommand;
        private ICommand _mouseMoveCommand;
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

        private ISettings _settings;
        private IVideoDialogLayout _videoDialogLayout;
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
        {
            _logger = logger;
            _dialogService = dialogService;
            _apiService = apiService;
            _githubService = githubService;
            _credentialService = credentialService;
            _streamlinkLauncher = streamlinkLauncher;
            _settings = settings;
            _videoDialogLayout = videoDialogLayout;
        }

        public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
        public ICommand ClosingCommand => _closingCommand ??= new DelegateCommand(ClosingExecute);
        public ICommand MouseMoveCommand => _mouseMoveCommand ??= new DelegateCommand(MouseMoveExecute);
        public ICommand SeasonSelectionChangedCommand => _seasonSelectionChangedCommand ??= new DelegateCommand(SeasonSelectionChangedExecute);
        public ICommand EventSelectionChangedCommand => _eventSelectionChangedCommand ??= new DelegateCommand(EventSelectionChangedExecute);
        public ICommand LiveSessionSelectionChangedCommand => _liveSessionSelectionChangedCommand ??= new DelegateCommand(LiveSessionSelectionChangedExecute);
        public ICommand SessionSelectionChangedCommand => _sessionSelectionChangedCommand ??= new DelegateCommand(SessionSelectionChangedExecute);
        public ICommand VodTypeSelectionChangedCommand => _vodTypeSelectionChangedCommand ??= new DelegateCommand(VodTypeSelectionChangedExecute);
        public ICommand WatchChannelCommand => _watchChannelCommand ??= new DelegateCommand<Channel>(WatchChannelExecute);
        public ICommand WatchEpisodeCommand => _watchEpisodeCommand ??= new DelegateCommand<Episode>(WatchEpisodeExecute);
        public ICommand WatchVlcChannelCommand => _watchVlcChannelCommand ??= new DelegateCommand<Channel>(WatchVlcChannelExecute, CanWatchVlcChannelExecute).ObservesProperty(() => VlcExeLocation);
        public ICommand WatchVlcEpisodeCommand => _watchVlcEpisodeCommand ??= new DelegateCommand<Episode>(WatchVlcEpisodeExecute, CanWatchVlcEpisodeExecute).ObservesProperty(() => VlcExeLocation);
        public ICommand WatchMpvChannelCommand => _watchMpvChannelCommand ??= new DelegateCommand<Channel>(WatchMpvChannelExecute, CanWatchMpvChannelExecute).ObservesProperty(() => MpvExeLocation);
        public ICommand WatchMpvEpisodeCommand => _watchMpvEpisodeCommand ??= new DelegateCommand<Episode>(WatchMpvEpisodeExecute, CanWatchMpvEpisodeExecute).ObservesProperty(() => MpvExeLocation);
        public ICommand CopyUrlChannelCommand => _copyUrlChannelCommand ??= new DelegateCommand<Channel>(CopyUrlChannelExecute);
        public ICommand CopyUrlEpisodeCommand => _copyUrlEpisodeCommand ??= new DelegateCommand<Episode>(CopyUrlEpisodeExecute);
        public ICommand DownloadChannelCommand => _downloadChannelCommand ??= new DelegateCommand<Channel>(DownloadChannelExecute, CanDownloadChannelExecute).ObservesProperty(() => SelectedSession).ObservesProperty(() => SelectedLiveSession);
        public ICommand DownloadEpisodeCommand => _downloadEpisodeCommand ??= new DelegateCommand<Episode>(DownloadEpisodeExecute);
        public ICommand SetRecordingLocationCommand => _setRecordingLocationCommand ??= new DelegateCommand(SetRecordingLocationExecute);
        public ICommand SaveVideoDialogLayoutCommand => _saveVideoDialogLayoutCommand ??= new DelegateCommand(SaveVideoDialogLayoutExecute, CanSaveVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogViewModels.Count);
        public ICommand OpenVideoDialogLayoutCommand => _openVideoDialogLayoutCommand ??= new DelegateCommand(OpenVideoDialogLayoutExecute, CanOpenVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogLayout.Instances.Count).ObservesProperty(() => Channels.Count);
        public ICommand DeleteCredentialCommand => _deleteCredentialCommand ??= new DelegateCommand(DeleteCredentialExecute);

        public ISettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public IVideoDialogLayout VideoDialogLayout
        {
            get => _videoDialogLayout;
            set => SetProperty(ref _videoDialogLayout, value);
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
            IsBusy = true;
            Settings.Load();
            VideoDialogLayout.Load();

            try
            {
                await CheckForUpdatesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while checking for updates.");
            }

            IsBusy = false;

            _dialogService.ShowDialog(nameof(LoginDialog), null, async dialogResult =>
            {
                if (dialogResult.Result == ButtonResult.OK)
                {
                    var token = dialogResult.Parameters.GetValue<string>(ParameterNames.TOKEN);
                    await InitializeAsync(token);
                }
                else
                {
                    _logger.Info("Login cancelled by user, shutting down...");
                    Application.Current.Shutdown();
                }
            });
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

        private async void SeasonSelectionChangedExecute()
        {
            IsBusy = true;
            ClearEvents();

            if (SelectedSeason != null)
            {
                var events = await _apiService.GetEventsForSeasonAsync(SelectedSeason.UID);
                Events.AddRange(events);
            }

            IsBusy = false;
        }

        private async void EventSelectionChangedExecute()
        {
            IsBusy = true;
            ClearSessions();

            if (SelectedEvent != null)
            {
                var sessions = await _apiService.GetSessionsForEventAsync(SelectedEvent.UID);
                Sessions.AddRange(sessions.Where(s => s.IsLive || s.IsReplay));
            }

            IsBusy = false;
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
                IsBusy = true;
                SelectedLiveSession = null;
                SelectedSession = null;
                ClearChannels();
                ClearEpisodes();

                if (SelectedVodType.ContentUrls.Any())
                {
                    // Limit number of concurrent requests to 50
                    var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 50 };
                    var downloader = new TransformBlock<string, Episode>(episodeUID => _apiService.GetEpisodeAsync(episodeUID), options);
                    var buffer = new BufferBlock<Episode>();
                    downloader.LinkTo(buffer);

                    foreach (var contentUrl in SelectedVodType.ContentUrls)
                    {
                        await downloader.SendAsync(contentUrl.GetUID());
                    }

                    downloader.Complete();
                    await downloader.Completion;

                    if (buffer.TryReceiveAll(out var episodes))
                    {
                        Episodes.AddRange(episodes.OrderBy(e => e.Title));
                    }
                }

                IsBusy = false;
            }
        }

        private void WatchChannelExecute(Channel channel)
        {
            var session = GetCurrentSession();

            WatchChannel(session, channel);
        }

        private void WatchEpisodeExecute(Episode episode)
        {
            var title = episode.ToString();
            var parameters = new DialogParameters
            {
                { ParameterNames.TITLE, title },
                { ParameterNames.NAME, title },
                { ParameterNames.TOKEN, _token },
                { ParameterNames.CONTENT_TYPE, ContentType.Asset },
                { ParameterNames.CONTENT_URL, episode.Items.First() },
                { ParameterNames.SYNC_UID, episode.UID },
                { ParameterNames.IS_LIVE, false },
                { ParameterNames.INSTANCE, null }
            };

            _logger.Info($"Starting internal player for episode with parameters: '{parameters}'.");
            OpenVideoDialog(parameters);
        }

        private bool CanWatchVlcChannelExecute(Channel channel)
        {
            return !string.IsNullOrWhiteSpace(VlcExeLocation) && File.Exists(VlcExeLocation);
        }

        private async void WatchVlcChannelExecute(Channel channel)
        {
            IsBusy = true;
            var session = GetCurrentSession();
            var title = GetTitle(session, channel);

            try
            {
                var url = await _apiService.GetTokenisedUrlForChannelAsync(_token, channel.Self);
                WatchStreamInVlc(url, title, session.IsLive);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while trying to watch a channel in VLC.");
                MessageBoxHelper.ShowError(ex.Message);
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
            var title = episode.ToString();

            try
            {
                var url = await _apiService.GetTokenisedUrlForAssetAsync(_token, episode.Items.First());
                WatchStreamInVlc(url, title, false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while trying to watch an episode in VLC.");
                MessageBoxHelper.ShowError(ex.Message);
            }

            IsBusy = false;
        }

        private bool CanWatchMpvChannelExecute(Channel channel)
        {
            return !string.IsNullOrWhiteSpace(MpvExeLocation) && File.Exists(MpvExeLocation);
        }

        private async void WatchMpvChannelExecute(Channel channel)
        {
            IsBusy = true;
            var session = GetCurrentSession();
            var title = GetTitle(session, channel);

            try
            {
                var url = await _apiService.GetTokenisedUrlForChannelAsync(_token, channel.Self);
                WatchStreamInMpv(url, title, session.IsLive);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while trying to watch a channel in MPV.");
                MessageBoxHelper.ShowError(ex.Message);
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
            var title = episode.ToString();

            try
            {
                var url = await _apiService.GetTokenisedUrlForAssetAsync(_token, episode.Items.First());
                WatchStreamInMpv(url, title, false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while trying to watch an episode in MPV.");
                MessageBoxHelper.ShowError(ex.Message);
            }

            IsBusy = false;
        }

        private async void CopyUrlChannelExecute(Channel channel)
        {
            IsBusy = true;

            try
            {
                var url = await _apiService.GetTokenisedUrlForChannelAsync(_token, channel.Self);
                Clipboard.SetText(url);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while copying a channel-URL to the clipboard.");
                MessageBoxHelper.ShowError(ex.Message);
            }

            IsBusy = false;
        }

        private async void CopyUrlEpisodeExecute(Episode episode)
        {
            IsBusy = true;

            try
            {
                var url = await _apiService.GetTokenisedUrlForAssetAsync(_token, episode.Items.First());
                Clipboard.SetText(url);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while copying an episode-URL to the clipboard.");
                MessageBoxHelper.ShowError(ex.Message);
            }

            IsBusy = false;
        }

        private bool CanDownloadChannelExecute(Channel channel)
        {
            var session = GetCurrentSession();

            return session != null && !session.IsLive;
        }

        private void DownloadChannelExecute(Channel channel)
        {
            var session = GetCurrentSession();
            var title = GetTitle(session, channel);
            PerformDownload(title, ContentType.Channel, channel.Self);
        }

        private void DownloadEpisodeExecute(Episode episode)
        {
            var title = episode.ToString();
            PerformDownload(title, ContentType.Asset, episode.Items.First());
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
            return VideoDialogViewModels.Any(vm => vm.ContentType == ContentType.Channel);
        }

        private void SaveVideoDialogLayoutExecute()
        {
            VideoDialogLayout.Clear();
            VideoDialogLayout.Add(VideoDialogViewModels.Where(vm => vm.ContentType == ContentType.Channel).Select(vm => vm.GetVideoDialogInstance()));

            if (VideoDialogLayout.Save())
            {
                MessageBoxHelper.ShowInfo("The current window layout has been successfully saved.");
            }
        }

        private bool CanOpenVideoDialogLayoutExecute()
        {
            return VideoDialogLayout.Instances.Any() && Channels.Count > 1;
        }

        private void OpenVideoDialogLayoutExecute()
        {
            var session = GetCurrentSession();

            foreach (var instance in VideoDialogLayout.Instances)
            {
                var channel = Channels.FirstOrDefault(c => c.Name == instance.ChannelName);

                if (channel != null && !VideoDialogViewModels.Any(vm => vm.ContentType == ContentType.Channel && vm.ContentUrl == channel.Self))
                {
                    WatchChannel(session, channel, instance);
                }
            }
        }

        private async void DeleteCredentialExecute()
        {
            IsBusy = true;

            if (MessageBoxHelper.AskQuestion("Are you sure you want to delete your credentials from this system?"))
            {
                var deleted = await Task.Run(() => _credentialService.DeleteCredential());

                if (deleted)
                {
                    MessageBoxHelper.ShowInfo("Your credentials have been successfully deleted from this system.");
                }
                else
                {
                    MessageBoxHelper.ShowError("Your credentials have already been deleted from this system.");
                }
            }

            IsBusy = false;
        }

        private async void RefreshLiveSessionsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _refreshLiveSessionsTimer?.Stop();
            await RefreshLiveSessionsAsync();
            _refreshLiveSessionsTimer?.Start();
        }

        private async Task InitializeAsync(string token)
        {
            IsBusy = true;
            SetToken(token);
            SetVlcExeLocation();
            SetMpvExeLocation();
            await LoadInitialDataAsync();
            IsBusy = false;

            _refreshLiveSessionsTimer = new Timer(60000) { AutoReset = false };
            _refreshLiveSessionsTimer.Elapsed += RefreshLiveSessionsTimer_Elapsed;
            _refreshLiveSessionsTimer.Start();
        }

        private async Task CheckForUpdatesAsync()
        {
            _logger.Info("Checking for updates...");

            var release = await _githubService.GetLatestRelease();

            if (release != null && !release.PreRelease && !release.Draft && Version.TryParse(release.TagName, out var latestVersion))
            {
                var currentVersion = AssemblyUtils.GetApplicationVersion();

                if (latestVersion > currentVersion)
                {
                    _logger.Info($"Found new release '{release.Name}'.");

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
                }
            }

            _logger.Info("Done checking for updates.");
        }

        private void SetToken(string token)
        {
            _token = token;
        }

        private void SetVlcExeLocation()
        {
            var vlcRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC") ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\VideoLAN\VLC");

            if (vlcRegistryKey != null && vlcRegistryKey.GetValue(null) is string vlcExeLocation && File.Exists(vlcExeLocation))
            {
                VlcExeLocation = vlcExeLocation;
                _logger.Info($"Found VLC installation at '{vlcExeLocation}'.");
            }
            else
            {
                _logger.Warn("Could not find VLC installation.");
            }
        }

        private void SetMpvExeLocation()
        {
            var mpvExeLocation = Path.Combine(Environment.CurrentDirectory, @"mpv\mpv.exe");

            if (File.Exists(mpvExeLocation))
            {
                MpvExeLocation = mpvExeLocation;
                _logger.Info($"Found MPV installation at '{mpvExeLocation}'.");
            }
            else
            {
                _logger.Warn("Could not find MPV installation.");
            }
        }

        private async Task LoadInitialDataAsync()
        {
            await Task.WhenAll(
                LoadSeasonsAsync(),
                LoadSeriesAsync(),
                LoadVodTypesAsync(),
                RefreshLiveSessionsAsync());
        }

        private async Task LoadSeasonsAsync()
        {
            var seasons = await _apiService.GetSeasonsAsync();
            Seasons.AddRange(seasons);
        }

        private async Task LoadSeriesAsync()
        {
            var series = (await _apiService.GetSeriesAsync()).Where(s => s.UID != NotApplicableSeriesUID);
            Series.AddRange(series);

            if (!Settings.SelectedSeries.Any())
            {
                var f1Series = Series.FirstOrDefault(s => s.UID == Formula1SeriesUID);

                if (f1Series != null)
                {
                    Settings.SelectedSeries.Add(f1Series.Self);
                }
            }
        }

        private async Task LoadVodTypesAsync()
        {
            var vodTypes = await _apiService.GetVodTypesAsync();
            VodTypes.AddRange(vodTypes.Where(vt => vt.ContentUrls.Any()));
        }

        private async Task RefreshLiveSessionsAsync()
        {
            _logger.Info("Refreshing live sessions...");

            IList<Session> liveSessions;

            try
            {
                liveSessions = (await _apiService.GetLiveSessionsAsync()).Where(session => session.IsLive).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while refreshing live sessions.");
                return;
            }

            var sessionsToRemove = LiveSessions.Where(existingLiveSession => liveSessions.All(liveSession => liveSession.UID != existingLiveSession.UID)).ToList();
            var sessionsToAdd = liveSessions.Where(newLiveSession => LiveSessions.All(liveSession => liveSession.UID != newLiveSession.UID)).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var sessionToRemove in sessionsToRemove)
                {
                    LiveSessions.Remove(sessionToRemove);
                }

                if (sessionsToAdd.Any())
                {
                    LiveSessions.AddRange(sessionsToAdd);
                }
            });

            _logger.Info("Done refreshing live sessions.");
        }

        private async Task SelectSessionAsync(Session session)
        {
            SelectedVodType = null;
            ClearChannels();
            ClearEpisodes();

            await Task.WhenAll(
                LoadChannelsForSessionAsync(session.UID),
                LoadEpisodesForSessionAsync(session.UID));
        }

        private async Task LoadChannelsForSessionAsync(string sessionUID)
        {
            var channels = await _apiService.GetChannelsForSessionAsync(sessionUID);
            Channels.AddRange(channels.OrderBy(c => c.ChannelType, new ChannelTypeComparer()));
        }

        private async Task LoadEpisodesForSessionAsync(string sessionUID)
        {
            var episodes = await _apiService.GetEpisodesForSessionAsync(sessionUID);
            Episodes.AddRange(episodes.OrderBy(e => e.Title));
        }

        private void WatchChannel(Session session, Channel channel, VideoDialogInstance instance = null)
        {
            var title = GetTitle(session, channel);
            var parameters = new DialogParameters
            {
                { ParameterNames.TITLE, title },
                { ParameterNames.NAME, channel.Name },
                { ParameterNames.TOKEN, _token },
                { ParameterNames.CONTENT_TYPE, ContentType.Channel },
                { ParameterNames.CONTENT_URL, channel.Self },
                { ParameterNames.SYNC_UID, session.UID },
                { ParameterNames.IS_LIVE, session.IsLive },
                { ParameterNames.INSTANCE, instance }
            };

            _logger.Info($"Starting internal player for channel with parameters: '{parameters}'.");
            OpenVideoDialog(parameters);
        }

        private void OpenVideoDialog(IDialogParameters parameters)
        {
            var viewModel = (IVideoDialogViewModel)_dialogService.Show(nameof(VideoDialog), parameters, OnVideoDialogClosed, false);
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

        private void PerformDownload(string title, ContentType contentType, string contentUrl)
        {
            var defaultFilename = $"{title}.mkv".RemoveInvalidFileNameChars();

            if (_dialogService.SelectFile("Select a filename", Settings.RecordingLocation, defaultFilename, ".mkv", out var filename))
            {
                var parameters = new DialogParameters
                {
                    { ParameterNames.NAME, title },
                    { ParameterNames.FILENAME, filename },
                    { ParameterNames.TOKEN, _token },
                    { ParameterNames.CONTENT_TYPE, contentType },
                    { ParameterNames.CONTENT_URL, contentUrl}
                };

                _logger.Info($"Starting download with parameters: '{parameters}'.");
                _dialogService.Show(nameof(DownloadDialog), parameters, null);
            }
        }

        private void WatchStreamInVlc(string url, string title, bool isLive)
        {
            if (isLive)
            {
                _streamlinkLauncher.StartStreamlinkVlc(VlcExeLocation, url, title);
            }
            else
            {
                ProcessUtils.CreateProcess(VlcExeLocation, $"{url} --meta-title=\"{title}\"").Start();
            }
        }

        private void WatchStreamInMpv(string url, string title, bool isLive)
        {
            if (isLive)
            {
                _streamlinkLauncher.StartStreamlinkMpv(MpvExeLocation, url, title);
            }
            else
            {
                ProcessUtils.CreateProcess(MpvExeLocation, $"{url} --title=\"{title}\"").Start();
            }
        }

        private void ClearEvents()
        {
            ClearSessions();
            Events.Clear();
        }

        private void ClearSessions()
        {
            ClearEpisodes();
            ClearChannels();
            Sessions.Clear();
            SelectedLiveSession = null;
        }

        private void ClearChannels()
        {
            Channels.Clear();
        }

        private void ClearEpisodes()
        {
            Episodes.Clear();
        }

        private Session GetCurrentSession() => SelectedLiveSession ?? SelectedSession;

        private static string GetTitle(Session session, Channel channel) => $"{session.SessionName} - {channel}";
    }
}