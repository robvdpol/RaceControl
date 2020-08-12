using Prism.Mvvm;

namespace RaceControl.Core.Mvvm
{
    public class ViewModelBase : BindableBase
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}