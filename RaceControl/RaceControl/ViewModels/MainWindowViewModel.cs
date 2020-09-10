using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Common.Utils;
using RaceControl.Comparers;
using RaceControl.Core.Helpers;
using RaceControl.Core.Mvvm;
using RaceControl.Core.Settings;
using RaceControl.Core.Streamlink;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Services.Interfaces.Github;
using RaceControl.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    // ReSharper disable once UnusedType.Global
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
        private ICommand _watchContentCommand;
        private ICommand _watchContentInVlcCommand;
        private ICommand _watchContentInMpvCommand;
        private ICommand _copyContentUrlCommand;
        private ICommand _downloadContentCommand;
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
        private ObservableCollection<IPlayableContent> _channels;
        private ObservableCollection<VodType> _vodTypes;
        private ObservableCollection<IPlayableContent> _episodes;
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
        public ICommand WatchContentCommand => _watchContentCommand ??= new DelegateCommand<IPlayableContent>(WatchContentExecute);
        public ICommand WatchContentInVlcCommand => _watchContentInVlcCommand ??= new DelegateCommand<IPlayableContent>(WatchContentInVlcExecute, CanWatchContentInVlcExecute).ObservesProperty(() => VlcExeLocation);
        public ICommand WatchContentInMpvCommand => _watchContentInMpvCommand ??= new DelegateCommand<IPlayableContent>(WatchContentInMpvExecute, CanWatchContentInMpvExecute).ObservesProperty(() => MpvExeLocation);
        public ICommand CopyContentUrlCommand => _copyContentUrlCommand ??= new DelegateCommand<IPlayableContent>(CopyContentUrlExecute);
        public ICommand DownloadContentCommand => _downloadContentCommand ??= new DelegateCommand<IPlayableContent>(DownloadContentExecute, CanDownloadContentExecute);
        public ICommand SetRecordingLocationCommand => _setRecordingLocationCommand ??= new DelegateCommand(SetRecordingLocationExecute);
        public ICommand SaveVideoDialogLayoutCommand => _saveVideoDialogLayoutCommand ??= new DelegateCommand(SaveVideoDialogLayoutExecute, CanSaveVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogViewModels.Count);
        public ICommand OpenVideoDialogLayoutCommand => _openVideoDialogLayoutCommand ??= new DelegateCommand(OpenVideoDialogLayoutExecute, CanOpenVideoDialogLayoutExecute).ObservesProperty(() => VideoDialogLayout.Instances.Count).ObservesProperty(() => Channels.Count);
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

        public ObservableCollection<IPlayableContent> Channels
        {
            get => _channels ??= new ObservableCollection<IPlayableContent>();
            set => SetProperty(ref _channels, value);
        }

        public ObservableCollection<VodType> VodTypes
        {
            get => _vodTypes ??= new ObservableCollection<VodType>();
            set => SetProperty(ref _vodTypes, value);
        }

        public ObservableCollection<IPlayableContent> Episodes
        {
            get => _episodes ??= new ObservableCollection<IPlayableContent>();
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

        private void LoadedExecute(RoutedEventArgs args)
        {
            Logger.Info("Initializing application...");
            IsBusy = true;
            Settings.Load();
            VideoDialogLayout.Load();
            SetVlcExeLocation();
            SetMpvExeLocation();
            CheckForUpdatesAsync().Await(HandleNonCriticalError);

            if (Login())
            {
                InitializeAsync().Await(() =>
                {
                    SetNotBusy();
                    SelectedSeason = Seasons.FirstOrDefault();
                },
                HandleCriticalError);
                RefreshLiveSessionsAsync().Await(HandleNonCriticalError);
            }
        }

        private void ClosingExecute()
        {
            if (_refreshLiveSessionsTimer != null)
            {
                _refreshLiveSessionsTimer.Stop();
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

        private void SeasonSelectionChangedExecute()
        {
            ClearEvents();

            if (SelectedSeason != null)
            {
                IsBusy = true;

                _apiService
                    .GetEventsForSeasonAsync(SelectedSeason.UID)
                    .Await(events =>
                    {
                        Events.AddRange(events);
                        SetNotBusy();
                    },
                    HandleCriticalError,
                    true);
            }
        }

        private void EventSelectionChangedExecute()
        {
            ClearSessions();

            if (SelectedEvent != null)
            {
                IsBusy = true;

                _apiService
                    .GetSessionsForEventAsync(SelectedEvent.UID)
                    .Await(sessions =>
                    {
                        Sessions.AddRange(sessions);
                        SetNotBusy();
                    },
                    HandleCriticalError,
                    true);
            }
        }

        private void LiveSessionSelectionChangedExecute()
        {
            if (SelectedLiveSession != null)
            {
                IsBusy = true;
                SelectedSession = null;
                SelectSessionAsync(SelectedLiveSession).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void SessionSelectionChangedExecute()
        {
            if (SelectedSession != null)
            {
                IsBusy = true;
                SelectedLiveSession = null;
                SelectSessionAsync(SelectedSession).Await(SetNotBusy, HandleCriticalError);
            }
        }

        private void VodTypeSelectionChangedExecute()
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

                    DownloadHelper
                        .BufferedDownload(_apiService.GetEpisodeAsync, SelectedVodType.ContentUrls.Select(c => c.GetUID()))
                        .Await(episodes =>
                        {
                            Episodes.AddRange(episodes.OrderBy(e => e.Title).Select(PlayableContent.Create));
                            SetNotBusy();
                        },
                        HandleCriticalError,
                        true);
                }
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
            VideoDialogLayout.Add(VideoDialogViewModels.Where(vm => vm.PlayableContent.ContentType == ContentType.Channel).Select(vm => vm.GetDialogSettings()));

            if (VideoDialogLayout.Save())
            {
                MessageBoxHelper.ShowInfo("The current window layout has been successfully saved.", "Layout");
            }
        }

        private bool CanOpenVideoDialogLayoutExecute()
        {
            return VideoDialogLayout.Instances.Any() && Channels.Count > 1;
        }

        private void OpenVideoDialogLayoutExecute()
        {
            var viewModelsToClose = VideoDialogViewModels.Where(vm => vm.PlayableContent.ContentType == ContentType.Channel).ToList();

            foreach (var viewModel in viewModelsToClose)
            {
                viewModel.CloseWindow();
            }

            foreach (var settings in VideoDialogLayout.Instances)
            {
                var playableContent = Channels.FirstOrDefault(p => p.ContentType == ContentType.Channel && p.Name == settings.ChannelName);

                if (playableContent != null)
                {
                    WatchContent(playableContent, settings);
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

        private void RefreshLiveSessionsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _refreshLiveSessionsTimer?.Stop();
            RefreshLiveSessionsAsync().Await(HandleNonCriticalError);
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
                LoadVodTypesAsync());

            _refreshLiveSessionsTimer = new Timer(60000) { AutoReset = false };
            _refreshLiveSessionsTimer.Elapsed += RefreshLiveSessionsTimer_Elapsed;
            _refreshLiveSessionsTimer.Start();
        }

        private async Task LoadSeasonsAsync()
        {
            var seasons = await _apiService.GetSeasonsAsync();
            Seasons.AddRange(seasons);
        }

        private async Task LoadSeriesAsync()
        {
            var series = await _apiService.GetSeriesAsync();
            Series.AddRange(series);

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
            var vodTypes = await _apiService.GetVodTypesAsync();
            VodTypes.AddRange(vodTypes.Where(vt => vt.ContentUrls.Any()));
        }

        private async Task RefreshLiveSessionsAsync()
        {
            Logger.Info("Refreshing live sessions...");

            var liveSessions = (await _apiService.GetLiveSessionsAsync()).Where(session => session.IsLive).ToList();
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
        }

        private async Task SelectSessionAsync(Session session)
        {
            Episodes.Clear();
            Channels.Clear();
            SelectedVodType = null;

            await Task.WhenAll(
                LoadChannelsForSessionAsync(session),
                LoadEpisodesForSessionAsync(session));
        }

        private async Task LoadChannelsForSessionAsync(Session session)
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

            Channels.AddRange(channels.OrderBy(c => c.ChannelType, new ChannelTypeComparer()).Select(c => PlayableContent.Create(session, c)));
        }

        private async Task LoadEpisodesForSessionAsync(Session session)
        {
            var episodes = await _apiService.GetEpisodesForSessionAsync(session.UID);
            Episodes.AddRange(episodes.OrderBy(e => e.Title).Select(PlayableContent.Create));
        }

        private void WatchContent(IPlayableContent playableContent, VideoDialogSettings settings = null)
        {
            var parameters = new DialogParameters
            {
                { ParameterNames.TOKEN, _token },
                { ParameterNames.CONTENT, playableContent },
                { ParameterNames.SETTINGS, settings }
            };

            var viewModel = (IVideoDialogViewModel)_dialogService.Show(nameof(VideoDialog), parameters, OnVideoDialogClosed, false, nameof(VideoDialogWindow));
            VideoDialogViewModels.Add(viewModel);
        }

        private void OnVideoDialogClosed(IDialogResult result)
        {
            var uniqueIdentifier = result.Parameters.GetValue<Guid>(ParameterNames.IDENTIFIER);
            var viewModel = VideoDialogViewModels.FirstOrDefault(vm => vm.UniqueIdentifier == uniqueIdentifier);

            if (viewModel != null)
            {
                VideoDialogViewModels.Remove(viewModel);
            }
        }

        private async Task WatchInVlcAsync(IPlayableContent playableContent)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);

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

        private async Task WatchInMpvAsync(IPlayableContent playableContent)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
            using var process = ProcessUtils.CreateProcess(MpvExeLocation, $"\"{streamUrl}\" --title=\"{playableContent.Title}\"");
            process.Start();
        }

        private async Task CopyUrlAsync(IPlayableContent playableContent)
        {
            var streamUrl = await _apiService.GetTokenisedUrlAsync(_token, playableContent);
            Clipboard.SetText(streamUrl);
        }

        private void StartDownload(IPlayableContent playableContent)
        {
            var defaultFilename = $"{playableContent.Title}.ts".RemoveInvalidFileNameChars();

            if (_dialogService.SelectFile("Select a filename", Settings.RecordingLocation, defaultFilename, ".ts", out var filename))
            {
                var parameters = new DialogParameters
                {
                    { ParameterNames.TOKEN, _token },
                    { ParameterNames.CONTENT, playableContent},
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
    }
}