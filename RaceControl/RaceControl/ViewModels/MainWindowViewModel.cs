using LibVLCSharp.Shared;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RaceControl.Common;
using RaceControl.Comparers;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly IApiService _apiService;
        private readonly LibVLC _libVLC;
        private readonly Timer _refreshLiveEventsTimer;

        private ICommand _loadedCommand;
        private ICommand _closingCommand;
        private ICommand _seasonSelectionChangedCommand;
        private ICommand _eventSelectionChangedCommand;
        private ICommand _sessionSelectionChangedCommand;
        private ICommand _vodTypeSelectionChangedCommand;
        private ICommand _watchChannelCommand;
        private ICommand _watchEpisodeCommand;
        private ICommand _watchVlcChannelCommand;
        private ICommand _watchVlcEpisodeCommand;
        private ICommand _copyUrlChannelCommand;
        private ICommand _copyUrlEpisodeCommand;

        private bool _loaded;
        private string _token;
        private string _vlcExeLocation;
        private ObservableCollection<Season> _seasons;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Session> _sessions;
        private ObservableCollection<Session> _liveSessions;
        private ObservableCollection<Channel> _channels;
        private ObservableCollection<VodType> _vodTypes;
        private ObservableCollection<Episode> _episodes;
        private Season _selectedSeason;
        private Event _selectedEvent;
        private Session _selectedSession;
        private VodType _selectedVodType;

        public MainWindowViewModel(IDialogService dialogService, IApiService apiService, LibVLC libVLC)
        {
            _dialogService = dialogService;
            _apiService = apiService;
            _libVLC = libVLC;
            _refreshLiveEventsTimer = new Timer(60000) { AutoReset = false };
        }

        public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
        public ICommand ClosingCommand => _closingCommand ??= new DelegateCommand(ClosingExecute);
        public ICommand SeasonSelectionChangedCommand => _seasonSelectionChangedCommand ??= new DelegateCommand(SeasonSelectionChangedExecute);
        public ICommand EventSelectionChangedCommand => _eventSelectionChangedCommand ??= new DelegateCommand(EventSelectionChangedExecute);
        public ICommand SessionSelectionChangedCommand => _sessionSelectionChangedCommand ??= new DelegateCommand(SessionSelectionChangedExecute);
        public ICommand VodTypeSelectionChangedCommand => _vodTypeSelectionChangedCommand ??= new DelegateCommand(VodTypeSelectionChangedExecute);
        public ICommand WatchChannelCommand => _watchChannelCommand ??= new DelegateCommand<Channel>(WatchChannelExecute);
        public ICommand WatchEpisodeCommand => _watchEpisodeCommand ??= new DelegateCommand<Episode>(WatchEpisodeExecute);
        public ICommand WatchVlcChannelCommand => _watchVlcChannelCommand ??= new DelegateCommand<Channel>(WatchVlcChannelExecute, CanWatchVlcChannelExecute).ObservesProperty(() => VlcExeLocation);
        public ICommand WatchVlcEpisodeCommand => _watchVlcEpisodeCommand ??= new DelegateCommand<Episode>(WatchVlcEpisodeExecute, CanWatchVlcEpisodeExecute).ObservesProperty(() => VlcExeLocation);
        public ICommand CopyUrlChannelCommand => _copyUrlChannelCommand ??= new DelegateCommand<Channel>(CopyUrlChannelExecute);
        public ICommand CopyUrlEpisodeCommand => _copyUrlEpisodeCommand ??= new DelegateCommand<Episode>(CopyUrlEpisodeExecute);

        public string VlcExeLocation
        {
            get => _vlcExeLocation;
            set => SetProperty(ref _vlcExeLocation, value);
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
            if (!_loaded)
            {
                _loaded = true;

                _dialogService.ShowDialog(nameof(LoginDialog), null, dialogResult =>
                {
                    if (dialogResult.Result == ButtonResult.OK)
                    {
                        _token = dialogResult.Parameters.GetValue<string>("token");
                    }
                });

                if (string.IsNullOrWhiteSpace(_token))
                {
                    if (args.Source is Window window)
                    {
                        window.Close();
                    }

                    return;
                }

                SetVlcExeLocation();
                await Initialize();
            }
        }

        private void SetVlcExeLocation()
        {
            var vlcRegistryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VideoLAN\VLC") ?? Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\VideoLAN\VLC");

            if (vlcRegistryKey != null)
            {
                VlcExeLocation = vlcRegistryKey.GetValue(null) as string;
            }
        }

        private async Task Initialize()
        {
            Seasons.AddRange((await _apiService.GetRaceSeasonsAsync()).Where(s => s.EventOccurrenceUrls.Any()));
            VodTypes.AddRange((await _apiService.GetVodTypesAsync()).Where(v => v.ContentUrls.Any()));
            await RefreshLiveEvents();
            _refreshLiveEventsTimer.Elapsed += RefreshLiveEventsTimer_Elapsed;
            _refreshLiveEventsTimer.Start();
        }

        private void ClosingExecute()
        {
            _refreshLiveEventsTimer.Elapsed -= RefreshLiveEventsTimer_Elapsed;
            _refreshLiveEventsTimer.Stop();
            _refreshLiveEventsTimer.Dispose();
        }

        private async void SeasonSelectionChangedExecute()
        {
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
        }

        private async void EventSelectionChangedExecute()
        {
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
        }

        private async void SessionSelectionChangedExecute()
        {
            SelectedVodType = null;
            ClearChannels();
            ClearEpisodes();

            if (SelectedSession != null)
            {
                var channels = await _apiService.GetChannelsAsync(SelectedSession.UID);
                Channels.AddRange(channels.OrderBy(c => c.Name, new ChannelComparer()));

                if (SelectedSession.ContentUrls.Any())
                {
                    var episodes = new ConcurrentBag<Episode>();
                    var tasks = SelectedSession.ContentUrls.Select(async episodeUrl =>
                    {
                        episodes.Add(await _apiService.GetEpisodeAsync(episodeUrl.GetUID()));
                    });
                    await Task.WhenAll(tasks);
                    Episodes.AddRange(episodes.OrderBy(e => e.Title));
                }
            }
        }

        private async void VodTypeSelectionChangedExecute()
        {
            SelectedSession = null;
            ClearEpisodes();
            ClearChannels();

            if (SelectedVodType != null && SelectedVodType.ContentUrls.Any())
            {
                var episodes = new ConcurrentBag<Episode>();
                var tasks = SelectedVodType.ContentUrls.Select(async episodeUrl =>
                {
                    episodes.Add(await _apiService.GetEpisodeAsync(episodeUrl.GetUID()));
                });
                await Task.WhenAll(tasks);
                Episodes.AddRange(episodes.OrderBy(e => e.Title));
            }
        }

        private void WatchChannelExecute(Channel channel)
        {
            Func<string, Task<string>> urlFunc = (channelUrl) => GetTokenisedUrlForChannelAsync(channelUrl);

            var parameters = new DialogParameters
            {
                { "urlfunc", urlFunc },
                { "url", channel.Self },
                { "syncuid", SelectedSession.UID },
                { "title", $"{SelectedSession} - {channel}" },
                { "islive", SelectedSession.IsLive }
            };

            _dialogService.Show(nameof(VideoDialog), parameters, null);
        }

        private void WatchEpisodeExecute(Episode episode)
        {
            Func<string, Task<string>> urlFunc = (assetUrl) => GetTokenisedUrlForAssetAsync(assetUrl);

            var parameters = new DialogParameters
            {
                { "urlfunc", urlFunc },
                { "url", episode.Items.First() },
                { "syncuid", episode.UID },
                { "title", episode.ToString() },
                { "islive", false }
            };

            _dialogService.Show(nameof(VideoDialog), parameters, null);
        }

        private bool CanWatchVlcChannelExecute(Channel channel)
        {
            return channel != null && !string.IsNullOrWhiteSpace(VlcExeLocation);
        }

        private async void WatchVlcChannelExecute(Channel channel)
        {
            var title = $"{SelectedSession} - {channel}";
            var url = await GetTokenisedUrlForChannelAsync(channel.Self);
            Process.Start(VlcExeLocation, $"{url} --meta-title=\"{title}\"");
        }

        private bool CanWatchVlcEpisodeExecute(Episode episode)
        {
            return episode != null && !string.IsNullOrWhiteSpace(VlcExeLocation);
        }

        private async void WatchVlcEpisodeExecute(Episode episode)
        {
            var title = episode.ToString();
            var url = await GetTokenisedUrlForAssetAsync(episode.Items.First());
            Process.Start(VlcExeLocation, $"{url} --meta-title=\"{title}\"");
        }

        private async void CopyUrlChannelExecute(Channel channel)
        {
            var url = await GetTokenisedUrlForChannelAsync(channel.Self);
            Clipboard.SetText(url);
        }

        private async void CopyUrlEpisodeExecute(Episode episode)
        {
            var url = await GetTokenisedUrlForAssetAsync(episode.Items.First());
            Clipboard.SetText(url);
        }

        private async void RefreshLiveEventsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _refreshLiveEventsTimer.Stop();
            await RefreshLiveEvents();
            _refreshLiveEventsTimer.Start();
        }

        private async Task RefreshLiveEvents()
        {
            var liveEvents = await _apiService.GetLiveEventsAsync();
            var liveSessions = new List<Session>();

            foreach (var liveEvent in liveEvents)
            {
                foreach (var liveSessionUrl in liveEvent.SessionOccurrenceUrls)
                {
                    var liveSession = await _apiService.GetSessionAsync(liveSessionUrl.GetUID());

                    if (liveSession.IsLive)
                    {
                        liveSession.Name = $"{liveEvent.Name} - {liveSession.Name}";
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
        }

        private async Task<string> GetTokenisedUrlForChannelAsync(string channelUrl)
        {
            return await _apiService.GetTokenisedUrlForChannelAsync(_token, channelUrl);
        }

        private async Task<string> GetTokenisedUrlForAssetAsync(string assetUrl)
        {
            return await _apiService.GetTokenisedUrlForAssetAsync(_token, assetUrl);
        }

        private void ClearEvents()
        {
            Events.Clear();
            ClearSessions();
        }

        private void ClearSessions()
        {
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