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

        public override string Title => "Login";

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

        private async Task<AuthResponse> LoginAsync()
        {
            return await _authorizationService.AuthenticateAsync(Email, Password);
        }

        private void LoginSuccess(AuthResponse authResponse)
        {
            Logger.Info("Login successful.");
            Error = null;
            IsBusy = false;
            _credentialService.SaveCredential(Email, Password);

            RaiseRequestClose(ButtonResult.OK, new DialogParameters
            {
                { ParameterNames.SubscriptionToken, authResponse.Data.SubscriptionToken },
                { ParameterNames.SubscriptionStatus, authResponse.Data.SubscriptionStatus }
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