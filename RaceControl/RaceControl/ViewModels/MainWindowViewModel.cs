using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RaceControl.Common;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Api;
using RaceControl.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private bool _loaded;
        private string _token;
        private ObservableCollection<Season> _seasons;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Session> _sessions;
        private ObservableCollection<Channel> _channels;
        private Season _selectedSeason;
        private Event _selectedEvent;
        private Session _selectedSession;
        private Channel _selectedChannel;

        public MainWindowViewModel(IDialogService dialogService, IApiService apiService)
        {
            _dialogService = dialogService;
            _apiService = apiService;
        }

        public string Title => "Race Control";

        public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
        public ICommand SeasonSelectionChangedCommand => _seasonSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(SeasonSelectionChangedExecute);
        public ICommand EventSelectionChangedCommand => _eventSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(EventSelectionChangedExecute);
        public ICommand SessionSelectionChangedCommand => _sessionSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(SessionSelectionChangedExecute);

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

        public Channel SelectedChannel
        {
            get => _selectedChannel;
            set => SetProperty(ref _selectedChannel, value);
        }

        private void LoadedExecute(RoutedEventArgs args)
        {
            if (!_loaded)
            {
                _loaded = true;

                _dialogService.ShowDialog(nameof(LoginDialog), null, async r =>
                {
                    _token = r.Parameters.GetValue<string>("token");
                    await Initialize();
                });
            }
        }

        private async Task Initialize()
        {
            Seasons.Clear();
            Seasons.AddRange((await _apiService.GetRaceSeasonsAsync()).OrderByDescending(s => s.Year));
        }

        private async void SeasonSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            ClearEvents();

            if (SelectedSeason != null)
            {
                foreach (var eventUrl in SelectedSeason.EventOccurrenceUrls)
                {
                    Events.Add(await _apiService.GetEventAsync(eventUrl.GetUID()));
                }
            }
        }

        private async void EventSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            ClearSessions();

            if (SelectedEvent != null)
            {
                foreach (var sessionUrl in SelectedEvent.SessionOccurrenceUrls)
                {
                    Sessions.Add(await _apiService.GetSessionAsync(sessionUrl.GetUID()));
                }
            }
        }

        private async void SessionSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            ClearChannels();

            if (SelectedSession != null)
            {
                Channels.AddRange(await _apiService.GetChannelsAsync(SelectedSession.UID));
            }
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

        /*
        private async void PlayExecute()
        {
            var seasons = await _apiService.GetRaceSeasonsAsync();

            foreach (var season in seasons)
            {
                foreach (var eventUrl in season.EventOccurrenceUrls)
                {
                    var eventId = eventUrl.GetUID();
                    var eventObj = await _apiService.GetEventAsync(eventId);

                    foreach (var sessionUrl in eventObj.SessionOccurrenceUrls)
                    {
                        var sessionId = sessionUrl.GetUID();
                        var channels = await _apiService.GetChannelsAsync(sessionId);

                        foreach (var channel in channels)
                        {
                            var parameters = new DialogParameters
                            {
                                { "token", _token },
                                { "channel", channel }
                            };

                            _dialogService.Show(nameof(VideoDialog), parameters, null);

                            return;
                        }
                    }
                }
            }
        }
        */
    }
}