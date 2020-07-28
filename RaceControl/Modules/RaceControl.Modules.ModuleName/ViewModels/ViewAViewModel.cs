using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Prism.Commands;
using Prism.Regions;
using RaceControl.Common;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;
using System.Windows.Input;

namespace RaceControl.Modules.ModuleName.ViewModels
{
    public class ViewAViewModel : RegionViewModelBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IApiService _apiService;

        private ICommand _playCommand;

        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;

        public ViewAViewModel(IRegionManager regionManager, IAuthorizationService authorizationService, IApiService apiService) :
            base(regionManager)
        {
            _authorizationService = authorizationService;
            _apiService = apiService;
        }

        public ICommand PlayCommand => _playCommand ?? (_playCommand = new DelegateCommand<VideoView>(PlayExecute));

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            LibVLCSharp.Shared.Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true,
                NetworkCaching = 10000
            };
        }

        private async void PlayExecute(VideoView videoView)
        {
            videoView.MediaPlayer = _mediaPlayer;

            var token = await _authorizationService.LoginAsync("rob_van_der_pol@hotmail.com", "yBz5XfSeacQKVq9");
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
                            var url = await _apiService.GetTokenisedUrlForChannelAsync(token.Token, channel.Self);
                            _mediaPlayer.Play(new Media(_libVLC, url, FromType.FromLocation));

                            return;
                        }
                    }
                }
            }
        }
    }
}