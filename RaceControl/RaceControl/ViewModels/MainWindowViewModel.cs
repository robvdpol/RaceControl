using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RaceControl.Common;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Views;
using System.Windows;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly IApiService _apiService;

        private ICommand _loadedCommand;
        private ICommand _playCommand;

        private bool _loaded;
        private string _token;

        public MainWindowViewModel(IDialogService dialogService, IApiService apiService)
        {
            _dialogService = dialogService;
            _apiService = apiService;
        }

        public string Title => "Race Control";

        public ICommand LoadedCommand => _loadedCommand ??= new DelegateCommand<RoutedEventArgs>(LoadedExecute);
        public ICommand PlayCommand => _playCommand ??= new DelegateCommand(PlayExecute);

        private void LoadedExecute(RoutedEventArgs args)
        {
            if (!_loaded)
            {
                _loaded = true;

                _dialogService.ShowDialog(nameof(LoginDialog), null, r =>
                {
                    var token = r.Parameters.GetValue<string>("token");
                    Initialize(token);
                });
            }
        }

        private void Initialize(string token)
        {
            _token = token;
        }

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
    }
}