using NLog;
using Prism.Mvvm;

namespace RaceControl.Core.Mvvm
{
    public class ViewModelBase : BindableBase
    {
        protected readonly ILogger Logger;

        private bool _isBusy;

        protected ViewModelBase(ILogger logger)
        {
            Logger = logger;
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}