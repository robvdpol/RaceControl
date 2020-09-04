using NLog;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.Credential;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Services.Interfaces.F1TV.Authorization;
using System;
using System.Windows.Input;
using DialogResult = Prism.Services.Dialogs.DialogResult;

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

        public ICommand LoginCommand => _loginCommand ??= new DelegateCommand(LoginExecute, CanLoginExecute)
            .ObservesProperty(() => Email)
            .ObservesProperty(() => Password)
            .ObservesProperty(() => IsBusy);

        public override string Title { get; } = "Login";

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

                if (LoginCommand.CanExecute(null))
                {
                    LoginCommand.Execute(null);
                }
            }
        }

        private bool CanLoginExecute()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;
        }

        private async void LoginExecute()
        {
            IsBusy = true;
            Error = null;
            TokenResponse token;

            try
            {
                Logger.Info("Attempting to login...");
                token = await _authorizationService.LoginAsync(Email, Password);
                Logger.Info("Login successful.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Login failed.");
                Error = ex.Message;
                IsBusy = false;
                return;
            }

            _credentialService.SaveCredential(Email, Password);
            RaiseRequestClose(new DialogResult(ButtonResult.OK, new DialogParameters { { ParameterNames.TOKEN, token.Token } }));
            IsBusy = false;
        }
    }
}