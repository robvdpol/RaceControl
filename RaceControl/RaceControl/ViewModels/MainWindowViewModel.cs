using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RaceControl.Common;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Views;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;
        private readonly IApiService _apiService;

        private ICommand _loginCommand;
        private ICommand _playCommand;

        private string _token;

        public MainWindowViewModel(IDialogService dialogService, IApiService apiService)
        {
            _dialogService = dialogService;
            _apiService = apiService;
        }

        public string Title => "Race Control";

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(LoginExecute));
        public ICommand PlayCommand => _playCommand ?? (_playCommand = new DelegateCommand(PlayExecute));

        private void LoginExecute()
        {
            _dialogService.ShowDialog(nameof(LoginDialog), null, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    _token = r.Parameters.GetValue<string>("token");
                }
            });
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
                            var url = await _apiService.GetTokenisedUrlForChannelAsync(_token, channel.Self);
                            var parameters = new DialogParameters
                            {
                                { "url", url }
                            };

                            _dialogService.Show(nameof(VideoDialog), parameters, r =>
                            {
                            });

                            return;
                        }
                    }
                }
            }
        }
    }
}