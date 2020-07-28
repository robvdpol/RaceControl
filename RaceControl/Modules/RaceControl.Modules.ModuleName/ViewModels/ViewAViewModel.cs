using Prism.Regions;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;

namespace RaceControl.Modules.ModuleName.ViewModels
{
    public class ViewAViewModel : RegionViewModelBase
    {
        private string _message;

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ViewAViewModel(IRegionManager regionManager, IAuthorizationService authorizationService) :
            base(regionManager)
        {
            Message = "Testmessage";
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            //do something
        }
    }
}