using LibVLCSharp.Shared;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Common.Utils;
using RaceControl.Comparers;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Github;
using RaceControl.Streamlink;
using RaceControl.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IExtendedDialogService _dialogService;
        private readonly IApiService _apiService;
        private readonly IGithubService _githubService;
        private readonly ICredentialService _credentialService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;
        private readonly LibVLC _libVLC;
        private readonly Timer _refreshLiveEventsTimer = new Timer(60000) { AutoReset = false };

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
        private ICommand _deleteCredentialCommand;

        private string _token;
        private string _vlcExeLocation;
        private string _mpvExeLocation;
        private bool _lowQualityMode;
        private bool _useAlternativeStream;
        private ObservableCollection<Season> _seasons;
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

        public MainWindowViewModel(
            ILogger logger,
            IExtendedDialogService dialogService,
            IApiService apiService,
            IGithubService githubService,
            ICredentialService credentialService,
            IStreamlinkLauncher streamlinkLauncher,
            LibVLC libVLC)
        {
            _logger = logger;
            _dialogService = dialogService;
            _apiService = apiService;
            _githubService = githubService;
            _credentialService = credentialService;
            _streamlinkLauncher = streamlinkLauncher;
            _libVLC = libVLC;
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
        public ICommand DeleteCredentialCommand => _deleteCredentialCommand ??= new DelegateCommand(DeleteCredentialExecute);

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

        public bool LowQualityMode
        {
            get => _lowQualityMode;
            set => SetProperty(ref _lowQualityMode, value);
        }

        public bool UseAlternativeStream
        {
            get => _useAlternativeStream;
            set => SetProperty(ref _useAlternativeStream, value);
        }

        public ObservableCollection<Season> Seasons
        {
            get => _seasons ??= new ObservableCollection<Season>();
            set => SetProperty(ref _seasons, value);
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

        private Session GetCurrentSession() => SelectedLiveSession ?? SelectedSession;

        private void LoadedExecute(RoutedEventArgs args)
        {
            _dialogService.ShowDialog(nameof(LoginDialog), null, async dialogResult =>
            {
                if (dialogResult.Result == ButtonResult.OK)
                {
                    var token = dialogResult.Parameters.GetValue<string>("token");
                    await Initialize(token);
                }
                else
                {
                    _logger.Info("Login cancelled by user.");
                    Application.Current.Shutdown();
                }
            });
        }

        private async Task Initialize(string token)
        {
            IsBusy = true;

            try
            {
                await CheckForUpdates();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An exception occurred while checking for updates.");
            }

            SetToken(token);
            SetVlcExeLocation();
            SetMpvExeLocation();
            Seasons.AddRange((await _apiService.GetRaceSeasonsAsync()).Where(s => s.EventOccurrenceUrls.Any()));
            VodTypes.AddRange((await _apiService.GetVodTypesAsync()).Where(v => v.ContentUrls.Any()));
            IsBusy = false;

            await RefreshLiveEvents();
            _refreshLiveEventsTimer.Elapsed += RefreshLiveEventsTimer_Elapsed;
            _refreshLiveEventsTimer.Start();
        }

        private void SetToken(string token)
        {
            _token = token;
        }

        private async Task CheckForUpdates()
        {
            _logger.Info("Checking for updates...");

            var release = await _githubService.GetLatestRelease();

            if (release != null && !release.PreRelease && !release.Draft && Version.TryParse(release.TagName, out var latestVersion))
            {
                var currentVersion = Assembly.GetEntryAssembly().GetName().Version;

                if (latestVersion > currentVersion)
                {
                    _logger.Info($"Found new release '{release.Name}'.");

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
            }

            _logger.Info("Done checking for updates.");
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
                _logger.Info("Could not find VLC installation.");
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
                _logger.Info("Could not find MPV installation.");
            }
        }

        private void ClosingExecute()
        {
            _refreshLiveEventsTimer.Elapsed -= RefreshLiveEventsTimer_Elapsed;
            _refreshLiveEventsTimer.Stop();
            _refreshLiveEventsTimer.Dispose();
        }

        private void MouseMoveExecute()
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

            if (SelectedSeason != null && SelectedSeason.EventOccurrenceUrls.Any())
            {
                var events = new ConcurrentBag<Event>();
                var tasks = SelectedSeason.EventOccurrenceUrls.Select(async eventUrl =>
                {
                    events.Add(await _apiService.GetEventAsync(eventUrl.GetUID()));
                });
                await Task.WhenAll(tasks);
                Events.AddRange(events.OrderBy(e => e.StartDate));
            }

            IsBusy = false;
        }

        private async void EventSelectionChangedExecute()
        {
            IsBusy = true;
            ClearSessions();

            if (SelectedEvent != null && SelectedEvent.SessionOccurrenceUrls.Any())
            {
                var sessions = new ConcurrentBag<Session>();
                var tasks = SelectedEvent.SessionOccurrenceUrls.Select(async sessionUrl =>
                {
                    sessions.Add(await _apiService.GetSessionAsync(sessionUrl.GetUID()));
                });
                await Task.WhenAll(tasks);
                Sessions.AddRange(sessions.Where(s => s.IsLive || s.IsReplay).OrderBy(s => s.StartTime));
            }

            IsBusy = false;
        }

        private async void LiveSessionSelectionChangedExecute()
        {
            if (SelectedLiveSession != null)
            {
                IsBusy = true;
                SelectedSession = null;
                await SelectSession(SelectedLiveSession);
                IsBusy = false;
            }
        }

        private async void SessionSelectionChangedExecute()
        {
            if (SelectedSession != null)
            {
                IsBusy = true;
                SelectedLiveSession = null;
                await SelectSession(SelectedSession);
                IsBusy = false;
            }
        }

        private async Task SelectSession(Session session)
        {
            SelectedVodType = null;
            ClearChannels();
            ClearEpisodes();

            var channels = await _apiService.GetChannelsAsync(session.UID);
            Channels.AddRange(channels.OrderBy(c => c.Name, new ChannelComparer()));

            if (session.ContentUrls.Any())
            {
                var episodes = new ConcurrentBag<Episode>();
                var tasks = session.ContentUrls.Select(async episodeUrl =>
                {
                    episodes.Add(await _apiService.GetEpisodeAsync(episodeUrl.GetUID()));
                });
                await Task.WhenAll(tasks);
                Episodes.AddRange(episodes.OrderBy(e => e.Title));
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
                    var episodes = new ConcurrentBag<Episode>();
                    var tasks = SelectedVodType.ContentUrls.Select(async episodeUrl =>
                    {
                        episodes.Add(await _apiService.GetEpisodeAsync(episodeUrl.GetUID()));
                    });
                    await Task.WhenAll(tasks);
                    Episodes.AddRange(episodes.OrderBy(e => e.Title));
                }

                IsBusy = false;
            }
        }

        private void WatchChannelExecute(Channel channel)
        {
            var session = GetCurrentSession();
            var parameters = new DialogParameters
            {
                { ParameterNames.Token, _token },
                { ParameterNames.ContentType, ContentType.Channel },
                { ParameterNames.ContentUrl, channel.Self },
                { ParameterNames.SyncUID, session.UID },
                { ParameterNames.Title, $"{session} - {channel}" },
                { ParameterNames.IsLive, session.IsLive },
                { ParameterNames.LowQualityMode, LowQualityMode },
                { ParameterNames.UseAlternativeStream, UseAlternativeStream }
            };

            _logger.Info($"Starting internal player for channel with parameters: '{parameters}'.");
            _dialogService.Show(nameof(VideoDialog), parameters, null, false);
        }

        private void WatchEpisodeExecute(Episode episode)
        {
            var parameters = new DialogParameters
            {
                { ParameterNames.Token, _token },
                { ParameterNames.ContentType, ContentType.Asset },
                { ParameterNames.ContentUrl, episode.Items.First() },
                { ParameterNames.SyncUID, episode.UID },
                { ParameterNames.Title, episode.ToString() },
                { ParameterNames.IsLive, false },
                { ParameterNames.LowQualityMode, false },
                { ParameterNames.UseAlternativeStream, false }
            };

            _logger.Info($"Starting internal player for episode with parameters: '{parameters}'.");
            _dialogService.Show(nameof(VideoDialog), parameters, null, false);
        }

        private bool CanWatchVlcChannelExecute(Channel channel)
        {
            return !string.IsNullOrWhiteSpace(VlcExeLocation) && File.Exists(VlcExeLocation);
        }

        private async void WatchVlcChannelExecute(Channel channel)
        {
            IsBusy = true;
            var session = GetCurrentSession();
            var title = $"{session} - {channel}";

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
            var title = $"{session} - {channel}";

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

        private async void RefreshLiveEventsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _refreshLiveEventsTimer.Stop();
            await RefreshLiveEvents();
            _refreshLiveEventsTimer.Start();
        }

        private async Task RefreshLiveEvents()
        {
            _logger.Info("Refreshing live events...");
            var liveEvents = await _apiService.GetLiveEventsAsync();
            var liveSessions = new List<Session>();

            foreach (var liveEvent in liveEvents)
            {
                foreach (var liveSessionUrl in liveEvent.SessionOccurrenceUrls)
                {
                    var liveSession = await _apiService.GetSessionAsync(liveSessionUrl.GetUID());

                    if (liveSession.IsLive)
                    {
                        liveSession.PrettyName = $"{liveEvent.Name} - {liveSession.Name}";
                        liveSessions.Add(liveSession);
                    }
                }
            }

            var sessionsToRemove = LiveSessions.Where(existingLiveSession => !liveSessions.Any(liveSession => liveSession.UID == existingLiveSession.UID)).ToList();
            var sessionsToAdd = liveSessions.Where(newLiveSession => !LiveSessions.Any(liveSession => liveSession.UID == newLiveSession.UID)).ToList();

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

            _logger.Info("Done refreshing live events.");
        }

        private void WatchStreamInVlc(string url, string title, bool isLive)
        {
            if (isLive)
            {
                _streamlinkLauncher.StartStreamlinkVlc(VlcExeLocation, url, LowQualityMode, UseAlternativeStream);
            }
            else
            {
                ProcessUtils.StartProcess(VlcExeLocation, $"{url} --meta-title=\"{title}\"");
            }
        }

        private void WatchStreamInMpv(string url, string title, bool isLive)
        {
            if (isLive)
            {
                _streamlinkLauncher.StartStreamlinkMpv(MpvExeLocation, url, LowQualityMode, UseAlternativeStream);
            }
            else
            {
                ProcessUtils.StartProcess(MpvExeLocation, $"{url} --title=\"{title}\"");
            }
        }

        private void ClearEvents()
        {
            Events.Clear();
            ClearSessions();
        }

        private void ClearSessions()
        {
            SelectedLiveSession = null;
            Sessions.Clear();
            ClearChannels();
        }

        private void ClearChannels()
        {
            Channels.Clear();
        }

        private void ClearEpisodes()
        {
            Episodes.Clear();
        }
    }
}