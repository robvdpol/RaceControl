using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DialogResult = Prism.Services.Dialogs.DialogResult;

namespace RaceControl.ViewModels
{
    public class LoginDialogViewModel : DialogViewModelBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICredentialService _credentialService;

        private ICommand _loginCommand;

        private string _token;
        private string _email;
        private string _password;
        private string _error;

        public LoginDialogViewModel(ILogger logger, IAuthorizationService authorizationService, ICredentialService credentialService) : base(logger)
        {
            _authorizationService = authorizationService;
            _credentialService = credentialService;
        }

        public override string Title { get; } = "Login";

        public ICommand LoginCommand => _loginCommand ??= new DelegateCommand(LoginExecute, CanLoginExecute).ObservesProperty(() => IsBusy).ObservesProperty(() => Email).ObservesProperty(() => Password);

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            CanClose = true;

            if (_credentialService.LoadCredential(out var email, out var password))
            {
                Email = email;
                Password = password;

                if (CanLoginExecute())
                {
                    LoginExecute();
                }
            }

            base.OnDialogOpened(parameters);
        }

        private bool CanLoginExecute()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void LoginExecute()
        {
            IsBusy = true;
            Login().Await(LoginSuccess, LoginError);
        }

        private async Task Login()
        {
            Logger.Info("Attempting to login...");
            _token = (await _authorizationService.LoginAsync(Email, Password)).Token;
            _credentialService.SaveCredential(Email, Password);
        }

        private void LoginSuccess()
        {
            Logger.Info("Login successful.");
            Error = null;
            IsBusy = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                RaiseRequestClose(new DialogResult(ButtonResult.OK, new DialogParameters { { ParameterNames.TOKEN, _token } }));
            });
        }

        private void LoginError(Exception ex)
        {
            Logger.Error(ex, "Login failed.");
            Error = ex.Message;
            IsBusy = false;
        }
    }
}