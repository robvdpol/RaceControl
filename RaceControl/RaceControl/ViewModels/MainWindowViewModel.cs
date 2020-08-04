using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RaceControl.Common;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Views;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly IApiService _apiService;

        private ICommand _loadedCommand;
        private ICommand _seasonSelectionChangedCommand;
        private ICommand _eventSelectionChangedCommand;
        private ICommand _sessionSelectionChangedCommand;
        private ICommand _watchChannelCommand;
        private ICommand _vodTypeSelectionChangedCommand;
        private ICommand _watchEpisodeCommand;

        private bool _loaded;
        private string _token;
        private ObservableCollection<Season> _seasons;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Session> _sessions;
        private ObservableCollection<Channel> _channels;
        private ObservableCollection<VodType> _vodTypes;
        private ObservableCollection<Episode> _episodes;
        private Season _selectedSeason;
        private Event _selectedEvent;
        private Session _selectedSession;
        private VodType _selectedVodType;

        public MainWindowViewModel(IDialogService dialogService, IApiService apiService)
        {
            _dialogService = dialogService;
            _apiService = apiService;
        }

        public string Title => "Race Control";

        public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
        public ICommand SeasonSelectionChangedCommand => _seasonSelectionChangedCommand ??= new DelegateCommand(SeasonSelectionChangedExecute);
        public ICommand EventSelectionChangedCommand => _eventSelectionChangedCommand ??= new DelegateCommand(EventSelectionChangedExecute);
        public ICommand SessionSelectionChangedCommand => _sessionSelectionChangedCommand ??= new DelegateCommand(SessionSelectionChangedExecute);
        public ICommand WatchChannelCommand => _watchChannelCommand ??= new DelegateCommand<Channel>(WatchChannelExecute);
        public ICommand VodTypeSelectionChangedCommand => _vodTypeSelectionChangedCommand ??= new DelegateCommand(VodTypeSelectionChangedExecute);
        public ICommand WatchEpisodeCommand => _watchEpisodeCommand ??= new DelegateCommand<Episode>(WatchEpisodeExecute);

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

                await Initialize();
            }
        }

        private async Task Initialize()
        {
            Seasons.Clear();
            VodTypes.Clear();
            Seasons.AddRange((await _apiService.GetRaceSeasonsAsync()).Where(s => s.EventOccurrenceUrls.Any()));
            VodTypes.AddRange((await _apiService.GetVodTypesAsync()).Where(v => v.ContentUrls.Any()));
        }

        private async void SeasonSelectionChangedExecute()
        {
            ClearEvents();

            if (SelectedSeason != null)
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

            if (SelectedEvent != null)
            {
                var sessions = new ConcurrentBag<Session>();
                var tasks = SelectedEvent.SessionOccurrenceUrls.Select(async sessionUrl =>
                {
                    sessions.Add(await _apiService.GetSessionAsync(sessionUrl.GetUID()));
                });
                await Task.WhenAll(tasks);
                Sessions.AddRange(sessions.OrderBy(s => s.StartTime));
            }
        }

        private async void SessionSelectionChangedExecute()
        {
            ClearChannels();

            if (SelectedSession != null)
            {
                Channels.AddRange(await _apiService.GetChannelsAsync(SelectedSession.UID));
            }
        }

        private void WatchChannelExecute(Channel channel)
        {
            var parameters = new DialogParameters
            {
                { "token", _token },
                { "session", SelectedSession },
                { "channel", channel }
            };

            _dialogService.Show(nameof(VideoDialog), parameters, null);
        }

        private async void VodTypeSelectionChangedExecute()
        {
            ClearEpisodes();

            if (SelectedVodType != null)
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

        private void WatchEpisodeExecute(Episode episode)
        {
            var parameters = new DialogParameters
            {
                { "token", _token },
                { "vodtype", SelectedVodType },
                { "episode", episode }
            };

            _dialogService.Show(nameof(VideoDialog), parameters, null);
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