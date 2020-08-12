using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;

namespace RaceControl.Core.Mvvm
{
    public class ViewModelBase : BindableBase
    {
        private bool _isBusy;

        private ICommand _previewKeyDownCommand;
        private ICommand _keyDownCommand;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand PreviewKeyDownCommand => _previewKeyDownCommand ??= new DelegateCommand<KeyEventArgs>(PreviewKeyDownExecute);
        public ICommand KeyDownCommand => _keyDownCommand ??= new DelegateCommand<KeyEventArgs>(KeyDownExecute);

        private void PreviewKeyDownExecute(KeyEventArgs args)
        {
            if (IsBusy)
            {
                args.Handled = true;
            }
        }

        private void KeyDownExecute(KeyEventArgs args)
        {
            if (IsBusy)
            {
                args.Handled = true;
            }
        }
    }
}