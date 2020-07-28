using Prism.Regions;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;

namespace RaceControl.Modules.ModuleName.ViewModels
{
    public class ViewAViewModel : RegionViewModelBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IApiService _apiService;

        private string _message;

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ViewAViewModel(IRegionManager regionManager, IAuthorizationService authorizationService, IApiService apiService) :
            base(regionManager)
        {
            _authorizationService = authorizationService;
            _apiService = apiService;
            Message = "Testmessage";
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            var token = await _authorizationService.LoginAsync("rob_van_der_pol@hotmail.com", "yBz5XfSeacQKVq9");
            var seasons = await _apiService.GetRaceSeasonsAsync();
        }
    }
}