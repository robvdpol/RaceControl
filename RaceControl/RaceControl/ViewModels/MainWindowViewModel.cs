using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RaceControl.Views;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IDialogService _dialogService;

        private ICommand _loginCommand;

        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(LoginExecute));

        public string Title => "Race Control";

        private void LoginExecute()
        {
            _dialogService.ShowDialog(nameof(LoginDialog), null, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var token = r.Parameters.GetValue<string>("token");
                }
            });
        }
    }
}