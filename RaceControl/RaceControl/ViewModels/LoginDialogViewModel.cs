using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Extensions;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class LoginDialogViewModel : DialogViewModelBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICredentialService _credentialService;

        private ICommand _loginCommand;

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
            base.OnDialogOpened(parameters);

            if (_credentialService.LoadCredential(out var email, out var password))
            {
                Email = email;
                Password = password;
                LoginCommand.TryExecute();
            }
        }

        private bool CanLoginExecute()
        {
            return !IsBusy && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private void LoginExecute()
        {
            Error = null;
            IsBusy = true;
            LoginAsync().Await(LoginSuccess, LoginError, true);
        }

        private async Task<string> LoginAsync()
        {
            return (await _authorizationService.AuthenticateAsync(Email, Password)).Data.SubscriptionToken;
        }

        private void LoginSuccess(string token)
        {
            Logger.Info("Login successful.");
            Error = null;
            IsBusy = false;
            _credentialService.SaveCredential(Email, Password);
            RaiseRequestClose(ButtonResult.OK, new DialogParameters { { ParameterNames.Token, token } });
        }

        private void LoginError(Exception ex)
        {
            Logger.Error(ex, "Login failed.");
            Error = ex.Message;
            IsBusy = false;
        }
    }
}