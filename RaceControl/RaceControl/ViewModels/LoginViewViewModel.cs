using Prism.Commands;
using Prism.Services.Dialogs;
using RaceControl.Core.Mvvm;
using RaceControl.Services.Interfaces.F1TV;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaceControl.ViewModels
{
    public class LoginViewViewModel : DialogViewModelBase, IDialogAware
    {
        private readonly IAuthorizationService _authorizationService;

        private ICommand _loginCommand;
        private ICommand _passwordChangedCommand;

        private string _email;
        private string _password;
        private string _error;
        private bool _canClose;

        public LoginViewViewModel(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public override string Title => "Login to F1TV";

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(LoginExecute, LoginCanExecute).ObservesProperty(() => Email).ObservesProperty(() => Password));
        public ICommand PasswordChangedCommand => _passwordChangedCommand ?? (_passwordChangedCommand = new DelegateCommand<RoutedEventArgs>(PasswordChangedExecute));

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

        public override bool CanCloseDialog()
        {
            return _canClose;
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

            var parameters = new DialogParameters
            {
                { "token", token }
            };
            _canClose = true;
            RaiseRequestClose(new DialogResult(ButtonResult.OK, parameters));
        }

        private void PasswordChangedExecute(RoutedEventArgs args)
        {
            if (args.Source is PasswordBox box)
            {
                Password = box.Password;
            }
        }
    }
}