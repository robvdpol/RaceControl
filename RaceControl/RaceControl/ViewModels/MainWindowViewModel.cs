using Prism.Mvvm;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Race Control";

        public MainWindowViewModel()
        {
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}