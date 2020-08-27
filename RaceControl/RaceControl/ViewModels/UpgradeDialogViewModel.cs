using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.Github;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class UpgradeDialogViewModel : DialogViewModelBase
    {
        private ICommand _closeCommand;
        private Release _release;

        public UpgradeDialogViewModel(ILogger logger) : base(logger)
        {
        }

        public ICommand CloseCommand => _closeCommand ??= new DelegateCommand<bool?>(CloseExecute);

        public Release Release
        {
            get => _release;
            set => SetProperty(ref _release, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Release = parameters.GetValue<Release>(ParameterNames.RELEASE);
            Title = Release.Name;

            base.OnDialogOpened(parameters);
        }

        private void CloseExecute(bool? upgrade)
        {
            RaiseRequestClose(new DialogResult(upgrade.HasValue && upgrade.Value ? ButtonResult.OK : ButtonResult.Cancel));
        }
    }
}