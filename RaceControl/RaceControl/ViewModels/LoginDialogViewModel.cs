using CredentialManagement;
using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Windows.Input;
using DialogResult = Prism.Services.Dialogs.DialogResult;

namespace RaceControl.ViewModels
{
    public class LoginDialogViewModel : DialogViewModelBase
    {
        private const string RaceControlF1TV = "RaceControlF1TV";

        private readonly IAuthorizationService _authorizationService;

        private ICommand _loginCommand;

        private string _email;
        private string _password;
        private string _error;
        private bool _canClose;

        public LoginDialogViewModel(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public override string Title => "Login to F1TV";

        public ICommand LoginCommand => _loginCommand ??= new DelegateCommand(LoginExecute, LoginCanExecute).ObservesProperty(() => Email).ObservesProperty(() => Password);

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

        public bool CanClose
        {
            get => _canClose;
            set => SetProperty(ref _canClose, value);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (LoadCredential() && LoginCanExecute())
            {
                LoginExecute();
            }
        }

        public override bool CanCloseDialog()
        {
            return CanClose;
        }

        private bool LoginCanExecute()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
        }

        private async void LoginExecute()
        {
            Error = null;
            string token;

            try
            {
                token = (await _authorizationService.LoginAsync(Email, Password)).Token;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return;
            }

            SaveCredential();
            CanClose = true;
            RaiseRequestClose(new DialogResult(ButtonResult.OK, new DialogParameters
            {
                { nameof(token), token }
            }));
        }

        private bool LoadCredential()
        {
            using (var cred = new Credential())
            {
                cred.Target = RaceControlF1TV;

                if (cred.Load())
                {
                    Email = cred.Username;
                    Password = cred.Password;

                    return true;
                }
            }

            return false;
        }

        private void SaveCredential()
        {
            using (var cred = new Credential())
            {
                cred.Target = RaceControlF1TV;
                cred.Username = Email;
                cred.Password = Password;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;

                try
                {
                    cred.Save();
                }
                catch
                {
                    // Just continue if credentials cannot be stored
                }
            }
        }
    }
}